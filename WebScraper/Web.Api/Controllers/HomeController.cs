using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebScraperDbContext _db;

        public HomeController(WebScraperDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var listings = await _db.Listings.Include(l => l.Images).ToListAsync();
            return View(listings);
        }

        public IActionResult Privacy() => View();
    }
}
