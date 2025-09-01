using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EngineersTown.Data;
using EngineersTown.Models;
using EngineersTown.Models.ViewModels;

namespace EngineersTown.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(ApplicationDbContext context, ILogger<PayrollController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Payroll
        public async Task<IActionResult> Index(DateTime? payrollMonth, int? departmentId)
        {
            try
            {
                var selectedMonth = payrollMonth ?? DateTime.Today;
                var viewModel = new PayrollReportViewModel
                {
                    PayrollMonth = selectedMonth,
                    DepartmentId = departmentId
                };

                // Get departments for dropdown
                var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                viewModel.Departments = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Departments" }
                };
                viewModel.Departments.AddRange(departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }));

                // Generate payroll report
                viewModel.DepartmentReports = await GeneratePayrollReportAsync(selectedMonth, departmentId);

                // Calculate summary
                viewModel.Summary = CalculatePayrollSummary(viewModel.DepartmentReports);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payroll report");
                return View(new PayrollReportViewModel());
            }
        }

        // GET: Payroll/PrintReport
        public async Task<IActionResult> PrintReport(DateTime? payrollMonth, int? departmentId)
        {
            try
            {
                var selectedMonth = payrollMonth ?? DateTime.Today;
                var viewModel = new PayrollReportViewModel
                {
                    PayrollMonth = selectedMonth,
                    DepartmentId = departmentId
                };

                // Generate payroll report
                viewModel.DepartmentReports = await GeneratePayrollReportAsync(selectedMonth, departmentId);
                viewModel.Summary = CalculatePayrollSummary(viewModel.DepartmentReports);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating printable payroll report");
                return View(new PayrollReportViewModel());
            }
        }

        private async Task<List<PayrollDepartmentDto>> GeneratePayrollReportAsync(DateTime payrollMonth, int? departmentId)
        {
            var departmentReports = new List<PayrollDepartmentDto>();

            // Get departments query
            var departmentsQuery = _context.Departments.AsQueryable();
            if (departmentId.HasValue)
            {
                departmentsQuery = departmentsQuery.Where(d => d.Id == departmentId.Value);
            }

            var departments = await departmentsQuery.OrderBy(d => d.Name).ToListAsync();

            foreach (var department in departments)
            {
                var departmentDto = new PayrollDepartmentDto
                {
                    DepartmentName = department.Name,
                    Employees = new List<PayrollEmployeeDto>()
                };

                // Get employees in this department
                var employees = await _context.Employees
                    .Include(e => e.Designation)
                    .Where(e => e.DepartmentId == department.Id)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                foreach (var employee in employees)
                {
                    var employeeDto = await GenerateEmployeePayrollAsync(employee, payrollMonth);
                    departmentDto.Employees.Add(employeeDto);
                }

                // Calculate department totals
                departmentDto.DepartmentGrossTotal = departmentDto.Employees.Sum(e => e.GrossSalary);
                departmentDto.DepartmentDeductionsTotal = departmentDto.Employees.Sum(e => e.TotalDeductions);
                departmentDto.DepartmentNetTotal = departmentDto.Employees.Sum(e => e.NetPayable);

                if (departmentDto.Employees.Any())
                {
                    departmentReports.Add(departmentDto);
                }
            }

            return departmentReports;
        }

        private async Task<PayrollEmployeeDto> GenerateEmployeePayrollAsync(Employee employee, DateTime payrollMonth)
        {
            var employeeDto = new PayrollEmployeeDto
            {
                EmployeeName = employee.Name,
                EmployeeType = GetEmployeeTypeDescription(employee.Type),
                DesignationName = employee.Designation.Name
            };

            // Get salary definition
            var salaryDefinition = await _context.SalaryDefinitions
                .FirstOrDefaultAsync(s => s.EmployeeId == employee.Id);

            if (salaryDefinition != null)
            {
                salaryDefinition.Employee = employee; // Set for calculations

                // Calculate salary components based on employee type
                switch (employee.Type)
                {
                    case "001": // Regular
                        employeeDto.BasicSalary = salaryDefinition.BasicSalary ?? 0;
                        employeeDto.Allowances = (salaryDefinition.HouseRentAllowance ?? 0) +
                                               (salaryDefinition.ConveyanceAllowance ?? 0) +
                                               (salaryDefinition.MedicalAllowance ?? 0) +
                                               (salaryDefinition.GunAllowance ?? 0) +
                                               (salaryDefinition.SupplementaryAllowance ?? 0) +
                                               (salaryDefinition.WashAllowance ?? 0) +
                                               (salaryDefinition.AdhocAllowance ?? 0) +
                                               (salaryDefinition.SRAAllowance ?? 0);
                        break;

                    case "002": // Contract
                        employeeDto.BasicSalary = salaryDefinition.LumpSumAmount ?? 0;
                        employeeDto.Allowances = (salaryDefinition.HouseRentAll ?? 0) +
                                               (salaryDefinition.ConveyanceAll ?? 0) +
                                               (salaryDefinition.GunAll ?? 0) +
                                               (salaryDefinition.MiscAllowance ?? 0);
                        break;

                    case "003": // Daily Wages
                        var attendanceInfo = await GetAttendanceInfoAsync(employee.Id, payrollMonth);
                        employeeDto.BasicSalary = (salaryDefinition.DailyWage ?? 0) * attendanceInfo.PresentDays;
                        employeeDto.Allowances = 0;
                        break;
                }

                employeeDto.GrossSalary = employeeDto.BasicSalary + employeeDto.Allowances;

                // Set deductions
                employeeDto.EPFDeduction = salaryDefinition.EPFDeduction ?? 0;
                employeeDto.IncomeTaxDeduction = salaryDefinition.IncomeTaxDeduction ?? 0;
                employeeDto.EOBIDeduction = salaryDefinition.EOBIDeduction ?? 0;
                employeeDto.MessDeduction = salaryDefinition.MessDeduction ?? 0;
                employeeDto.OtherDeductions = salaryDefinition.OtherDeductions ?? 0;
            }

            // Get attendance information
            var attendance = await GetAttendanceInfoAsync(employee.Id, payrollMonth);
            employeeDto.TotalWorkingDays = attendance.TotalWorkingDays;
            employeeDto.PresentDays = attendance.PresentDays;
            employeeDto.AbsentDays = attendance.AbsentDays;
            employeeDto.LateDays = attendance.LateDays;

            // Calculate attendance deduction
            if (employee.Type != "003") // Not for daily wages (already calculated in basic)
            {
                employeeDto.AttendanceDeduction = CalculateAttendanceDeduction(
                    employeeDto.GrossSalary,
                    attendance.TotalWorkingDays,
                    attendance.AbsentDays);
            }

            // Calculate total deductions and net payable
            employeeDto.TotalDeductions = employeeDto.EPFDeduction +
                                        employeeDto.IncomeTaxDeduction +
                                        employeeDto.EOBIDeduction +
                                        employeeDto.MessDeduction +
                                        employeeDto.OtherDeductions +
                                        employeeDto.AttendanceDeduction;

            employeeDto.NetPayable = employeeDto.GrossSalary - employeeDto.TotalDeductions;

            return employeeDto;
        }

        private async Task<(int TotalWorkingDays, int PresentDays, int AbsentDays, int LateDays)> GetAttendanceInfoAsync(int employeeId, DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get office timings to exclude off days
            var offDays = await _context.OfficeTimings
                .Where(o => o.IsOffDay)
                .Select(o => o.DayOfWeek)
                .ToListAsync();

            // Calculate total working days in month (excluding weekends/off days)
            var totalWorkingDays = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!offDays.Contains(date.DayOfWeek))
                {
                    totalWorkingDays++;
                }
            }

            // Get daily attendance records for the month
            var attendanceRecords = await _context.DailyAttendances
                .Where(d => d.EmployeeId == employeeId &&
                           d.Date >= startDate &&
                           d.Date <= endDate)
                .ToListAsync();

            var presentDays = attendanceRecords.Count(a => a.Status == "Present");
            var lateDays = attendanceRecords.Count(a => a.Status == "Late");
            var absentDays = totalWorkingDays - presentDays - lateDays;

            return (totalWorkingDays, presentDays, absentDays, lateDays);
        }

        private decimal CalculateAttendanceDeduction(decimal grossSalary, int totalWorkingDays, int absentDays)
        {
            if (totalWorkingDays == 0 || absentDays == 0) return 0;

            var dailyRate = grossSalary / totalWorkingDays;
            return Math.Round(dailyRate * absentDays, 2);
        }

        private PayrollSummaryDto CalculatePayrollSummary(List<PayrollDepartmentDto> departmentReports)
        {
            return new PayrollSummaryDto
            {
                TotalEmployees = departmentReports.Sum(d => d.Employees.Count),
                TotalGross = departmentReports.Sum(d => d.DepartmentGrossTotal),
                TotalDeductions = departmentReports.Sum(d => d.DepartmentDeductionsTotal),
                TotalNet = departmentReports.Sum(d => d.DepartmentNetTotal)
            };
        }

        private string GetEmployeeTypeDescription(string type)
        {
            return type switch
            {
                "001" => "Regular",
                "002" => "Contract",
                "003" => "Daily Wages",
                _ => "Unknown"
            };
        }
    }
}