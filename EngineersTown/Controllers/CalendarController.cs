// CalendarController.cs
using EngineersTown.Data;
using EngineersTown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EngineersTown.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calendar
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var currentDate = DateTime.Now;
            var viewYear = year ?? currentDate.Year;
            var viewMonth = month ?? currentDate.Month;

            var model = await GenerateCalendarModel(viewYear, viewMonth);
            return View(model);
        }

        // POST: Calendar/Update
        [HttpPost]
        public async Task<IActionResult> Update(int year, int month)
        {
            var model = await GenerateCalendarModel(year, month);
            return View("Index", model);
        }

        // POST: Calendar/AddHoliday
        [HttpPost]
        public async Task<IActionResult> AddHoliday([FromBody] HolidayRequest request)
        {
            if (request.Date == default)
            {
                return Json(new { success = false, message = "Invalid date" });
            }

            // Check if holiday already exists
            var existingHoliday = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Date.Date == request.Date.Date && e.IsCustomHoliday);

            if (existingHoliday != null)
            {
                return Json(new { success = false, message = "Holiday already exists for this date" });
            }

            var holiday = new CalendarEvent
            {
                Date = request.Date.Date,
                Title = request.Title,
                Description = request.Description,
                IsCustomHoliday = true,
                CreatedAt = DateTime.Now
            };

            _context.CalendarEvents.Add(holiday);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Holiday added successfully" });
        }

        // POST: Calendar/RemoveHoliday
        [HttpPost]
        public async Task<IActionResult> RemoveHoliday([FromBody] HolidayRequest request)
        {
            var holiday = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Date.Date == request.Date.Date && e.IsCustomHoliday);

            if (holiday != null)
            {
                _context.CalendarEvents.Remove(holiday);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Holiday removed successfully" });
            }

            return Json(new { success = false, message = "Holiday not found" });
        }

        public class HolidayRequest
        {
            public DateTime Date { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
        }
        private async Task<CalendarViewModel> GenerateCalendarModel(int year, int month)
        {
            var model = new CalendarViewModel
            {
                Year = year,
                Month = month
            };

            // Get office timings
            model.OfficeTimings = await _context.OfficeTimings.ToListAsync();

            // Get calendar events
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            model.Events = await _context.CalendarEvents
                .Where(e => e.Date >= firstDayOfMonth && e.Date <= lastDayOfMonth)
                .ToListAsync();

            // Generate calendar days
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var firstDay = new DateTime(year, month, 1);
            var lastDay = new DateTime(year, month, daysInMonth);

            // Add days from previous month
            var startDayOfWeek = (int)firstDay.DayOfWeek;
            for (int i = 0; i < startDayOfWeek; i++)
            {
                var prevDate = firstDay.AddDays(-(startDayOfWeek - i));
                model.Days.Add(new CalendarDay
                {
                    Date = prevDate,
                    DayNumber = prevDate.Day,
                    IsCurrentMonth = false,
                    IsToday = prevDate.Date == DateTime.Today
                });
            }

            // Add current month days
            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(year, month, day);
                var dayOfWeek = currentDate.DayOfWeek;
                var officeTiming = model.OfficeTimings.FirstOrDefault(ot => ot.DayOfWeek == dayOfWeek);
                var holiday = model.Events.FirstOrDefault(e => e.Date.Date == currentDate.Date && e.IsCustomHoliday);

                model.Days.Add(new CalendarDay
                {
                    Date = currentDate,
                    DayNumber = day,
                    IsCurrentMonth = true,
                    IsToday = currentDate.Date == DateTime.Today,
                    IsOffDay = officeTiming?.IsOffDay ?? false,
                    IsCustomHoliday = holiday != null,
                    OfficeStartTime = officeTiming?.StartTime,
                    OfficeEndTime = officeTiming?.EndTime,
                    EventTitle = holiday?.Title
                });
            }

            // Add days from next month to complete the grid
            var totalCells = 42; // 6 weeks * 7 days
            var remainingCells = totalCells - model.Days.Count;
            for (int i = 1; i <= remainingCells; i++)
            {
                var nextDate = lastDay.AddDays(i);
                model.Days.Add(new CalendarDay
                {
                    Date = nextDate,
                    DayNumber = nextDate.Day,
                    IsCurrentMonth = false,
                    IsToday = nextDate.Date == DateTime.Today
                });
            }

            return model;
        }
    }
}