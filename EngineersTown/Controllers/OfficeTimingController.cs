using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngineersTown.Models;
using EngineersTown.Data;

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
            try
            {
                var officeTimings = await _context.OfficeTimings
                    .OrderBy(o => o.DayOfWeek)
                    .ToListAsync();

                return View(officeTimings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading office timings.";
                return View(new List<OfficeTiming>());
            }
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
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error updating office timing.";
                }
            }

            return View(officeTiming);
        }

        private bool OfficeTimingExists(int id)
        {
            return _context.OfficeTimings.Any(e => e.Id == id);
        }
    }
}