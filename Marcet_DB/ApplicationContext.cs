using Marcet_DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Marcet_DB
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated(); // создать базу данных 
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .HasKey(u => u.CustomerId);
            modelBuilder.Entity<Product>()
                .HasKey(u => u.ProductId);
        }

    }
}
