using Application.Interfaces;
using Application.Scrapers;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IScraper _scraper;
        private readonly WebScraperDbContext _db;
        private readonly Infrastructure.Repositories.IListingRepository _listings;
        private readonly Infrastructure.Repositories.IImageRepository _images;
        private readonly IServiceProvider _services;

        public ListingsController(IScraper scraper, WebScraperDbContext db, Infrastructure.Repositories.IListingRepository listings, Infrastructure.Repositories.IImageRepository images, IServiceProvider services)
        {
            _scraper = scraper;
            _db = db;
            _listings = listings;
            _images = images;
            _services = services;
        }

        public record ScrapeRequest(string Url, int Results = 10);

        public record BulkScrapeResult(int Created, int Skipped, IEnumerable<object> Details);

        [HttpPost]
        public async Task<IActionResult> ScrapeAndSave([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            // Check database first to avoid unnecessary scraping
            var existing = await _db.Listings.Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.OriginalUrl == request.Url);
            if (existing != null)
            {
                return Conflict(new { message = "Listing already exists", listing = existing });
            }

            // Scrape the listing
            var scraper = ResolveScraper(request.Url);
            Listing listing;
            try
            {
                listing = await scraper.ScrapeListingAsync(request.Url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Scraping failed", details = ex.Message });
            }

            if (listing is null)
                return StatusCode(500, new { error = "Scraper returned no data" });

            // Prevent duplicates by OriginalUrl or external ListingId
            var exists = await _listings.GetByOriginalUrlAsync(listing.OriginalUrl)
                ?? await _listings.GetByListingIdAsync(listing.ListingId ?? string.Empty);

            if (exists != null)
            {
                return Conflict(new { message = "Listing already exists", listing = exists });
            }

            // Ensure Images collection is not null
            if (listing.Images != null)
            {
                foreach (var img in listing.Images)
                {
                    img.Listing = null; // avoid accidental cycles
                }
            }

            await _listings.AddAsync(listing);
            var saveResult = await _listings.SaveChangesAsync();

            // Debugging log
            System.Diagnostics.Debug.WriteLine($"ScrapeAndSave: Saved listing with ID {listing.Id}, SaveChanges result: {saveResult}");

            return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetListing(int id)
        {
            var listing = await _listings.GetByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        

        [HttpPost("extract-urls")]
        public async Task<IActionResult> ExtractUrls([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            IEnumerable<string> listingUrls;
            try
            {
                // Support optional results count
                var take = (request.Results <= 0) ? 10 : request.Results;
                var scraper = ResolveScraper(request.Url);
                var urls = await scraper.ExtractListingUrlsFromSearchAsync(request.Url, take);
                listingUrls = urls.Take(take);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract listing URLs", details = ex.Message });
            }

            return Ok(listingUrls);
        }

        [HttpPost("extract-keyword")]
        public async Task<IActionResult> ExtractKeyword([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            try
            {
                var scraper = ResolveScraper(request.Url);
                var kw = await scraper.ExtractSearchKeywordFromUrlAsync(request.Url);
                if (kw == null) return NotFound(new { message = "No search keyword found" });
                return Ok(new { keyword = kw });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract keyword", details = ex.Message });
            }
        }

        private IScraper ResolveScraper(string url)
        {
            var provider = HttpContext?.RequestServices ?? _services;
            if (!string.IsNullOrWhiteSpace(url) && url.Contains("storia.ro", StringComparison.OrdinalIgnoreCase))
            {
                return provider.GetRequiredService<StoriaScraper>();
            }

            if (!string.IsNullOrWhiteSpace(url) && url.Contains("olx.ro", StringComparison.OrdinalIgnoreCase))
            {
                return provider.GetRequiredService<OlxScraper>();
            }

            return _scraper;
        }
    }
}
