using Microsoft.AspNetCore.Mvc;
using EngineersTown.Services;
using EngineersTown.Models.ViewModels;

namespace EngineersTown.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IAttendanceService attendanceService,
            ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? date, int? departmentId)
        {
            try
            {
                var selectedDate = date ?? DateTime.Today;
                var viewModel = await _attendanceService.GetAttendanceDataAsync(selectedDate, departmentId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading attendance data");

                // Return empty view model in case of error
                var emptyViewModel = new AttendanceViewModel
                {
                    SelectedDate = date ?? DateTime.Today,
                    SelectedDepartmentId = departmentId
                };

                ViewBag.ErrorMessage = "An error occurred while loading attendance data.";
                return View(emptyViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAttendance(DateTime? date)
        {
            try
            {
                var selectedDate = date ?? DateTime.Today;
                await _attendanceService.ProcessAttendanceLogsAsync(selectedDate);
                TempData["SuccessMessage"] = $"Attendance processed successfully for {selectedDate:yyyy-MM-dd}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attendance");
                TempData["ErrorMessage"] = "An error occurred while processing attendance.";
            }

            return RedirectToAction("Index", new { date });
        }
    }
}