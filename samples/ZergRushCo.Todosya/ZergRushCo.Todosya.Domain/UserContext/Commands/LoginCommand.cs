using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
{
    /// <summary>
    /// User login command.
    /// </summary>
    public class LoginCommand
    {
        /// <summary>
        /// User's email.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// User's password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Remember user's auth cookie.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
