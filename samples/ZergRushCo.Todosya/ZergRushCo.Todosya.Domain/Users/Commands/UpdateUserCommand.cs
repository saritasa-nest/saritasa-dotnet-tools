using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    /// <summary>
    /// Update user command.
    /// </summary>
    public class UpdateUserCommand
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public UpdateUserCommand()
        {
        }

        /// <summary>
        /// .ctor to fill from user entity.
        /// </summary>
        /// <param name="user">User entity.</param>
        public UpdateUserCommand(User user)
        {
            UserId = user.Id;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Country = user.Country;
            City = user.City;
            BirthDay = user.BirthDay;
            HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
        }

        /// <summary>
        /// User id we are going update to.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// New user email. Should be unique in system.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// New user password. Leave empty to avoid change.
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordHash")]
        public string Password { get; set; }

        /// <summary>
        /// User should confirm new password. Leave empty to avoid change.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("PasswordHash", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nwe user first name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// New user last name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// New user country.
        /// </summary>
        [MaxLength(120)]
        public string Country { get; set; }

        /// <summary>
        /// New user city.
        /// </summary>
        [MaxLength(120)]
        public string City { get; set; }

        /// <summary>
        /// Updated user birthday.
        /// </summary>
        [Display(Name = "Birth Day")]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Need the field only for ASP.NET Identity.
        /// </summary>
        public bool HasPassword { get; set; }
    }
}
