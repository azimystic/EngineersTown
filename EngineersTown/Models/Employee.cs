using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineersTown.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZkedID { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }
         public bool HasLeft { get; set; } = false;

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int DesignationId { get; set; }

        [Required]
        [StringLength(3)]
        public string Type { get; set; } = string.Empty; // 001 = Regular, 002 = Contract, 003 = Daily Wager

        public DateTime? ContractExpiryDate { get; set; }
        public string? BPS { get; set; } 

        // Navigation properties
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey("DesignationId")]
        public virtual Designation Designation { get; set; } = null!;

        public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
        public virtual ICollection<DailyAttendance> DailyAttendances { get; set; } = new List<DailyAttendance>();
    }
}