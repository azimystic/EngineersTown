using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EngineersTown.Models.ViewModels
{
    public class SalaryDefinitionViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;

        // Regular Employee Fields
        [Display(Name = "Basic Salary")]
        public decimal? BasicSalary { get; set; }

        [Display(Name = "House Rent Allowance")]
        public decimal? HouseRentAllowance { get; set; }

        [Display(Name = "Conveyance Allowance")]
        public decimal? ConveyanceAllowance { get; set; }

        [Display(Name = "Medical Allowance")]
        public decimal? MedicalAllowance { get; set; }

        [Display(Name = "Gun Allowance")]
        public decimal? GunAllowance { get; set; }

        [Display(Name = "Supplementary Allowance")]
        public decimal? SupplementaryAllowance { get; set; }

        [Display(Name = "Wash Allowance")]
        public decimal? WashAllowance { get; set; }

        [Display(Name = "Adhoc Allowance")]
        public decimal? AdhocAllowance { get; set; }

        [Display(Name = "SRA Allowance")]
        public decimal? SRAAllowance { get; set; }

        // Contract Employee Fields
        [Display(Name = "Lump Sum Amount")]
        public decimal? LumpSumAmount { get; set; }

        [Display(Name = "House Rent (All)")]
        public decimal? HouseRentAll { get; set; }

        [Display(Name = "Conveyance (All)")]
        public decimal? ConveyanceAll { get; set; }

        [Display(Name = "Gun (All)")]
        public decimal? GunAll { get; set; }

        [Display(Name = "Misc Allowance")]
        public decimal? MiscAllowance { get; set; }

        // Daily Wages Fields
        [Display(Name = "Daily Wage")]
        public decimal? DailyWage { get; set; }

        // Deductions
        [Display(Name = "EPF Deduction")]
        public decimal? EPFDeduction { get; set; }

        [Display(Name = "Income Tax")]
        public decimal? IncomeTaxDeduction { get; set; }

        [Display(Name = "EOBI")]
        public decimal? EOBIDeduction { get; set; }

        [Display(Name = "Mess Deduction")]
        public decimal? MessDeduction { get; set; }

        [Display(Name = "Other Deductions")]
        public decimal? OtherDeductions { get; set; }

        public decimal GrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
    }

    public class SalarySearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SalaryDefinitionViewModel> Employees { get; set; } = new List<SalaryDefinitionViewModel>();
    }

}