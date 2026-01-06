using Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public interface IListingRepository : IRepository<Listing>
    {
        Task<Listing?> GetByOriginalUrlAsync(string originalUrl);
        Task<Listing?> GetByListingIdAsync(string listingId);
        Task<IEnumerable<Listing>> GetLatestAsync(int count);
    }
}
