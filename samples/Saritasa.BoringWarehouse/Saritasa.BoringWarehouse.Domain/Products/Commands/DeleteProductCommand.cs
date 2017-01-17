namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    using System.ComponentModel.DataAnnotations;

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
