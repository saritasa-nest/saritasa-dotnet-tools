using System.ComponentModel.DataAnnotations;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    /// <summary>
    /// Command to delete product by id.
    /// </summary>
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
