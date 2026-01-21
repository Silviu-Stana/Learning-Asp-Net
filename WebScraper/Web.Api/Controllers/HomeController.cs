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
                    var listing = await GetListingOrScrapeAsync(listingUrl, () => olxScraper.ScrapeListingAsync(listingUrl));
                    if (listing != null)
                    {
                        combinedListings.Add(("From OLX", listing));
                    }
                }
            }
            else if (searchUrl.Contains("storia.ro"))
            {
                var storiaScraper = HttpContext.RequestServices.GetRequiredService<StoriaScraper>();
                var storiaListings = await storiaScraper.ExtractListingUrlsFromSearchAsync(searchUrl, 10);

                foreach (var listingUrl in storiaListings)
                {
                    var listing = await GetListingOrScrapeAsync(listingUrl, () => storiaScraper.ScrapeListingAsync(listingUrl));
                    if (listing != null)
                    {
                        combinedListings.Add(("From Storia", listing));
                    }
                }
            }

            return View(combinedListings);
        }

        public IActionResult Privacy() => View();

        private async Task SaveListingIfNeededAsync(Domain.Entities.Listing listing)
        {
            if (listing is null) return;

            var existing = await _listings.GetByOriginalUrlAsync(listing.OriginalUrl);
            if (existing == null && !string.IsNullOrEmpty(listing.ListingId))
            {
                existing = await _listings.GetByListingIdAsync(listing.ListingId);
            }

            if (existing != null) return;

            if (listing.Images != null)
            {
                foreach (var image in listing.Images)
                {
                    image.Listing = null; // prevent circular references when saving
                }
            }

            await _listings.AddAsync(listing);
            await _listings.SaveChangesAsync();
        }

        //Returns cached list from DB when available, otherwise Scrape it.
        private async Task<Domain.Entities.Listing?> GetListingOrScrapeAsync(string listingUrl, Func<Task<Domain.Entities.Listing>> scrapeOperation)
        {
            var existing = await _listings.GetByOriginalUrlAsync(listingUrl);
            if (existing != null) return existing;

            var listing = await scrapeOperation();
            await SaveListingIfNeededAsync(listing);
            return listing;
        }
    }
}
