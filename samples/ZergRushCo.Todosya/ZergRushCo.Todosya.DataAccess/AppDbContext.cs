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
        public AppDbContext()
        {
        }

        public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public DbSet<Project> Projects { get; set; }

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
            modelBuilder.Entity<Task>()
                .HasRequired(c => c.Project)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasRequired(c => c.User)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
