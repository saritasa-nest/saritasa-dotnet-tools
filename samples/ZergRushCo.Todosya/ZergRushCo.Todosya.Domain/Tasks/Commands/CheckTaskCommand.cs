namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    /// <summary>
    /// Make task done/not-done.
    /// </summary>
    public class CheckTaskCommand
    {
        public int TaskId { get; set; }

        public int UserId { get; set; }

        public bool IsDone { get; set; }
    }
}
