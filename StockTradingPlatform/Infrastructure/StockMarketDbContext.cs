using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace Infrastructure
{
    public class StockMarketDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<BuyOrder> BuyOrders { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuyOrder>().ToTable(nameof(BuyOrders));
            modelBuilder.Entity<SellOrder>().ToTable(nameof(SellOrders));

            modelBuilder.Entity<BuyOrder>(entity =>
            {
                // OrderDate: Default to current date/time
                entity.Property(b => b.OrderDate)
                    .HasDefaultValueSql("GETDATE()");

                // Index on StockSymbol for faster lookups
                entity.HasIndex(b => b.StockSymbol)
                    .HasDatabaseName("IX_BuyOrder_StockSymbol");
            });
        }

    }
}
