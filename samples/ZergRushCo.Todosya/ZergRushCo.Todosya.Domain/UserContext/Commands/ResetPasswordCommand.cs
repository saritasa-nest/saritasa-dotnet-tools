using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
{
    /// <summary>
    /// Reset user's password. Required on password recovery.
    /// </summary>
    public class ResetPasswordCommand
    {
        /// <summary>
        /// User's email to restore password.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// New user's password.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirm new user's password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Confirm Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        /// <summary>
        /// Special generated recovery token for user.
        /// </summary>
        public string Code { get; set; }
    }
}
