using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;

namespace OopS22Mvc76266.Web.Controllers
{
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
    }
}