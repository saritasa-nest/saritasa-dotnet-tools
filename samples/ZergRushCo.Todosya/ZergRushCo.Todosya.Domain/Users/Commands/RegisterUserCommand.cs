using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    /// <summary>
    /// Register user command.
    /// </summary>
    public class RegisterUserCommand
    {
        /// <summary>
        /// User id. Output property.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User's email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// User's password.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// User should confirm his password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// User's country.
        /// </summary>
        [MaxLength(120)]
        public string Country { get; set; }

        /// <summary>
        /// User's city.
        /// </summary>
        [MaxLength(120)]
        public string City { get; set; }

        /// <summary>
        /// User's birthday.
        /// </summary>
        [Display(Name = "Birth Day")]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Identity result for ASP.NET Identity.
        /// </summary>
        public IdentityResult Result { get; set; }
    }
}
