using Domain.Entities;

namespace Application.Interfaces
{
    public interface IScraper
    {
        Task<Listing> ScrapeListingAsync(string url);
        Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl, int maxResults = 10);
        Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl);
    }
}