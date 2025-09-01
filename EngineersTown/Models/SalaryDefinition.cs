using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineersTown.Models
{
    public class SalaryDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        // Regular Employee (Type 001) Fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HouseRentAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ConveyanceAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GunAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SupplementaryAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WashAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdhocAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SRAAllowance { get; set; }

        // Contract Employee (Type 002) Fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LumpSumAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HouseRentAll { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ConveyanceAll { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GunAll { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MiscAllowance { get; set; }

        // Daily Wages Employee (Type 003) Fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DailyWage { get; set; }

        // Common Deduction Fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EPFDeduction { get; set; } // 8.33% of Basic for Type 001

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IncomeTaxDeduction { get; set; } // 5% of Gross

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EOBIDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MessDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherDeductions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        // Calculated Properties
        [NotMapped]
        public decimal GrossSalary
        {
            get
            {
                return Employee?.Type switch
                {
                    "001" => (BasicSalary ?? 0) + (HouseRentAllowance ?? 0) + (ConveyanceAllowance ?? 0) +
                             (MedicalAllowance ?? 0) + (GunAllowance ?? 0) + (SupplementaryAllowance ?? 0) +
                             (WashAllowance ?? 0) + (AdhocAllowance ?? 0) + (SRAAllowance ?? 0),
                    "002" => (LumpSumAmount ?? 0) + (HouseRentAll ?? 0) + (ConveyanceAll ?? 0) +
                             (GunAll ?? 0) + (MiscAllowance ?? 0),
                    "003" => DailyWage ?? 0,
                    _ => 0
                };
            }
        }

        [NotMapped]
        public decimal TotalDeductions
        {
            get
            {
                return (EPFDeduction ?? 0) + (IncomeTaxDeduction ?? 0) + (EOBIDeduction ?? 0) +
                       (MessDeduction ?? 0) + (OtherDeductions ?? 0);
            }
        }

        [NotMapped]
        public decimal NetSalary => GrossSalary - TotalDeductions;

        // Method to calculate EPF (8.33% of Basic for Regular employees)
        public void CalculateEPF()
        {
            if (Employee?.Type == "001" && BasicSalary.HasValue)
            {
                EPFDeduction = Math.Round((BasicSalary.Value * 8.33m) / 100, 2);
            }
            else
            {
                EPFDeduction = 0;
            }
        }

        // Method to calculate Income Tax (5% of Gross)
        public void CalculateIncomeTax()
        {
            if (Employee?.Type != "003") // Not for daily wages
            {
                IncomeTaxDeduction = Math.Round((GrossSalary * 5m) / 100, 2);
            }
            else
            {
                IncomeTaxDeduction = 0;
            }
        }
    }
}