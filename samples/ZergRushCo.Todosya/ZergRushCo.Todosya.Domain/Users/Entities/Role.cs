using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.Users.Entities
{
    /// <summary>
    /// User role.
    /// </summary>
    public class Role : IRole<int>
    {
        public int Id { get; private set; }

        public string Name { get; set; }
    }
}
