namespace Saritasa.BoringWarehouse.Web.Models
{
    using Domain.Products.Entities;

    public class AddProductPropertyTemplate
    {
        public AddProductPropertyTemplate(int index, ProductProperty property = null)
        {
            Property = property ?? new ProductProperty();
            Index = index;
        }

        public ProductProperty Property { get; }

        public int Index { get; }
    }
}
