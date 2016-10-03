namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class UpdateTaskCommand : CreateTaskCommand
    {
        public int Id { get; set; }
    }
}
