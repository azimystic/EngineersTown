using EngineersTown.Models.ViewModels;

namespace EngineersTown.Services
{
    public interface IAttendanceService
    {
        Task ProcessAttendanceLogsAsync();
        Task<AttendanceViewModel> GetAttendanceDataAsync(DateTime date, int? departmentId);
        Task SaveAttendanceLogAsync(string zkedId, DateTime logTime, string status);
        Task ProcessDailyAttendanceAsync(DateTime date);
    }
}