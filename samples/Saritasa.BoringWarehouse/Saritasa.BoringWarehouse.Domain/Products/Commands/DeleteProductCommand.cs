using System.ComponentModel.DataAnnotations;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    public class DeleteProductCommand
    {
        public DeleteProductCommand(int id)
        {
            ProductId = id;
        }

        [Key]
        public int ProductId { get; }
    }
}
