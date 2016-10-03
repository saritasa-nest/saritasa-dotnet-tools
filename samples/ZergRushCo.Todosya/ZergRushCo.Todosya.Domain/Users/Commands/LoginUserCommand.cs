using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    public class LoginUserCommand
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public int UserId { get; set; }
    }
}
