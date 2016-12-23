using System;
using SandBox.Commands;
using Saritasa.Tools.Messages.Abstractions;

namespace SandBox
{
    /// <summary>
    /// Commands handlers.
    /// </summary>
    [CommandHandlers]
    public class CommandsHandlers
    {
        public void HandleProductUpdate(UpdateProductCommand command, IProductsRepository productsRepository)
        {
            Console.WriteLine($"CommandsHandlers: Update product command is executed!");
            var product = productsRepository.Get(command.ProductId);
            if (product == null)
            {
                productsRepository.Add(new Product(command.ProductId, command.Name, command.BestBefore));
            }
            else
            {
                product.Name = command.Name;
                product.BestBefore = command.BestBefore;
            }
        }
    }
}
