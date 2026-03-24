using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;

namespace OopS22Mvc76266.Web.Controllers
{
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
    }
}