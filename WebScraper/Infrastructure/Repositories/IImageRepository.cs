using Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public interface IImageRepository : IRepository<Image>
    {
        Task<IEnumerable<Image>> GetByListingIdAsync(int listingId);
    }
}
