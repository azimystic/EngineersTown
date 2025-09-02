using EngineersTown.Data;
using EngineersTown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EngineersTown.Controllers
{
    public class PayrollReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PayrollReports
        public async Task<IActionResult> Index()
        {
            // Get all available months/years from PayrollReport table
            var availablePeriods = await _context.PayrollReports
                .Select(p => new { p.Month, p.Year })
                .Distinct()
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();

            ViewBag.AvailablePeriods = availablePeriods;
            return View();
        }

        // GET: PayrollReports/Generate
        public IActionResult Generate(int? month, int? year)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            ViewBag.Month = month ?? currentMonth;
            ViewBag.Year = year ?? currentYear;
            ViewBag.Months = Enumerable.Range(1, 12).Select(i => new { Value = i, Text = new DateTime(2000, i, 1).ToString("MMMM") });

            return View();
        }

        // POST: PayrollReports/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(int month, int year)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Check if data exists for this period
            var existingData = await _context.PayrollReports
                .Where(p => p.Month == month && p.Year == year)
                .AnyAsync();

            if (existingData && (month != currentMonth || year != currentYear) && (month != currentMonth - 1 || year != currentYear))
            {
                // Data exists and it's not current or previous month, just show existing data
                return RedirectToAction("ViewReport", new { month, year });
            }

            // Generate or recalculate report
            await GeneratePayrollData(month, year, existingData);

            return RedirectToAction("ViewReport", new { month, year });
        }

        // GET: PayrollReports/ViewReport
        public async Task<IActionResult> ViewReport(int month, int year)
        {
            var reportData = await GetPayrollReportData(month, year);

            if (!reportData.Any())
            {
                TempData["ErrorMessage"] = $"No payroll data found for {month:D2}/{year}";
                return RedirectToAction("Index");
            }

            var viewModel = new PayrollReportViewModel
            {
                Month = month,
                Year = year,
                RegularEmployees = reportData.Where(r => r.EmployeeType == "001")
                                            .GroupBy(r => r.DepartmentName)
                                            .OrderBy(g => g.Key)
                                            .ToList(),
                ContractEmployees = reportData.Where(r => r.EmployeeType == "002")
                                             .GroupBy(r => r.DepartmentName)
                                             .OrderBy(g => g.Key)
                                             .ToList(),
                DailyWageEmployees = reportData.Where(r => r.EmployeeType == "003")
                                              .GroupBy(r => r.DepartmentName)
                                              .OrderBy(g => g.Key)
                                              .ToList()
            };

            return View(viewModel);
        }

        // GET: PayrollReports/Print
        public async Task<IActionResult> Print(int month, int year)
        {
            var reportData = await GetPayrollReportData(month, year);

            if (!reportData.Any())
            {
                return NotFound();
            }

            var viewModel = new PayrollReportViewModel
            {
                Month = month,
                Year = year,
                RegularEmployees = reportData.Where(r => r.EmployeeType == "001")
                                            .GroupBy(r => r.DepartmentName)
                                            .OrderBy(g => g.Key)
                                            .ToList(),
                ContractEmployees = reportData.Where(r => r.EmployeeType == "002")
                                             .GroupBy(r => r.DepartmentName)
                                             .OrderBy(g => g.Key)
                                             .ToList(),
                DailyWageEmployees = reportData.Where(r => r.EmployeeType == "003")
                                              .GroupBy(r => r.DepartmentName)
                                              .OrderBy(g => g.Key)
                                              .ToList()
            };

            return View(viewModel);
        }

        private async Task<List<PayrollReport>> GetPayrollReportData(int month, int year)
        {
            return await _context.PayrollReports
                .Where(p => p.Month == month && p.Year == year)
                .OrderBy(p => p.DepartmentName)
                .ThenBy(p => p.EmployeeName)
                .ToListAsync();
        }

        private async Task GeneratePayrollData(int month, int year, bool existingData)
        {
            if (existingData)
            {
                // Delete existing data for recalculation
                var existing = await _context.PayrollReports
                    .Where(p => p.Month == month && p.Year == year)
                    .ToListAsync();
                _context.PayrollReports.RemoveRange(existing);
            }

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.DailyAttendances)
                .ToListAsync();

            var salaryDefinitions = await _context.SalaryDefinitions
                .Include(s => s.Employee)
                .ToListAsync();

            var payrollReports = new List<PayrollReport>();

            foreach (var employee in employees)
            {
                var salaryDef = salaryDefinitions.FirstOrDefault(s => s.EmployeeId == employee.Id);
                if (salaryDef == null) continue;

                var attendanceData = await CalculateAttendance(employee.Id, month, year);
                var payrollReport = CreatePayrollReport(employee, salaryDef, month, year, attendanceData);

                payrollReports.Add(payrollReport);
            }

            _context.PayrollReports.AddRange(payrollReports);
            await _context.SaveChangesAsync();
        }

        private async Task<AttendanceData> CalculateAttendance(int employeeId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var totalDaysInMonth = DateTime.DaysInMonth(year, month);

            // Get office timings
            var officeTimings = await _context.OfficeTimings.ToListAsync();
            var offDays = officeTimings.Where(ot => ot.IsOffDay).Select(ot => ot.DayOfWeek).ToList();

            // Get holidays from calendar
            var holidays = await _context.CalendarEvents
                .Where(ce => ce.Date >= startDate && ce.Date <= endDate)
                .Select(ce => ce.Date.Day)
                .ToListAsync();

            // Calculate working days
            var workingDays = 0;
            for (int day = 1; day <= totalDaysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (!offDays.Contains(date.DayOfWeek) && !holidays.Contains(day))
                {
                    workingDays++;
                }
            }

            // Get attendance records
            var attendanceRecords = await _context.DailyAttendances
                .Where(da => da.EmployeeId == employeeId &&
                           da.Date >= startDate && da.Date <= endDate)
                .ToListAsync();

            var presentDays = 0;
            var lateDays = 0.0m;

            foreach (var attendance in attendanceRecords)
            {
                if (attendance.Status == "Present")
                {
                    presentDays++;
                }
                else if (attendance.Status == "Late")
                {
                    lateDays += 0.5m;
                    presentDays++; // Still counted as present but with deduction
                }
            }

            var absentDays = workingDays - presentDays;

            return new AttendanceData
            {
                TotalWorkingDays = workingDays,
                PresentDays = presentDays,
                LateDays = lateDays,
                AbsentDays = absentDays
            };
        }

        private PayrollReport CreatePayrollReport(Employee employee, SalaryDefinition salaryDef,
            int month, int year, AttendanceData attendance)
        {
            var report = new PayrollReport
            {
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
                EmployeeName = employee.Name,
                ZkedID = employee.ZkedID,
                EmployeeType = employee.Type,
                DepartmentName = employee.Department.Name,
                DesignationName = employee.Designation.Name,
                ContractExpiryDate = employee.ContractExpiryDate,
                DOB = employee.DOB,
                TotalWorkingDays = attendance.TotalWorkingDays,
                PresentDays = attendance.PresentDays,
                LateDays = attendance.LateDays,
                AbsentDays = attendance.AbsentDays
            };

            // Set salary components based on employee type
            switch (employee.Type)
            {
                case "001": // Regular
                    report.BasicSalary = salaryDef.BasicSalary;
                    report.HouseRentAllowance = salaryDef.HouseRentAllowance;
                    report.ConveyanceAllowance = salaryDef.ConveyanceAllowance;
                    report.MedicalAllowance = salaryDef.MedicalAllowance;
                    report.GunAllowance = salaryDef.GunAllowance;
                    report.SupplementaryAllowance = salaryDef.SupplementaryAllowance;
                    report.WashAllowance = salaryDef.WashAllowance;
                    report.AdhocAllowance = salaryDef.AdhocAllowance;
                    report.SRAAllowance = salaryDef.SRAAllowance;
                    break;

                case "002": // Contract
                    report.LumpSumAmount = salaryDef.LumpSumAmount;
                    report.HouseRentAll = salaryDef.HouseRentAll;
                    report.ConveyanceAll = salaryDef.ConveyanceAll;
                    report.GunAll = salaryDef.GunAll;
                    report.MiscAllowance = salaryDef.MiscAllowance;
                    break;

                case "003": // Daily Wage
                    report.DailyWage = salaryDef.DailyWage;
                    var totalDaysInMonth = DateTime.DaysInMonth(year, month);
                    report.TotalDailyWages = (salaryDef.DailyWage ?? 0) * totalDaysInMonth;
                    break;
            }

            // Set deductions
            report.EPFDeduction = salaryDef.EPFDeduction;
            report.IncomeTaxDeduction = salaryDef.IncomeTaxDeduction;
            report.EOBIDeduction = salaryDef.EOBIDeduction;
            report.MessDeduction = salaryDef.MessDeduction;
            report.OtherDeductions = salaryDef.OtherDeductions;

            // Calculate totals
            CalculatePayrollTotals(report);

            return report;
        }

        private void CalculatePayrollTotals(PayrollReport report)
        {
            // Calculate gross salary
            switch (report.EmployeeType)
            {
                case "001":
                    report.GrossSalary = (report.BasicSalary ?? 0) + (report.HouseRentAllowance ?? 0) +
                                       (report.ConveyanceAllowance ?? 0) + (report.MedicalAllowance ?? 0) +
                                       (report.GunAllowance ?? 0) + (report.SupplementaryAllowance ?? 0) +
                                       (report.WashAllowance ?? 0) + (report.AdhocAllowance ?? 0) +
                                       (report.SRAAllowance ?? 0);
                    break;
                case "002":
                    report.GrossSalary = (report.LumpSumAmount ?? 0) + (report.HouseRentAll ?? 0) +
                                       (report.ConveyanceAll ?? 0) + (report.GunAll ?? 0) +
                                       (report.MiscAllowance ?? 0);
                    break;
                case "003":
                    report.GrossSalary = report.TotalDailyWages ?? 0;
                    break;
            }

            // Calculate total deductions
            report.TotalDeductions = (report.EPFDeduction ?? 0) + (report.IncomeTaxDeduction ?? 0) +
                                   (report.EOBIDeduction ?? 0) + (report.MessDeduction ?? 0) +
                                   (report.OtherDeductions ?? 0);

            // Calculate absent deductions
            if (report.TotalWorkingDays > 0)
            {
                var dailySalary = report.GrossSalary / DateTime.DaysInMonth(report.Year, report.Month);
                report.AbsentDeductions = Math.Round(dailySalary * (report.AbsentDays + report.LateDays), 2);
            }

            // Calculate net payable
            report.NetPayable = report.GrossSalary - report.TotalDeductions - report.AbsentDeductions;
        }
    }

    // Helper classes
    public class AttendanceData
    {
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public decimal LateDays { get; set; }
        public int AbsentDays { get; set; }
    }

    public class PayrollReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<IGrouping<string, PayrollReport>> RegularEmployees { get; set; } = new();
        public List<IGrouping<string, PayrollReport>> ContractEmployees { get; set; } = new();
        public List<IGrouping<string, PayrollReport>> DailyWageEmployees { get; set; } = new();

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
}