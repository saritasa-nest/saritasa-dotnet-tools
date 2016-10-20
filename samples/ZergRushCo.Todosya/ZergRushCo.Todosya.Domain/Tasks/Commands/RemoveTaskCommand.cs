namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    /// <summary>
    /// Remove task command.
    /// </summary>
    public class RemoveTaskCommand
    {
        /// <summary>
        /// Task id.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// User id who removes the command.
        /// </summary>
        public int UserId { get; set; }
    }
}
