// CalendarViewModel.cs
namespace EngineersTown.Models
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<CalendarDay> Days { get; set; } = new List<CalendarDay>();
        public List<OfficeTiming> OfficeTimings { get; set; } = new List<OfficeTiming>();
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsOffDay { get; set; }
        public bool IsCustomHoliday { get; set; }
        public TimeSpan? OfficeStartTime { get; set; }
        public TimeSpan? OfficeEndTime { get; set; }
        public string? EventTitle { get; set; }
    }
}