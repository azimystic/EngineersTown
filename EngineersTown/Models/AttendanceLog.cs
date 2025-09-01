using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineersTown.Models
{
    public class AttendanceLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime LogTime { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } = string.Empty; // IN, OUT, etc.

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}