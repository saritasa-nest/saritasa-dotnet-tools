namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class RemoveProjectCommand
    {
        public int ProjectId { get; set; }

        public int UpdatedByUserId { get; set; }
    }
}
