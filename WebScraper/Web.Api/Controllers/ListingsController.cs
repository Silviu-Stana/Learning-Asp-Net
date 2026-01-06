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
    }
}
