namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    /// <summary>
    /// Make task done/not-done.
    /// </summary>
    public class CheckTaskCommand
    {
        /// <summary>
        /// Task id.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// User id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Is task done.
        /// </summary>
        public bool IsDone { get; set; }
    }
}
