using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ListingRepository : Repository<Listing>, IListingRepository
    {
        public ListingRepository(WebScraperDbContext db) : base(db)
        {
        }

        public async Task<Listing?> GetByOriginalUrlAsync(string originalUrl)
        {
            return await _db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.OriginalUrl == originalUrl);
        }

        public async Task<Listing?> GetByListingIdAsync(string listingId)
        {
            if (string.IsNullOrEmpty(listingId)) return null;
            return await _db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.ListingId == listingId);
        }

        public async Task<IEnumerable<Listing>> GetLatestAsync(int count)
        {
            return await _db.Listings.Include(l => l.Images).OrderByDescending(l => l.ExtractionDate).Take(count).ToListAsync();
        }
    }
}
