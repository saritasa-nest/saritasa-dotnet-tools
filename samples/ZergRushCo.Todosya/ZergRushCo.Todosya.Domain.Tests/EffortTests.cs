using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using ZergRushCo.Todosya.DataAccess;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.Tests
{
    /// <summary>
    /// Effort library tests. Mainly for demonstration purposes.
    /// </summary>
    public class EffortTests
    {
        [Fact]
        public void Omitting_SaveChanges_should_not_add_to_context()
        {
            var connection =
                Effort.DbConnectionFactory.CreatePersistent("Omitting_SaveChanges_should_not_add_to_context");

            // create user1
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                dbcontext.Users.Add(new User()
                {
                    FirstName = "First",
                    LastName = "Last",
                    Email = "test@saritasa.com",
                    UserName = "test@saritasa.com",
                    PasswordHash = "123",
                });
                dbcontext.SaveChanges();
            }

            // check it is created
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                Assert.Equal(1, dbcontext.Users.Count());
                Assert.False(string.IsNullOrEmpty(dbcontext.Users.First().Id));
            }

            // create another user but rollback
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                dbcontext.Users.Add(new User()
                {
                    FirstName = "First2",
                    LastName = "Last2",
                    Email = "test2@saritasa.com",
                    UserName = "test2@saritasa.com"
                });
            }

            // it is should not be added
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                Assert.Equal(1, dbcontext.Users.Count());
            }
        }

        [Fact]
        public void Transactions_should_be_able_to_rollback()
        {
            var connection = Effort.DbConnectionFactory.CreatePersistent("Transactions_should_be_able_to_rollback");

            // create user1 within transaction and rollback
            using (TransactionScope transaction = new TransactionScope())
            {
                using (var dbcontext = new AppDbContext(connection))
                {
                    dbcontext.UseSqliteDatabase = false;
                    dbcontext.Users.Add(new User()
                    {
                        FirstName = "First",
                        LastName = "Last",
                        Email = "test@saritasa.com",
                        UserName = "test@saritasa.com",
                        PasswordHash = "123",
                    });
                    dbcontext.SaveChanges();
                }
            }

            // check it is not created
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                Assert.Empty(dbcontext.Users);
            }

            // create real user
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                dbcontext.Users.Add(new User()
                {
                    FirstName = "First",
                    LastName = "Last",
                    Email = "test@saritasa.com",
                    UserName = "test@saritasa.com",
                    PasswordHash = "123",
                });
                dbcontext.SaveChanges();
            }

            // update it but rollback
            using (TransactionScope transaction = new TransactionScope())
            {
                using (var dbcontext = new AppDbContext(connection))
                {
                    dbcontext.UseSqliteDatabase = false;
                    var user = dbcontext.Users.FirstOrDefault();
                    user.FirstName = "Bender";
                    user.LastName = "Rodriguez";
                    dbcontext.SaveChanges();
                }
            }

            // test it is not updated
            using (var dbcontext = new AppDbContext(connection))
            {
                dbcontext.UseSqliteDatabase = false;
                var user = dbcontext.Users.FirstOrDefault();
                Assert.Equal("First", user.FirstName);
                Assert.Equal("Last", user.LastName);
            }
        }
    }
}