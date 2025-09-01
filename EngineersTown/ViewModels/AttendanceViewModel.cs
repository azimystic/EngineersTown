using Microsoft.AspNetCore.Mvc.Rendering;

namespace EngineersTown.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public int? SelectedDepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<EmployeeAttendanceDto> EmployeeAttendances { get; set; } = new List<EmployeeAttendanceDto>();
    }

    public class EmployeeAttendanceDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
    }
}