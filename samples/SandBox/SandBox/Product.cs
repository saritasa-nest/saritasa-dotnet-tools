using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBox
{
    /// <summary>
    /// Product entity.
    /// </summary>
    public class Product
    {
        public int Id { get; private set; }

        public string Name { get; set; }

        public DateTime? BestBefore { get; set; }

        public Product(int id, string name, DateTime? bestBefore = null)
        {
            Id = id;
            Name = name;
            BestBefore = bestBefore;
        }
    }
}
