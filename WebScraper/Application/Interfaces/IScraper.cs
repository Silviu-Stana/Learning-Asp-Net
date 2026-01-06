using Domain.Entities;

namespace Application.Interfaces
{
    public interface IScraper
    {
        Task<Listing> ScrapeListingAsync(string url);
    }
}