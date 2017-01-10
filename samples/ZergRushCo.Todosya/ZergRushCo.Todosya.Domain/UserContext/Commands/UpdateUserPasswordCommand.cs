using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
{
    /// <summary>
    /// Update user password.
    /// </summary>
    public class UpdateUserPasswordCommand
    {
        /// <summary>
        /// User id we are going update to.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Current user password.
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// New user password.
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// User should confirm new password. Leave empty to avoid change.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm new password")]
        public string NewPasswordConfirm { get; set; }

        /// <summary>
        /// ASP.NET MVC identity result of update.
        /// </summary>
        public IdentityResult Result { get; set; }
    }
}
