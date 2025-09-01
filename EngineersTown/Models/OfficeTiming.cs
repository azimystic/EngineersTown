using System.ComponentModel.DataAnnotations;

namespace EngineersTown.Models
{
    public class OfficeTiming
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public bool IsOffDay { get; set; } = false;
    }
}