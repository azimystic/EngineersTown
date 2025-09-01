using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EngineersTown.Data;
using EngineersTown.Models;
using EngineersTown.Models.ViewModels;
 
namespace EngineersTown.Controllers
{
    public class SalaryDefinitionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalaryDefinitionController> _logger;

        public SalaryDefinitionController(ApplicationDbContext context, ILogger<SalaryDefinitionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SalaryDefinition
        public async Task<IActionResult> Index()
        {
            try
            {
                var salaryDefinitions = await _context.SalaryDefinitions
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Designation)
                    .ToListAsync();

                var viewModelList = salaryDefinitions.Select(s => new SalaryDefinitionViewModel
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeName = s.Employee.Name,
                    EmployeeType = s.Employee.Type,
                    DepartmentName = s.Employee.Department?.Name ?? "N/A",
                    DesignationName = s.Employee.Designation?.Name ?? "N/A",
                    GrossSalary = s.GrossSalary,
                    TotalDeductions = s.TotalDeductions,
                    NetSalary = s.NetSalary
                }).ToList();

                return View(viewModelList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading salary definitions");
                TempData["ErrorMessage"] = "Error loading salary definitions.";
                return View(new List<SalaryDefinitionViewModel>());
            }
        }

        // GET: SalaryDefinition/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Where(e => !_context.SalaryDefinitions.Any(s => s.EmployeeId == e.Id))
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                var model = new SalaryCreateViewModel
                {
                    Employees = new SelectList(employees, "Id", "Name"),
                    SalaryDefinition = new SalaryDefinitionViewModel()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = "Error loading create form.";
                return RedirectToAction("Index");
            }
        }


        // POST: SalaryDefinition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalaryDefinitionViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var employees = await _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Designation)
                        .Where(e => !_context.SalaryDefinitions.Any(s => s.EmployeeId == e.Id))
                        .OrderBy(e => e.Name)
                        .ToListAsync();

                    ViewBag.Employees = new SelectList(employees, "Id", "Name");
                    return View(viewModel);
                }

                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .FirstOrDefaultAsync(e => e.Id == viewModel.EmployeeId);

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToAction("Index");
                }

                // Check if salary definition already exists
                var existingSalary = await _context.SalaryDefinitions
                    .FirstOrDefaultAsync(s => s.EmployeeId == viewModel.EmployeeId);

                if (existingSalary != null)
                {
                    TempData["ErrorMessage"] = "Salary definition already exists for this employee.";
                    return RedirectToAction("Edit", new { id = existingSalary.Id });
                }

                var salaryDefinition = new SalaryDefinition
                {
                    EmployeeId = viewModel.EmployeeId,
                    CreatedAt = DateTime.Now
                };

                MapViewModelToSalaryDefinition(viewModel, salaryDefinition, employee.Type);

                // Calculate automatic deductions
                salaryDefinition.Employee = employee;
                salaryDefinition.CalculateEPF();
                salaryDefinition.CalculateIncomeTax();

                _context.SalaryDefinitions.Add(salaryDefinition);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Salary definition created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating salary definition");
                TempData["ErrorMessage"] = "Error creating salary definition.";

                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Where(e => !_context.SalaryDefinitions.Any(s => s.EmployeeId == e.Id))
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                ViewBag.Employees = new SelectList(employees, "Id", "Name");
                return View(viewModel);
            }
        }

        // GET: SalaryDefinition/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var salaryDefinition = await _context.SalaryDefinitions
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Designation)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (salaryDefinition == null)
                {
                    TempData["ErrorMessage"] = "Salary definition not found.";
                    return RedirectToAction("Index");
                }

                var viewModel = new SalaryDefinitionViewModel
                {
                    Id = salaryDefinition.Id,
                    EmployeeId = salaryDefinition.EmployeeId,
                    EmployeeName = salaryDefinition.Employee.Name,
                    EmployeeType = salaryDefinition.Employee.Type,
                    DepartmentName = salaryDefinition.Employee.Department?.Name ?? "N/A",
                    DesignationName = salaryDefinition.Employee.Designation?.Name ?? "N/A"
                };

                MapSalaryDefinitionToViewModel(salaryDefinition, viewModel, salaryDefinition.Employee.Type);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading salary definition {Id}", id);
                TempData["ErrorMessage"] = "Error loading salary definition.";
                return RedirectToAction("Index");
            }
        }

        // POST: SalaryDefinition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalaryDefinitionViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Id)
                {
                    TempData["ErrorMessage"] = "Invalid request.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                var salaryDefinition = await _context.SalaryDefinitions
                    .Include(s => s.Employee)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (salaryDefinition == null)
                {
                    TempData["ErrorMessage"] = "Salary definition not found.";
                    return RedirectToAction("Index");
                }

                MapViewModelToSalaryDefinition(viewModel, salaryDefinition, salaryDefinition.Employee.Type);
                salaryDefinition.UpdatedAt = DateTime.Now;

                // Recalculate automatic deductions
                salaryDefinition.CalculateEPF();
                salaryDefinition.CalculateIncomeTax();

                _context.SalaryDefinitions.Update(salaryDefinition);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Salary definition updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating salary definition {Id}", id);
                TempData["ErrorMessage"] = "Error updating salary definition.";
                return View(viewModel);
            }
        }

        // GET: SalaryDefinition/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var salaryDefinition = await _context.SalaryDefinitions
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Designation)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (salaryDefinition == null)
                {
                    TempData["ErrorMessage"] = "Salary definition not found.";
                    return RedirectToAction("Index");
                }

                var viewModel = new SalaryDefinitionViewModel
                {
                    Id = salaryDefinition.Id,
                    EmployeeId = salaryDefinition.EmployeeId,
                    EmployeeName = salaryDefinition.Employee.Name,
                    EmployeeType = salaryDefinition.Employee.Type,
                    DepartmentName = salaryDefinition.Employee.Department?.Name ?? "N/A",
                    DesignationName = salaryDefinition.Employee.Designation?.Name ?? "N/A"
                };

                MapSalaryDefinitionToViewModel(salaryDefinition, viewModel, salaryDefinition.Employee.Type);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading salary definition details {Id}", id);
                TempData["ErrorMessage"] = "Error loading salary definition details.";
                return RedirectToAction("Index");
            }
        }

        // POST: SalaryDefinition/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var salaryDefinition = await _context.SalaryDefinitions.FindAsync(id);
                if (salaryDefinition == null)
                {
                    TempData["ErrorMessage"] = "Salary definition not found.";
                    return RedirectToAction("Index");
                }

                _context.SalaryDefinitions.Remove(salaryDefinition);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Salary definition deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting salary definition {Id}", id);
                TempData["ErrorMessage"] = "Error deleting salary definition.";
            }

            return RedirectToAction("Index");
        }

        // Helper methods
        private void MapSalaryDefinitionToViewModel(SalaryDefinition salaryDef, SalaryDefinitionViewModel viewModel, string employeeType)
        {
            // Common fields
            viewModel.EPFDeduction = salaryDef.EPFDeduction;
            viewModel.IncomeTaxDeduction = salaryDef.IncomeTaxDeduction;
            viewModel.EOBIDeduction = salaryDef.EOBIDeduction;
            viewModel.MessDeduction = salaryDef.MessDeduction;
            viewModel.OtherDeductions = salaryDef.OtherDeductions;

            // Type-specific fields
            switch (employeeType)
            {
                case "001": // Regular
                    viewModel.BasicSalary = salaryDef.BasicSalary;
                    viewModel.HouseRentAllowance = salaryDef.HouseRentAllowance;
                    viewModel.ConveyanceAllowance = salaryDef.ConveyanceAllowance;
                    viewModel.MedicalAllowance = salaryDef.MedicalAllowance;
                    viewModel.GunAllowance = salaryDef.GunAllowance;
                    viewModel.SupplementaryAllowance = salaryDef.SupplementaryAllowance;
                    viewModel.WashAllowance = salaryDef.WashAllowance;
                    viewModel.AdhocAllowance = salaryDef.AdhocAllowance;
                    viewModel.SRAAllowance = salaryDef.SRAAllowance;
                    break;
                case "002": // Contract
                    viewModel.LumpSumAmount = salaryDef.LumpSumAmount;
                    viewModel.HouseRentAll = salaryDef.HouseRentAll;
                    viewModel.ConveyanceAll = salaryDef.ConveyanceAll;
                    viewModel.GunAll = salaryDef.GunAll;
                    viewModel.MiscAllowance = salaryDef.MiscAllowance;
                    break;
                case "003": // Daily Wages
                    viewModel.DailyWage = salaryDef.DailyWage;
                    break;
            }

            // Calculated properties
            viewModel.GrossSalary = salaryDef.GrossSalary;
            viewModel.TotalDeductions = salaryDef.TotalDeductions;
            viewModel.NetSalary = salaryDef.NetSalary;
        }

        private void MapViewModelToSalaryDefinition(SalaryDefinitionViewModel viewModel, SalaryDefinition salaryDef, string employeeType)
        {
            // Reset all fields first
            ResetSalaryFields(salaryDef);

            // Common deductions
            salaryDef.EOBIDeduction = viewModel.EOBIDeduction;
            salaryDef.MessDeduction = viewModel.MessDeduction;
            salaryDef.OtherDeductions = viewModel.OtherDeductions;

            // Type-specific fields
            switch (employeeType)
            {
                case "001": // Regular
                    salaryDef.BasicSalary = viewModel.BasicSalary;
                    salaryDef.HouseRentAllowance = viewModel.HouseRentAllowance;
                    salaryDef.ConveyanceAllowance = viewModel.ConveyanceAllowance;
                    salaryDef.MedicalAllowance = viewModel.MedicalAllowance;
                    salaryDef.GunAllowance = viewModel.GunAllowance;
                    salaryDef.SupplementaryAllowance = viewModel.SupplementaryAllowance;
                    salaryDef.WashAllowance = viewModel.WashAllowance;
                    salaryDef.AdhocAllowance = viewModel.AdhocAllowance;
                    salaryDef.SRAAllowance = viewModel.SRAAllowance;
                    break;
                case "002": // Contract
                    salaryDef.LumpSumAmount = viewModel.LumpSumAmount;
                    salaryDef.HouseRentAll = viewModel.HouseRentAll;
                    salaryDef.ConveyanceAll = viewModel.ConveyanceAll;
                    salaryDef.GunAll = viewModel.GunAll;
                    salaryDef.MiscAllowance = viewModel.MiscAllowance;
                    break;
                case "003": // Daily Wages
                    salaryDef.DailyWage = viewModel.DailyWage;
                    break;
            }
        }

        private void ResetSalaryFields(SalaryDefinition salaryDef)
        {
            // Reset all salary fields to null
            salaryDef.BasicSalary = null;
            salaryDef.HouseRentAllowance = null;
            salaryDef.ConveyanceAllowance = null;
            salaryDef.MedicalAllowance = null;
            salaryDef.GunAllowance = null;
            salaryDef.SupplementaryAllowance = null;
            salaryDef.WashAllowance = null;
            salaryDef.AdhocAllowance = null;
            salaryDef.SRAAllowance = null;
            salaryDef.LumpSumAmount = null;
            salaryDef.HouseRentAll = null;
            salaryDef.ConveyanceAll = null;
            salaryDef.GunAll = null;
            salaryDef.MiscAllowance = null;
            salaryDef.DailyWage = null;
        }
    }
}