namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    /// <summary>
    /// Update task command.
    /// </summary>
    public class UpdateTaskCommand : CreateTaskCommand
    {
        /// <summary>
        /// Task id is to be updated.
        /// </summary>
        public int Id { get; set; }
    }
}
