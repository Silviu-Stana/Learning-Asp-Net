using Domain.Entities;

namespace Application.Interfaces
{
    public interface IScraper
    {
        Task<Listing> ScrapeListingAsync(string url);
        Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl);
        Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl);
    }
}