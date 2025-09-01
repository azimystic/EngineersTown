using System.ComponentModel.DataAnnotations;

namespace EngineersTown.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
    }
}