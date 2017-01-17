using System;

namespace SandBox.Commands
{
    /// <summary>
    /// Test command about product update.
    /// </summary>
    public class UpdateProductCommand
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public DateTime? BestBefore { get; set; }
    }
}
