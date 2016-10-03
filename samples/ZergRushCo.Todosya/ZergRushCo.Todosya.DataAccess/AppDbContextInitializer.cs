using System;
using System.Data.Entity;
using SQLite.CodeFirst;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Database context initializer.
    /// </summary>
    public class AppDbContextInitializer : SqliteCreateDatabaseIfNotExists<AppDbContext>
    {
        public AppDbContextInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void Seed(AppDbContext context)
        {
        }
    }
}
