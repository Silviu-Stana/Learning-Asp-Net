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
        private readonly Infrastructure.Repositories.IListingRepository _listings;
        private readonly Infrastructure.Repositories.IImageRepository _images;

        public ListingsController(IScraper scraper, WebScraperDbContext db, Infrastructure.Repositories.IListingRepository listings, Infrastructure.Repositories.IImageRepository images)
        {
            _scraper = scraper;
            _db = db;
            _listings = listings;
            _images = images;
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

        /// <summary>
        /// Export the latest listings in the requested format. Supported: csv, json, excel
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] string format = "csv")
        {

            format = (format ?? string.Empty).ToLowerInvariant();
            var items = (await _listings.GetLatestAsync(100)).ToList();
            
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(items));

            if (!items.Any())
            {
                return NotFound(new { message = "No listings available for export." });
            }

            if (format == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(items);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", "listings.json");
            }
            

            // Build CSV
            string Escape(string? s)
            {
                if (s == null) return string.Empty;
                var v = s.Replace("\"", "\"\"");
                if (v.Contains(',') || v.Contains('\n') || v.Contains('\r') || v.Contains('"'))
                    return "\"" + v + "\"";
                return v;
            }

            var csvLines = new List<string>();
            csvLines.Add("Id,Title,Price,Currency,Location,SellerName,Views,OriginalUrl,ExtractionDate,ListingId,Description");
            foreach (var it in items)
            {
                csvLines.Add(string.Join(",",
                    it.Id.ToString(),
                    Escape(it.Title),
                    it.Price.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Escape(it.Currency),
                    Escape(it.Location),
                    Escape(it.SellerName),
                    string.IsNullOrEmpty(it.Views) ? "0" : it.Views,
                    Escape(it.OriginalUrl),
                    it.ExtractionDate.ToString("o"),
                    Escape(it.ListingId),
                    Escape(it.Description)
                ));
            }

            var csv = string.Join("\r\n", csvLines);
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv);

            if (format == "excel")
            {
                // return CSV with excel mime and .xlsx extension (Excel will open CSV)
                return File(csvBytes, "application/vnd.ms-excel", "listings.xlsx");
            }

            // default csv
            return File(csvBytes, "text/csv", "listings.csv");
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
                listingUrls = await _scraper.ExtractListingUrlsFromSearchAsync(request.Url, request.Results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to extract listing URLs", details = ex.Message });
            }

            foreach (var url in listingUrls.Take(10))
            {
                try
                {
                    // Check if this URL already exists in DB to skip scraping
                    var existsBefore = await _listings.GetByOriginalUrlAsync(url);
                    if (existsBefore != null)
                    {
                        skipped++;
                        details.Add(new { url, status = "skipped", reason = "exists", existingId = existsBefore.Id });
                        continue;
                    }

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

                    await _listings.AddAsync(scraped);
                    await _listings.SaveChangesAsync();
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
                // Support optional results count
                var take = (request.Results <= 0) ? 10 : request.Results;
                var urls = await _scraper.ExtractListingUrlsFromSearchAsync(request.Url, take);
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
