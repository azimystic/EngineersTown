using EngineersTown.Data;
using EngineersTown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EngineersTown.Controllers
{
    public class AttendanceReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new AttendanceReportFilterViewModel
            {
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult GenerateReport(int year, int month)
        {
            return RedirectToAction("PrintReport", new { year, month });
        }

        public async Task<IActionResult> PrintReport(int year, int month)
        {
            var reportData = await GenerateAttendanceReportData(year, month);
            return View(reportData);
        }

        private async Task<AttendanceReportViewModel> GenerateAttendanceReportData(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var daysInMonth = DateTime.DaysInMonth(year, month);

            // Get all employees with their departments and designations
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.DailyAttendances.Where(da => da.Date >= startDate && da.Date <= endDate))
                .Where(e => !e.HasLeft)
                .OrderBy(e => e.Type)
                .ThenBy(e => e.Department.Name)
                .ThenBy(e => e.Name)
                .ToListAsync();

            // Get office timings
            var officeTimings = await _context.OfficeTimings.ToListAsync();

            // Get calendar events (holidays)
            var holidays = await _context.CalendarEvents
                .Where(ce => ce.Date >= startDate && ce.Date <= endDate)
                .ToListAsync();

            // Group employees by type
            var regularEmployees = employees.Where(e => e.Type == "001").ToList();
            var contractEmployees = employees.Where(e => e.Type == "002").ToList();
            var dailyWageEmployees = employees.Where(e => e.Type == "003").ToList();

            var reportModel = new AttendanceReportViewModel
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + year,
                DaysInMonth = daysInMonth,
                StartDate = startDate,
                EndDate = endDate,
                RegularEmployees = GroupEmployeesByDepartment(regularEmployees, startDate, endDate, holidays, officeTimings),
                ContractEmployees = GroupEmployeesByDepartment(contractEmployees, startDate, endDate, holidays, officeTimings),
                DailyWageEmployees = GroupEmployeesByDepartment(dailyWageEmployees, startDate, endDate, holidays, officeTimings)
            };

            return reportModel;
        }

        private List<IGrouping<string, AttendanceEmployeeViewModel>> GroupEmployeesByDepartment(
            List<Employee> employees,
            DateTime startDate,
            DateTime endDate,
            List<CalendarEvent> holidays,
            List<OfficeTiming> officeTimings)
        {
            var employeeViewModels = new List<AttendanceEmployeeViewModel>();

            foreach (var employee in employees)
            {
                var attendanceData = new List<DailyAttendanceData>();

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayOfWeek = date.DayOfWeek;
                    var officeTiming = officeTimings.FirstOrDefault(ot => ot.DayOfWeek == dayOfWeek);
                    var isOffDay = officeTiming?.IsOffDay ?? false;
                    var holiday = holidays.FirstOrDefault(h => h.Date.Date == date.Date);

                    var dailyAttendance = employee.DailyAttendances.FirstOrDefault(da => da.Date.Date == date.Date);

                    var attendanceDay = new DailyAttendanceData
                    {
                        Date = date,
                        DayOfWeek = dayOfWeek,
                        IsOffDay = isOffDay,
                        HolidayName = holiday?.Title,
                        IsHoliday = holiday != null,
                        TimeIn = dailyAttendance?.TimeIn?.ToString(@"hh\:mm") ?? "",
                        TimeOut = dailyAttendance?.TimeOut?.ToString(@"hh\:mm") ?? "",
                        Status = GetAttendanceStatus(dailyAttendance, isOffDay, holiday != null),
                        Day = date.Day
                    };

                    attendanceData.Add(attendanceDay);
                }

                var employeeViewModel = new AttendanceEmployeeViewModel
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    DepartmentName = employee.Department.Name,
                    DesignationName = employee.Designation.Name,
                    EmployeeType = employee.Type,
                    AttendanceData = attendanceData,
                    PresentDays = attendanceData.Count(a => a.Status == "P"),
                    AbsentDays = attendanceData.Count(a => a.Status == "A"),
                    LateDays = attendanceData.Count(a => a.Status == "L"),
                    HolidayDays = attendanceData.Count(a => a.IsHoliday),
                    OffDays = attendanceData.Count(a => a.IsOffDay)
                };

                employeeViewModels.Add(employeeViewModel);
            }

            return employeeViewModels.GroupBy(e => e.DepartmentName).ToList();
        }

        private string GetAttendanceStatus(DailyAttendance? attendance, bool isOffDay, bool isHoliday)
        {
            if (isOffDay) return "OFF";
            if (isHoliday) return "H";

            if (attendance == null) return "A";

            return attendance.Status switch
            {
                "Present" => "P",
                "Late" => "L",
                "Absent" => "A",
                _ => "A"
            };
        }
    }

    // View Models
    public class AttendanceReportFilterViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int DaysInMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<IGrouping<string, AttendanceEmployeeViewModel>> RegularEmployees { get; set; } = new();
        public List<IGrouping<string, AttendanceEmployeeViewModel>> ContractEmployees { get; set; } = new();
        public List<IGrouping<string, AttendanceEmployeeViewModel>> DailyWageEmployees { get; set; } = new();
    }

    public class AttendanceEmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public List<DailyAttendanceData> AttendanceData { get; set; } = new();
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int HolidayDays { get; set; }
        public int OffDays { get; set; }
    }

    public class DailyAttendanceData
    {
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int Day { get; set; }
        public bool IsOffDay { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public string TimeIn { get; set; } = string.Empty;
        public string TimeOut { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}