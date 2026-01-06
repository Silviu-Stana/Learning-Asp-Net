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
            var listings = await _listings.GetLatestAsync(10);
            return View(listings);
        }

        public IActionResult Privacy() => View();
    }
}
