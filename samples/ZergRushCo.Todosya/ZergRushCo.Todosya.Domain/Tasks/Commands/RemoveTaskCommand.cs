namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class RemoveTaskCommand
    {
        public int TaskId { get; set; }

        public int UserId { get; set; }
    }
}
