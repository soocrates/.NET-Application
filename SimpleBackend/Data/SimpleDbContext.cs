using Microsoft.EntityFrameworkCore;
using SimpleBackend.Models;

namespace SimpleBackend.Data
{
    public class SimpleDbContext : DbContext
    {
        public SimpleDbContext(DbContextOptions<SimpleDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Categories with image and description
            modelBuilder.Entity<Category>().HasData(
                new Category
                { 
                    CategoryId = 1, 
                    Name = "Electronics", 
                    ImageUrl = "https://example.com/images/electronics.png", 
                    Description = "Devices and gadgets"
                },
                new Category
                { 
                    CategoryId = 2, 
                    Name = "Books", 
                    ImageUrl = "https://example.com/images/books.png", 
                    Description = "Fiction and non-fiction books"
                }
            );

            // Seed Products without setting the navigation property, only foreign key (CategoryId)
            modelBuilder.Entity<Product>().HasData(
                new Product
                { 
                    ProductId = 1, 
                    Name = "Smartphone", 
                    Price = 500, 
                    CategoryId = 1,  // Use only CategoryId for relationships
                    ImageUrl = "https://example.com/images/smartphone.png", 
                    Description = "Latest model smartphone with high performance" 
                },
                new Product
                { 
                    ProductId = 2, 
                    Name = "Laptop", 
                    Price = 1000, 
                    CategoryId = 1,  // Use only CategoryId for relationships
                    ImageUrl = "https://example.com/images/laptop.png", 
                    Description = "High-end laptop for professionals" 
                },
                new Product
                { 
                    ProductId = 3, 
                    Name = "Novel", 
                    Price = 20, 
                    CategoryId = 2,  // Use only CategoryId for relationships
                    ImageUrl = "https://example.com/images/novel.png", 
                    Description = "Popular fiction novel" 
                }
            );
        }
    }
}
