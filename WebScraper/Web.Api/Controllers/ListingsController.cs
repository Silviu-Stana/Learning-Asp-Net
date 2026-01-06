using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IScraper _scraper;
        private readonly WebScraperDbContext _db;

        public ListingsController(IScraper scraper, WebScraperDbContext db)
        {
            _scraper = scraper;
            _db = db;
        }

        public record ScrapeRequest(string Url);

        public record BulkScrapeResult(int Created, int Skipped, IEnumerable<object> Details);

        [HttpPost]
        public async Task<IActionResult> ScrapeAndSave([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            // Scrape the listing
            Listing listing;
            try
            {
                listing = await _scraper.ScrapeListingAsync(request.Url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Scraping failed", details = ex.Message });
            }

            if (listing is null)
                return StatusCode(500, new { error = "Scraper returned no data" });

            // Prevent duplicates by OriginalUrl or external ListingId
            var exists = await _db.Listings
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.OriginalUrl == listing.OriginalUrl || (!string.IsNullOrEmpty(listing.ListingId) && l.ListingId == listing.ListingId));

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

            _db.Listings.Add(listing);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetListing(int id)
        {
            var listing = await _db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        /// <summary>
        /// Given a search page URL (e.g. https://www.olx.ro/oferte/q-KEYWORDS/),
        /// extract the first 10 listing URLs and scrape & save them.
        /// </summary>
        [HttpPost("from-search")]
        public async Task<IActionResult> ScrapeFromSearch([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            List<object> details = new();
            int created = 0, skipped = 0;

            IEnumerable<string> listingUrls;
            try
            {
                listingUrls = await _scraper.ExtractListingUrlsFromSearchAsync(request.Url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract listing URLs", details = ex.Message });
            }

            foreach (var url in listingUrls.Take(10))
            {
                try
                {
                    var scraped = await _scraper.ScrapeListingAsync(url);
                    if (scraped == null)
                    {
                        details.Add(new { url, status = "no-data" });
                        continue;
                    }

                    var exists = await _db.Listings.Include(l => l.Images)
                        .FirstOrDefaultAsync(l => l.OriginalUrl == scraped.OriginalUrl || (!string.IsNullOrEmpty(scraped.ListingId) && l.ListingId == scraped.ListingId));

                    if (exists != null)
                    {
                        skipped++;
                        details.Add(new { url, status = "skipped", reason = "exists", existingId = exists.Id });
                        continue;
                    }

                    if (scraped.Images != null)
                    {
                        foreach (var img in scraped.Images)
                        {
                            img.Listing = null; // avoid cycles
                        }
                    }

                    _db.Listings.Add(scraped);
                    await _db.SaveChangesAsync();
                    created++;
                    details.Add(new { url, status = "created", id = scraped.Id });
                }
                catch (Exception ex)
                {
                    details.Add(new { url, status = "error", details = ex.Message });
                }
            }

            return Ok(new BulkScrapeResult(created, skipped, details));
        }

        [HttpPost("extract-urls")]
        public async Task<IActionResult> ExtractUrls([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            IEnumerable<string> listingUrls;
            try
            {
                listingUrls = await _scraper.ExtractListingUrlsFromSearchAsync(request.Url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract listing URLs", details = ex.Message });
            }

            return Ok(listingUrls.Take(10));
        }

        [HttpPost("extract-keyword")]
        public async Task<IActionResult> ExtractKeyword([FromBody] ScrapeRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { error = "Url is required" });

            try
            {
                var kw = await _scraper.ExtractSearchKeywordFromUrlAsync(request.Url);
                if (kw == null) return NotFound(new { message = "No search keyword found" });
                return Ok(new { keyword = kw });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract keyword", details = ex.Message });
            }
        }
    }
}
