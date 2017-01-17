using System.Data.Common;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using SQLite.CodeFirst;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class AppDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public AppDbContext() : base("AppDbContext")
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="nameOrConnectionString">Connection string name of connection string itself.</param>
        public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="connection">ADO.NET database connection.</param>
        public AppDbContext(DbConnection connection) : base(connection, true)
        {
        }

        /// <summary>
        /// Tasks database set.
        /// </summary>
        public DbSet<Task> Tasks { get; set; }

        /// <summary>
        /// Projects database set.
        /// </summary>
        public DbSet<Project> Projects { get; set; }

        /// <summary>
        /// Database model initializer.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (Database.Connection is System.Data.SQLite.SQLiteConnection)
            {
                var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<AppDbContext>(modelBuilder);
                Database.SetInitializer(sqliteConnectionInitializer);
            }

            modelBuilder.Entity<Task>()
                .HasRequired(c => c.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasRequired(c => c.User)
                .WithMany()
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
