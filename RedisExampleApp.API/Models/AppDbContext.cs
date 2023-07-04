using Microsoft.EntityFrameworkCore;

namespace RedisExampleApp.API.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Seed Data
            modelBuilder.Entity<Product>().HasData(
                new Product() { Id = 1, Name = "Kalem 1", Price = 15 },
                new Product() { Id = 2, Name = "Kalem 2", Price = 20 },
                new Product() { Id = 3, Name = "Kalem 3", Price = 25 }
                );
        }


    }
}
