using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.BoringWarehouse.Domain;

namespace Saritasa.BoringWarehouse.DataAccess
{
    /// <summary>
    /// Database context initializer.
    /// </summary>
    public class AppDbContextInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            // TODO: Add admin user
        }
    }
}
