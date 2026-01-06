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
            // Return only the last 10 listings by ExtractionDate (most recent first)
            var listings = await _db.Listings
                .Include(l => l.Images)
                .OrderByDescending(l => l.ExtractionDate)
                .Take(10)
                .ToListAsync();
            return View(listings);
        }

        public IActionResult Privacy() => View();
    }
}
