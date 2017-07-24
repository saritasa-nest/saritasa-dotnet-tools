using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Messages.Abstractions.Commands;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
{
    /// <summary>
    /// Register user command.
    /// </summary>
    public class RegisterUserCommand
    {
        /// <summary>
        /// User's email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
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
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Identity result for ASP.NET Identity.
        /// </summary>
        [CommandOut]
        public IdentityResult Result { get; set; }

        /// <summary>
        /// Out user's instance.
        /// </summary>
        [CommandOut]
        public User User { get; set; }
    }
}
