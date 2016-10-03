using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.BoringWarehouse.Domain.Users.Entities;
using static Candy.CollectionsExtensions;

namespace Saritasa.BoringWarehouse.Domain.Users.Queries
{
    public class UsersObjectQuery : BaseObjectQuery
    {
        public PagedResult<User> Search(IEnumerable<User> users)
        {
            // Get total record count
            long total = users.LongCount();
            // Filtering
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                // Find by name
                users = users.Where(u => u.FirstName.StartsWith(SearchPattern) || 
                                            u.LastName.StartsWith(SearchPattern) ||
                                            u.Email.StartsWith(SearchPattern) ||
                                            u.Phone.StartsWith(SearchPattern));
            }
            // Get count after filtering
            long filteredCount = users.LongCount();
            // Order
            switch (OrderColumn?.ToLower())
            {
                case "firstname":
                    users = users.Order(u => u.FirstName, SortOrder);
                    break;
                case "lastname":
                    users = users.Order(u => u.LastName, SortOrder);
                    break;
                case "role":
                    users = users.Order(u => u.Role.ToString(), SortOrder);
                    break;
                case "email":
                    users = users.Order(u => u.Email, SortOrder);
                    break;
                case "phone":
                    users = users.Order(u => u.Phone, SortOrder);
                    break;
                case "isactive":
                    users = users.Order(u => u.IsActive, SortOrder);
                    break;
                default:
                    users = users.Order(u => u.Id, SortOrder);
                    break;
            }
            users = users.Skip(Offset).Take(Limit);
            return new PagedResult<User>(users.ToList(), Offset, Limit, total, filteredCount);
        }
    }
}
