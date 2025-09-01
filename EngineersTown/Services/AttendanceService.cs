using Microsoft.EntityFrameworkCore;
using EngineersTown.Data;
using EngineersTown.Models;
using EngineersTown.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EngineersTown.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AttendanceService> _logger;

        private readonly int _lateThresholdMinutes;
        private readonly int _earlyExitThresholdMinutes;
        private readonly int _minimumWorkingHours;

        public AttendanceService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AttendanceService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

            _lateThresholdMinutes = int.Parse(_configuration["AttendanceRules:LateThresholdMinutes"] ?? "20");
            _earlyExitThresholdMinutes = int.Parse(_configuration["AttendanceRules:EarlyExitThresholdMinutes"] ?? "20");
            _minimumWorkingHours = int.Parse(_configuration["AttendanceRules:MinimumWorkingHours"] ?? "7");
        }

        public async Task ProcessAttendanceLogsAsync()
        {
            try
            {
                var today = DateTime.Today;
                await ProcessDailyAttendanceAsync(today);
                _logger.LogInformation("Processed daily attendance for {Date}", today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attendance logs");
            }
        }

        public async Task SaveAttendanceLogAsync(string zkedId, DateTime logTime, string status)
        {
            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.ZkedID == zkedId);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with ZkedID: {ZkedId}", zkedId);
                    return;
                }

                // Check if this exact punch already exists
                var existingLog = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id &&
                                            a.LogTime == logTime &&
                                            a.Status == status);

                if (existingLog != null)
                {
                    _logger.LogDebug("Duplicate attendance log ignored for employee {EmployeeId} at {LogTime}", employee.Id, logTime);
                    return;
                }

                var attendanceLog = new AttendanceLog
                {
                    EmployeeId = employee.Id,
                    LogTime = logTime,
                    Status = status,
                    CreatedAt = DateTime.Now
                };

                _context.AttendanceLogs.Add(attendanceLog);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Saved attendance log for employee {EmployeeId}: {Status} at {LogTime}",
                    employee.Id, status, logTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving attendance log for ZkedID: {ZkedId}", zkedId);
            }
        }

        public async Task ProcessDailyAttendanceAsync(DateTime date)
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .ToListAsync();

                foreach (var employee in employees)
                {
                    await ProcessEmployeeDailyAttendanceAsync(employee, date);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing daily attendance for date: {Date}", date);
            }
        }

        private async Task ProcessEmployeeDailyAttendanceAsync(Employee employee, DateTime date)
        {
            try
            {
                // Get office timing for the day
                var dayOfWeek = date.DayOfWeek;
                var officeTiming = await _context.OfficeTimings
                    .FirstOrDefaultAsync(o => o.DayOfWeek == dayOfWeek);

                if (officeTiming?.IsOffDay == true)
                {
                    // Skip processing for off days
                    return;
                }

                // Get attendance logs for the employee on this date
                var logs = await _context.AttendanceLogs
                    .Where(a => a.EmployeeId == employee.Id && a.LogTime.Date == date.Date)
                    .OrderBy(a => a.LogTime)
                    .ToListAsync();

                // Check if daily attendance record already exists
                var existingAttendance = await _context.DailyAttendances
                    .FirstOrDefaultAsync(d => d.EmployeeId == employee.Id && d.Date.Date == date.Date);

                if (logs.Any())
                {
                    var timeIn = logs.First().LogTime.TimeOfDay;
                    var timeOut = logs.Count > 1 ? logs.Last().LogTime.TimeOfDay : (TimeSpan?)null;
                    var status = CalculateAttendanceStatus(timeIn, timeOut, officeTiming);

                    if (existingAttendance != null)
                    {
                        // Update existing record
                        existingAttendance.TimeIn = timeIn;
                        existingAttendance.TimeOut = timeOut;
                        existingAttendance.Status = status;
                        existingAttendance.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        // Create new record
                        var dailyAttendance = new DailyAttendance
                        {
                            EmployeeId = employee.Id,
                            Date = date.Date,
                            TimeIn = timeIn,
                            TimeOut = timeOut,
                            Status = status,
                            CreatedAt = DateTime.Now
                        };
                        _context.DailyAttendances.Add(dailyAttendance);
                    }
                }
                else
                {
                    // No punches - mark as absent
                    if (existingAttendance != null)
                    {
                        existingAttendance.TimeIn = null;
                        existingAttendance.TimeOut = null;
                        existingAttendance.Status = "Absent";
                        existingAttendance.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        var dailyAttendance = new DailyAttendance
                        {
                            EmployeeId = employee.Id,
                            Date = date.Date,
                            TimeIn = null,
                            TimeOut = null,
                            Status = "Absent",
                            CreatedAt = DateTime.Now
                        };
                        _context.DailyAttendances.Add(dailyAttendance);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing daily attendance for employee {EmployeeId} on {Date}",
                    employee.Id, date);
            }
        }

        private string CalculateAttendanceStatus(TimeSpan timeIn, TimeSpan? timeOut, OfficeTiming? officeTiming)
        {
            if (officeTiming == null)
            {
                return "Present"; // Default if no office timing defined
            }

            var officeStart = officeTiming.StartTime;
            var officeEnd = officeTiming.EndTime;
            var lateThreshold = TimeSpan.FromMinutes(_lateThresholdMinutes);
            var earlyExitThreshold = TimeSpan.FromMinutes(_earlyExitThresholdMinutes);
            var minimumWorkingTime = TimeSpan.FromHours(_minimumWorkingHours);

            // Check if arrived late
            bool isLateArrival = timeIn > (officeStart + lateThreshold);

            // Check if left early
            bool isEarlyExit = timeOut.HasValue && timeOut.Value < (officeEnd - earlyExitThreshold);

            // Check if no time out
            bool noTimeOut = !timeOut.HasValue;

            // Calculate working duration if both time in and time out are available
            TimeSpan? workingDuration = timeOut.HasValue ? timeOut.Value - timeIn : null;
            bool workedMinimumHours = workingDuration.HasValue && workingDuration.Value >= minimumWorkingTime;

            // Apply business rules
            if (isLateArrival && workedMinimumHours)
            {
                return "Present"; // Late arrival but worked full duration
            }
            else if (isLateArrival || isEarlyExit || noTimeOut)
            {
                return "Late";
            }
            else
            {
                return "Present";
            }
        }

        public async Task<AttendanceViewModel> GetAttendanceDataAsync(DateTime date, int? departmentId)
        {
            var viewModel = new AttendanceViewModel
            {
                SelectedDate = date,
                SelectedDepartmentId = departmentId
            };

            try
            {
                // Get departments for dropdown
                var departments = await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                viewModel.Departments = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Departments" }
                };
                viewModel.Departments.AddRange(departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }));

                // Build query for employees
                var query = _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .AsQueryable();

                if (departmentId.HasValue)
                {
                    query = query.Where(e => e.DepartmentId == departmentId.Value);
                }

                var employees = await query
                    .OrderBy(e => e.Department.Name)
                    .ThenBy(e => e.Name)
                    .ToListAsync();

                // Get daily attendance records for the selected date
                var dailyAttendances = await _context.DailyAttendances
                    .Where(d => d.Date.Date == date.Date)
                    .ToDictionaryAsync(d => d.EmployeeId);

                // Build employee attendance data
                viewModel.EmployeeAttendances = employees.Select(emp =>
                {
                    var attendance = dailyAttendances.ContainsKey(emp.Id) ? dailyAttendances[emp.Id] : null;

                    return new EmployeeAttendanceDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        DepartmentName = emp.Department.Name,
                        DesignationName = emp.Designation.Name,
                        TimeIn = attendance?.TimeIn?.ToString(@"hh\:mm") ?? "-",
                        TimeOut = attendance?.TimeOut?.ToString(@"hh\:mm") ?? "-",
                        Status = attendance?.Status ?? "Absent",
                        StatusDisplay = GetStatusDisplay(attendance?.Status ?? "Absent")
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance data for date: {Date}", date);
            }

            return viewModel;
        }

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "Present" => "P",
                "Absent" => "A",
                "Late" => "Late",
                _ => "A"
            };
        }
    }
}