namespace Saritasa.BoringWarehouse.DataAccess
{
    using System;
    using System.Data.Entity;

    using Domain.Products.Entities;
    using Domain.Users.Entities;

    /// <summary>
    /// Database context initializer.
    /// </summary>
    public class AppDbContextInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            // Add admin user
            var user = new User
            {
                FirstName = "admin",
                LastName = "admin",
                Email = "admin@admin.com",
                IsActive = true,
                Phone = "123",
                Role = UserRole.Admin,
                PasswordHashed = "SHA256$8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918"
            };
            context.Users.Add(user);
            // Prefill
            // Company
            var company = new Company
            {
                Name = "My Company",
                CreatedBy = user,
                CreatedAt = DateTime.Now
            };
            context.Companies.Add(company);
            // Products
            var random = new Random();
            for (int productIndex = 0; productIndex < 100; productIndex++)
            {
                var product = new Product
                {
                    Name = $"Product Name {productIndex}",
                    Comment = $"Comment for product {productIndex}",
                    IsActive = true,
                    Quantity = random.Next(1, 100),
                    Sku = $"Sku for product {productIndex}",
                    CreatedBy = user,
                    CreatedAt = DateTime.Now,
                    Company = company
                };
                // Add properties
                int propertiesCount = random.Next(0, 10);
                for (int propertyIndex = 0; propertyIndex < propertiesCount; propertyIndex++)
                {
                    var property = new ProductProperty
                    {
                        Name = $"Property #{propertyIndex}",
                        Value = $"Value #{propertyIndex}",
                        Product = product
                    };
                    product.Properties.Add(property);
                }
                // Add to context
                context.Products.Add(product);
            }
            base.Seed(context);
        }
    }
}
