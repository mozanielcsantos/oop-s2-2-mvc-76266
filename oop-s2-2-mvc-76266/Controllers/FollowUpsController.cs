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
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FollowUpsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var followUps = await _db.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .AsNoTracking()
                .OrderBy(f => f.DueDate)
                .ToListAsync();

            return View(followUps);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create()
        {
            await LoadInspectionsAsync();
            return View(new FollowUp
            {
                DueDate = DateTime.Today.AddDays(7),
                Status = "Open"
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Inspector")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUp followUp)
        {
            try
            {
                var inspection = await _db.Inspections
                    .Include(i => i.Premises)
                    .FirstOrDefaultAsync(i => i.Id == followUp.InspectionId);

                if (inspection == null)
                {
                    Log.Warning("FollowUp creation failed: invalid InspectionId {InspectionId}", followUp.InspectionId);
                    ModelState.AddModelError(nameof(followUp.InspectionId), "Selected inspection is invalid.");
                }

                if (inspection != null && followUp.DueDate < inspection.InspectionDate)
                {
                    Log.Warning(
                        "FollowUp creation blocked because DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                        followUp.DueDate,
                        inspection.InspectionDate,
                        inspection.Id);

                    ModelState.AddModelError(nameof(followUp.DueDate), "Due date cannot be before the inspection date.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadInspectionsAsync();
                    return View(followUp);
                }

                followUp.Status = "Open";
                followUp.ClosedDate = null;

                _db.FollowUps.Add(followUp);
                await _db.SaveChangesAsync();

                Log.Information(
                    "FollowUp created. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}, DueDate: {DueDate}",
                    followUp.Id,
                    followUp.InspectionId,
                    followUp.DueDate);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating follow-up");
                throw;
            }
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var followUp = await _db.FollowUps.FindAsync(id);

                if (followUp == null)
                {
                    Log.Warning("Attempt to close non-existing FollowUp {FollowUpId}", id);
                    return NotFound();
                }

                followUp.Status = "Closed";
                followUp.ClosedDate = DateTime.UtcNow;

                _db.Update(followUp);
                await _db.SaveChangesAsync();

                Log.Information("FollowUp closed. FollowUpId: {FollowUpId}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error closing follow-up {FollowUpId}", id);
                throw;
            }
        }

        private async Task LoadInspectionsAsync()
        {
            var inspections = await _db.Inspections
                .Include(i => i.Premises)
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync();

            var items = inspections
                .Where(i => i.Premises != null)
                .Select(i => new
                {
                    i.Id,
                    Display = $"{i.InspectionDate:yyyy-MM-dd} - {i.Premises!.Name} ({i.Outcome})"
                })
                .ToList();

            ViewBag.InspectionId = new SelectList(items, "Id", "Display");
        }
    }
}