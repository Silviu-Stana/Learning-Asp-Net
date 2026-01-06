using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly WebScraperDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(WebScraperDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _set.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> ListAsync()
        {
            return await _set.ToListAsync();
        }

        public virtual void Remove(T entity)
        {
            _set.Remove(entity);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
