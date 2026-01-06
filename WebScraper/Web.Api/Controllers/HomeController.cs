using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories;
using Application.Services;
using System.Net.Http; // Add this using directive
using Application.Scrapers;

namespace Web.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IListingRepository _listings;

        public HomeController(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<IActionResult> Index(string? searchUrl)
        {
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                return View(new List<(string Source, Domain.Entities.Listing Listing)>());
            }

            var combinedListings = new List<(string Source, Domain.Entities.Listing Listing)>();

            if (searchUrl.Contains("olx.ro"))
            {
                var olxScraper = HttpContext.RequestServices.GetRequiredService<OlxScraper>();
                var olxListings = await olxScraper.ExtractListingUrlsFromSearchAsync(searchUrl, 10);

                foreach (var listingUrl in olxListings)
                {
                    var listing = await olxScraper.ScrapeListingAsync(listingUrl);
                    combinedListings.Add(("From OLX", listing));
                }
            }
            else if (searchUrl.Contains("storia.ro"))
            {
                var storiaScraper = HttpContext.RequestServices.GetRequiredService<StoriaScraper>();
                var storiaListings = await storiaScraper.ExtractListingUrlsFromSearchAsync(searchUrl, 10);

                foreach (var listingUrl in storiaListings)
                {
                    var listing = await storiaScraper.ScrapeListingAsync(listingUrl);
                    combinedListings.Add(("From Storia", listing));
                }
            }

            return View(combinedListings);
        }

        public IActionResult Privacy() => View();
    }
}
