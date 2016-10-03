using System.Data.Entity;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.DataAccess
{
    /// <summary>
    /// Entity framework database context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
            Database.SetInitializer(new AppDbContextInitializer());
        }

        public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer(new AppDbContextInitializer());
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductProperty> ProductProperties { get; set; }

        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasRequired(c => c.CreatedBy)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasRequired(c => c.CreatedBy)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
