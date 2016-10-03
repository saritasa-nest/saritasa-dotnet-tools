using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class UpdateProjectCommand
    {
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(125)]
        public string Name { get; set; }

        [MaxLength(10)]
        [ColorValidation]
        public string Color { get; set; }

        public int UpdatedByUserId { get; set; }
    }
}
