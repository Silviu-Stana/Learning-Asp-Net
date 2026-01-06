using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories;

namespace Web.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IListingRepository _listings;

        public HomeController(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<IActionResult> Index()
        {
            // Return only the last 10 listings by ExtractionDate (most recent first)
            // Show up to 50 most recent listings on the homepage
            var listings = await _listings.GetLatestAsync(50);
            return View(listings);
        }

        public IActionResult Privacy() => View();
    }
}
