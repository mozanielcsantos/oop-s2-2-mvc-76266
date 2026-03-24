using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;

namespace OopS22Mvc76266.Web.Controllers
{
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
    }
}