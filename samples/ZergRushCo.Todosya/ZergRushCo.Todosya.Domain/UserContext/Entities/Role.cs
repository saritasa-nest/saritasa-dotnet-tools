using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.UserContext.Entities
{
    /// <summary>
    /// User role.
    /// </summary>
    public class Role : IRole<int>
    {
        /// <summary>
        /// Unique role id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; }
    }
}
