namespace Saritasa.BoringWarehouse.Domain.Products.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Commands;
    using Entities;
    using Users.Entities;
    using Tools.Commands;
    using Tools.Exceptions;

    /// <summary>
    /// Product handlers.
    /// </summary>
    [CommandHandlers]
    public class ProductHandlers
    {
        public void HandleCreate(CreateProductCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (IAppUnitOfWork uow = uowFactory.Create())
            {
                Company company = uow.CompanyRepository.Get(command.CompanyId);
                if (company == null)
                {
                    throw new DomainException("The assigned company not found");
                }
                User creator = uow.Users.FirstOrDefault(u => u.Id == command.CreatedByUserId);
                if (creator == null)
                {
                    throw new DomainException("Can not find creator");
                }
                var product = new Product
                {
                    Company = company,
                    Name = command.Name,
                    Comment = command.Comment,
                    Quantity = command.Quantity,
                    Sku = command.Sku,
                    Properties = new List<ProductProperty>(command.Properties),
                    IsActive = command.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = creator
                };
                uow.ProductRepository.Add(product);
                uow.Complete();
                command.ProductId = product.Id;
            }
        }

        public void HandleDelete(DeleteProductCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (IAppUnitOfWork uow = uowFactory.Create())
            {
                Product product = uow.ProductRepository.Get(command.ProductId, Product.IncludeAll);
                if (product == null)
                {
                    throw new NotFoundException("Deleted product not found");
                }
                // Delete properties before
                uow.ProductPropertyRepository.RemoveRange(product.Properties);
                uow.ProductRepository.Remove(product);
                uow.Complete();
            }
        }

        public void HandleUpdate(UpdateProductCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (IAppUnitOfWork uow = uowFactory.Create())
            {
                Product product = uow.ProductRepository.Get(command.ProductId, Product.IncludeAll);
                Company company = uow.CompanyRepository.Get(command.CompanyId);
                if (company == null)
                {
                    throw new DomainException("The assigned company not found");
                }
                User updater = uow.Users.FirstOrDefault(u => u.Id == command.UpdatedByUserId);
                if (updater == null)
                {
                    throw new DomainException("Can not find updater");
                }
                // Delete properties
                foreach (ProductProperty removedProperty in product.Properties.Where(oldP => !command.Properties.Any(newP => newP.Id == oldP.Id)).ToList())
                {
                    product.Properties.Remove(removedProperty);
                    uow.ProductPropertyRepository.Remove(removedProperty);
                }
                // Update existing properties
                foreach (ProductProperty existProperty in product.Properties)
                {
                    ProductProperty updatedProperty = command.Properties.SingleOrDefault(pp => pp.Id == existProperty.Id);
                    existProperty.Name = updatedProperty.Name;
                    existProperty.Value = updatedProperty.Value;
                }
                // Add new properties
                foreach (ProductProperty property in command.Properties.Where(pp => pp.Id == 0))
                {
                    product.Properties.Add(property);
                }
                product.Company = company;
                product.Name = command.Name;
                product.Comment = command.Comment;
                product.Quantity = command.Quantity;
                product.Sku = command.Sku;
                product.IsActive = command.IsActive;
                product.UpdatedAt = DateTime.Now;
                uow.Complete();
            }
        }
    }
}
