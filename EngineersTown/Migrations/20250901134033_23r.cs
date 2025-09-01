using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngineersTown.Migrations
{
    /// <inheritdoc />
    public partial class _23r : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HouseRentAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ConveyanceAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GunAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SupplementaryAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WashAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdhocAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SRAAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LumpSumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HouseRentAll = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ConveyanceAll = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GunAll = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MiscAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DailyWage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPFDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IncomeTaxDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EOBIDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MessDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryDefinitions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDefinitions_EmployeeId",
                table: "SalaryDefinitions",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryDefinitions");
        }
    }
}
