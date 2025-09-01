using Microsoft.AspNetCore.Mvc.Rendering;

namespace EngineersTown.Models.ViewModels
{
    public class PayrollReportViewModel
    {
        public DateTime? PayrollMonth { get; set; }
        public int? DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<PayrollDepartmentDto> DepartmentReports { get; set; } = new List<PayrollDepartmentDto>();
        public PayrollSummaryDto Summary { get; set; } = new PayrollSummaryDto();
    }

    public class PayrollDepartmentDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public List<PayrollEmployeeDto> Employees { get; set; } = new List<PayrollEmployeeDto>();
        public decimal DepartmentGrossTotal { get; set; }
        public decimal DepartmentDeductionsTotal { get; set; }
        public decimal DepartmentNetTotal { get; set; }
    }

    public class PayrollEmployeeDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;

        // Attendance
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }

        // Salary Components
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal GrossSalary { get; set; }

        // Deductions
        public decimal EPFDeduction { get; set; }
        public decimal IncomeTaxDeduction { get; set; }
        public decimal EOBIDeduction { get; set; }
        public decimal MessDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal AttendanceDeduction { get; set; }
        public decimal TotalDeductions { get; set; }

        public decimal NetPayable { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int TotalEmployees { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNet { get; set; }
    }
}