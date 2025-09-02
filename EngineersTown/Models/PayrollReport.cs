using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineersTown.Models
{
    public class PayrollReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZkedID { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string EmployeeType { get; set; } = string.Empty; // 001, 002, 003

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DesignationName { get; set; } = string.Empty;

        public DateTime? ContractExpiryDate { get; set; }
        public DateTime DOB { get; set; }

        // Regular Employee (Type 001) Allowances
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalDailyWages { get; set; }

        // Deductions
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EPFDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IncomeTaxDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EOBIDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MessDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherDeductions { get; set; }

        // Attendance Data
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public decimal LateDays { get; set; } // Can be 0.5 for late
        public int AbsentDays { get; set; }

        // Calculated Fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AbsentDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayable { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        // Computed Properties
        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DOB.Year;
                if (DOB.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        public int YearsUntil60 => Math.Max(0, 60 - Age);

        [NotMapped]
        public string FormattedPeriod => $"{Month:D2}/{Year}";
    }
}