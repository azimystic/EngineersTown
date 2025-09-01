using EngineersTown.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EngineersTown.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<DailyAttendance> DailyAttendances { get; set; }
        public DbSet<OfficeTiming> OfficeTimings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.ZkedID).IsUnique();
                entity.HasIndex(e => e.CNIC).IsUnique();

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Designation)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DesignationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Designation
            modelBuilder.Entity<Designation>(entity =>
            {
                entity.HasOne(d => d.Department)
                    .WithMany(dept => dept.Designations)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AttendanceLog
            modelBuilder.Entity<AttendanceLog>(entity =>
            {
                entity.HasOne(a => a.Employee)
                    .WithMany(e => e.AttendanceLogs)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => new { a.EmployeeId, a.LogTime });
            });

            // Configure DailyAttendance
            modelBuilder.Entity<DailyAttendance>(entity =>
            {
                entity.HasOne(d => d.Employee)
                    .WithMany(e => e.DailyAttendances)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(d => new { d.EmployeeId, d.Date }).IsUnique();
            });

            // Configure OfficeTiming
            modelBuilder.Entity<OfficeTiming>(entity =>
            {
                entity.HasIndex(o => o.DayOfWeek).IsUnique();
            });

            // Seed default office timings
            SeedOfficeTimings(modelBuilder);

            // Seed sample data
            SeedSampleData(modelBuilder);
        }

        private void SeedOfficeTimings(ModelBuilder modelBuilder)
        {
            var officeTimings = new List<OfficeTiming>();

            for (int i = 0; i < 7; i++)
            {
                var dayOfWeek = (DayOfWeek)i;
                var isOffDay = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

                officeTimings.Add(new OfficeTiming
                {
                    Id = i + 1,
                    DayOfWeek = dayOfWeek,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0),  // 5:00 PM
                    IsOffDay = isOffDay
                });
            }

            modelBuilder.Entity<OfficeTiming>().HasData(officeTimings);
        }

        private void SeedSampleData(ModelBuilder modelBuilder)
        {
            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Engineering" },
                new Department { Id = 2, Name = "Human Resources" },
                new Department { Id = 3, Name = "Finance" },
                new Department { Id = 4, Name = "Operations" }
            );

            // Seed Designations
            modelBuilder.Entity<Designation>().HasData(
                new Designation { Id = 1, DepartmentId = 1, Name = "Software Engineer" },
                new Designation { Id = 2, DepartmentId = 1, Name = "Senior Engineer" },
                new Designation { Id = 3, DepartmentId = 1, Name = "Tech Lead" },
                new Designation { Id = 4, DepartmentId = 2, Name = "HR Manager" },
                new Designation { Id = 5, DepartmentId = 2, Name = "HR Executive" },
                new Designation { Id = 6, DepartmentId = 3, Name = "Accountant" },
                new Designation { Id = 7, DepartmentId = 4, Name = "Operations Manager" }
            );

            // Seed Sample Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Ahmad Ali",
                    ZkedID = "101",
                    CNIC = "12345-1234567-1",
                    DOB = new DateTime(1990, 5, 15),
                    DepartmentId = 1,
                    DesignationId = 1,
                    Type = "001"
                },
                new Employee
                {
                    Id = 2,
                    Name = "Sara Khan",
                    ZkedID = "102",
                    CNIC = "12345-1234567-2",
                    DOB = new DateTime(1988, 8, 22),
                    DepartmentId = 1,
                    DesignationId = 2,
                    Type = "001"
                },
                new Employee
                {
                    Id = 3,
                    Name = "Hassan Sheikh",
                    ZkedID = "103",
                    CNIC = "12345-1234567-3",
                    DOB = new DateTime(1992, 12, 10),
                    DepartmentId = 2,
                    DesignationId = 4,
                    Type = "002",
                    ContractExpiryDate = new DateTime(2025, 12, 31)
                }
            );
        }
    }
}