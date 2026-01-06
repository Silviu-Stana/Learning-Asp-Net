using Domain.Entities;

namespace Infrastructure.Repositories
{
    public interface IListingRepository : IRepository<Listing>
    {
        Task<Listing?> GetByOriginalUrlAsync(string originalUrl);
        Task<Listing?> GetByListingIdAsync(string listingId);
        Task<IEnumerable<Listing>> GetLatestAsync(int count);
    }
}
