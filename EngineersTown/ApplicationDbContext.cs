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
        public DbSet<SalaryDefinition> SalaryDefinitions { get; set; }

        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<PayrollReport> PayrollReports { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PayrollReport>(entity =>
            {
                entity.HasOne(pr => pr.Employee)
                      .WithMany()
                      .HasForeignKey(pr => pr.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Create composite unique index to prevent duplicate records for same employee/month/year
                entity.HasIndex(pr => new { pr.EmployeeId, pr.Month, pr.Year })
                      .IsUnique();

                // Create index for faster period-based queries
                entity.HasIndex(pr => new { pr.Month, pr.Year });

                // Create index for department-based queries
                entity.HasIndex(pr => pr.DepartmentName);
            });
            // Configure PayrollReport decimal fields
            var payrollDecimalFields = new[]
            {
            nameof(PayrollReport.BasicSalary),
            nameof(PayrollReport.HouseRentAllowance),
            nameof(PayrollReport.ConveyanceAllowance),
            nameof(PayrollReport.MedicalAllowance),
            nameof(PayrollReport.GunAllowance),
            nameof(PayrollReport.SupplementaryAllowance),
            nameof(PayrollReport.WashAllowance),
            nameof(PayrollReport.AdhocAllowance),
            nameof(PayrollReport.SRAAllowance),
            nameof(PayrollReport.LumpSumAmount),
            nameof(PayrollReport.HouseRentAll),
            nameof(PayrollReport.ConveyanceAll),
            nameof(PayrollReport.GunAll),
            nameof(PayrollReport.MiscAllowance),
            nameof(PayrollReport.DailyWage),
            nameof(PayrollReport.TotalDailyWages),
            nameof(PayrollReport.EPFDeduction),
            nameof(PayrollReport.IncomeTaxDeduction),
            nameof(PayrollReport.EOBIDeduction),
            nameof(PayrollReport.MessDeduction),
            nameof(PayrollReport.OtherDeductions),
            nameof(PayrollReport.GrossSalary),
            nameof(PayrollReport.TotalDeductions),
            nameof(PayrollReport.AbsentDeductions),
            nameof(PayrollReport.NetPayable),
            nameof(PayrollReport.LateDays)
        };

            foreach (var field in payrollDecimalFields)
            {
                modelBuilder.Entity<PayrollReport>()
                            .Property(field)
                            .HasPrecision(18, 2);
            }
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CalendarEvent>()
               .HasIndex(e => e.Date);

            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(e => new { e.Date, e.IsCustomHoliday });
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
                    EndTime = new TimeSpan(16, 0, 0),  // 5:00 PM
                    IsOffDay = isOffDay
                });
            }

            modelBuilder.Entity<OfficeTiming>().HasData(officeTimings);
        }

       
    }
}