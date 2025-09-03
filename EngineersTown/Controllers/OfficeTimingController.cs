using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngineersTown.Data;
using EngineersTown.Models;

namespace EngineersTown.Controllers
{
    public class OfficeTimingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficeTimingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OfficeTiming
        public async Task<IActionResult> Index()
        {
            var officeTimings = await _context.OfficeTimings
                .OrderBy(ot => ot.DayOfWeek)
                .ToListAsync();
            
            return View(officeTimings);
        }

        // GET: OfficeTiming/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OfficeTiming/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DayOfWeek,StartTime,EndTime,IsOffDay")] OfficeTiming officeTiming)
        {
            if (ModelState.IsValid)
            {
                // Check if timing for this day already exists
                var existingTiming = await _context.OfficeTimings
                    .FirstOrDefaultAsync(ot => ot.DayOfWeek == officeTiming.DayOfWeek);
                
                if (existingTiming != null)
                {
                    ModelState.AddModelError("DayOfWeek", "Office timing for this day already exists.");
                    return View(officeTiming);
                }

                _context.Add(officeTiming);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Office timing created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(officeTiming);
        }

        // GET: OfficeTiming/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officeTiming = await _context.OfficeTimings.FindAsync(id);
            if (officeTiming == null)
            {
                return NotFound();
            }
            return View(officeTiming);
        }

        // POST: OfficeTiming/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayOfWeek,StartTime,EndTime,IsOffDay")] OfficeTiming officeTiming)
        {
            if (id != officeTiming.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(officeTiming);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Office timing updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficeTimingExists(officeTiming.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(officeTiming);
        }

        private bool OfficeTimingExists(int id)
        {
            return _context.OfficeTimings.Any(e => e.Id == id);
        }
    }
}