using Microsoft.EntityFrameworkCore;
using NetCoreConfMad2023_API.Models;

namespace NetCoreConfMad2023_API.Data
{
    public class OnlineShoppingContext : DbContext
    {
        public OnlineShoppingContext(DbContextOptions<OnlineShoppingContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Products");
        }
    }
}
