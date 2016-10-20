using System.Data.Common;
using System.Data.Entity;
using SQLite.CodeFirst;
using ZergRushCo.Todosya.Domain.Tasks.Entities;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public AppDbContext()
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
        /// Use Sqlite database initializer. True by default. We don't need it for testing.
        /// </summary>
        public bool UseSqliteDatabase { get; set; } = true;

        /// <summary>
        /// Users database set.
        /// </summary>
        public DbSet<User> Users { get; set; }

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
            if (UseSqliteDatabase)
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
        }
    }
}
