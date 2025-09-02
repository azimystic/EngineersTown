using EngineersTown.Data;
using EngineersTown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EngineersTown.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .ToListAsync();
            return View(employees);
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["DesignationId"] = new SelectList(new List<Designation>(), "Id", "Name");
            ViewData["EmployeeTypes"] = new SelectList(new[]
            {
                new { Value = "001", Text = "Regular" },
                new { Value = "002", Text = "Contract" },
                new { Value = "003", Text = "Daily Wager" }
            }, "Value", "Text");

            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ZkedID,CNIC,DOB,DepartmentId,DesignationId,Type,ContractExpiryDate")] Employee employee)
        {
            ModelState.Remove("Department");
            ModelState.Remove("Designation");
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d => d.DepartmentId == employee.DepartmentId), "Id", "Name", employee.DesignationId);
            ViewData["EmployeeTypes"] = new SelectList(new[]
            {
                new { Value = "001", Text = "Regular" },
                new { Value = "002", Text = "Contract" },
                new { Value = "003", Text = "Daily Wager" }
            }, "Value", "Text", employee.Type);

            return View(employee);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get designations by department
        [HttpGet]
        public async Task<JsonResult> GetDesignationsByDepartment(int departmentId)
        {
            var designations = await _context.Designations
                .Where(d => d.DepartmentId == departmentId)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Json(designations);
        }
    }
}