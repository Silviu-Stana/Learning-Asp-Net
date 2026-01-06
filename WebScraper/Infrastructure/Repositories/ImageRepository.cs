using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        public ImageRepository(WebScraperDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Image>> GetByListingIdAsync(int listingId)
        {
            return await _db.Images.Where(i => i.ListingId == listingId).ToListAsync();
        }
    }
}
