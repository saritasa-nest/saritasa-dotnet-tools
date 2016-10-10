namespace Saritasa.BoringWarehouse.DataAccess
{
    using System.Data.Entity;

    using Domain.Products.Entities;
    using Domain.Users.Entities;

    /// <summary>
    /// Entity framework database context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext() 
            : base()
        {
            Database.SetInitializer(new AppDbContextInitializer());
        }

        public AppDbContext(string nameOrConnectionString) 
            : base(nameOrConnectionString)
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
