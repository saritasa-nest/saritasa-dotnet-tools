using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class CreateProjectCommand
    {
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(125)]
        public string Name { get; set; }

        [MaxLength(10)]
        [ColorValidation]
        public string Color { get; set; } = "#2225AD";

        public int CreatedByUserId { get; set; }
    }
}
