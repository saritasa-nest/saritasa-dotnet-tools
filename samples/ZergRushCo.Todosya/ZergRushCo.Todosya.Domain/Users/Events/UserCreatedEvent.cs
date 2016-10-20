using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Events
{
    /// <summary>
    /// User has been created.
    /// </summary>
    public class UserCreatedEvent
    {
        /// <summary>
        /// User entity.
        /// </summary>
        public User User { get; set; }
    }
}
