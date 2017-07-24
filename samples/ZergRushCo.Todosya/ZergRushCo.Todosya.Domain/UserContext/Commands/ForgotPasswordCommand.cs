using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
{
    /// <summary>
    /// Forgot password command. Allows to restore password by email address.
    /// </summary>
    public class ForgotPasswordCommand
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
