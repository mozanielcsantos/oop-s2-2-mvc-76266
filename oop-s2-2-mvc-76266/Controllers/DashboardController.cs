using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;
using OopS22Mvc76266.Web.ViewModels;

namespace OopS22Mvc76266.Web.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? town, string? riskRating)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var premisesQuery = _db.Premises.AsQueryable();

        if (!string.IsNullOrWhiteSpace(town))
        {
            premisesQuery = premisesQuery.Where(p => p.Town == town);
        }

        if (!string.IsNullOrWhiteSpace(riskRating))
        {
            premisesQuery = premisesQuery.Where(p => p.RiskRating == riskRating);
        }

        var filteredPremisesIds = await premisesQuery
            .Select(p => p.Id)
            .ToListAsync();

        var inspectionsThisMonth = await _db.Inspections
            .Where(i => filteredPremisesIds.Contains(i.PremisesId))
            .Where(i => i.InspectionDate >= monthStart)
            .CountAsync();

        var failedInspectionsThisMonth = await _db.Inspections
            .Where(i => filteredPremisesIds.Contains(i.PremisesId))
            .Where(i => i.InspectionDate >= monthStart && i.Outcome == "Fail")
            .CountAsync();

        var overdueOpenFollowUps = await _db.FollowUps
            .Include(f => f.Inspection)
            .Where(f => f.Inspection != null && filteredPremisesIds.Contains(f.Inspection.PremisesId))
            .Where(f => f.Status == "Open" && f.DueDate < now)
            .CountAsync();

        var vm = new DashboardViewModel
        {
            Town = town,
            RiskRating = riskRating,
            AvailableTowns = await _db.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(),
            AvailableRiskRatings = await _db.Premises
                .Select(p => p.RiskRating)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync(),
            InspectionsThisMonth = inspectionsThisMonth,
            FailedInspectionsThisMonth = failedInspectionsThisMonth,
            OverdueOpenFollowUps = overdueOpenFollowUps
        };

        return View(vm);
    }
}