using FoodSafety.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;
using Serilog;

namespace OopS22Mvc76266.Web.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PremisesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var premises = await _db.Premises
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(premises);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Premises());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Premises premises)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Premises creation validation failed");
                    return View(premises);
                }

                _db.Premises.Add(premises);
                await _db.SaveChangesAsync();

                Log.Information("Premises created: {PremisesId} - {PremisesName}", premises.Id, premises.Name);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating premises");
                throw;
            }
        }
    }
}