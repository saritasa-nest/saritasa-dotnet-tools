namespace ZergRushCo.Todosya.Domain.UserContext.Entities
{
    /// <summary>
    /// Possible user roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Regular registered user. Has access to common site functionality.
        /// </summary>
        Regular,

        /// <summary>
        /// Admin user. Can disable/edit other users.
        /// </summary>
        Admin
    }
}
