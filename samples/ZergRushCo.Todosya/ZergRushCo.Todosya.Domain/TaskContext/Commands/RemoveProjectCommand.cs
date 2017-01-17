namespace ZergRushCo.Todosya.Domain.TaskContext.Commands
{
    /// <summary>
    /// Remove project command.
    /// </summary>
    public class RemoveProjectCommand
    {
        /// <summary>
        /// Project id to remove.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// User id who removes the project.
        /// </summary>
        public string UpdatedByUserId { get; set; }
    }
}
