using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class WebScraperDbContext : DbContext
    {
        public WebScraperDbContext(DbContextOptions<WebScraperDbContext> options) : base(options)
        {
        }

        public DbSet<Listing> Listings { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Listing>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Images)
                      .WithOne(e => e.Listing)
                      .HasForeignKey(e => e.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Price).HasPrecision(18, 2); // Precision: 18, Scale: 2
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}