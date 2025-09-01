namespace EngineersTown.Services
{
    public interface IZKTecoService
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<List<AttendancePunch>> FetchNewAttendanceLogsAsync();
        Task<bool> IsConnectedAsync();
    }

    public class AttendancePunch
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime PunchTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}