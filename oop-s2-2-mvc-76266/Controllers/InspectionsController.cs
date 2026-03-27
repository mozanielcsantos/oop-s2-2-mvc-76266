using FoodSafety.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;
using Serilog;

namespace OopS22Mvc76266.Web.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InspectionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var inspections = await _db.Inspections
                .Include(i => i.Premises)
                .AsNoTracking()
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync();

            return View(inspections);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create()
        {
            await LoadPremisesAsync();
            return View(new Inspection
            {
                InspectionDate = DateTime.Today
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Inspector")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inspection inspection)
        {
            try
            {
                if (inspection.Score < 0 || inspection.Score > 100)
                {
                    Log.Warning("Invalid inspection score attempted: {Score}", inspection.Score);
                    ModelState.AddModelError(nameof(inspection.Score), "Score must be between 0 and 100.");
                }

                if (!await _db.Premises.AnyAsync(p => p.Id == inspection.PremisesId))
                {
                    Log.Warning("Inspection creation failed because PremisesId {PremisesId} does not exist", inspection.PremisesId);
                    ModelState.AddModelError(nameof(inspection.PremisesId), "Selected premises is invalid.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadPremisesAsync();
                    return View(inspection);
                }

                inspection.Outcome = inspection.Score >= 60 ? "Pass" : "Fail";

                _db.Inspections.Add(inspection);
                await _db.SaveChangesAsync();

                Log.Information(
                    "Inspection created. InspectionId: {InspectionId}, PremisesId: {PremisesId}, Score: {Score}, Outcome: {Outcome}",
                    inspection.Id,
                    inspection.PremisesId,
                    inspection.Score,
                    inspection.Outcome);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating inspection");
                throw;
            }
        }

        private async Task LoadPremisesAsync()
        {
            var premises = await _db.Premises
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.PremisesId = new SelectList(premises, "Id", "Name");
        }
    }
}