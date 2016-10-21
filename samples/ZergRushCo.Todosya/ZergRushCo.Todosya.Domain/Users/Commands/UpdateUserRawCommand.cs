using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    /// <summary>
    /// Alternate command to update user. Is used mainly for ASP.NET Identity.
    /// </summary>
    public class UpdateUserRawCommand
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public UpdateUserRawCommand()
        {
        }

        /// <summary>
        /// .ctor to create from user entity.
        /// </summary>
        /// <param name="user">User entity.</param>
        public UpdateUserRawCommand(User user)
        {
            UserId = user.Id;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Country = user.Country;
            City = user.City;
            BirthDay = user.BirthDay;
            PasswordHash = user.PasswordHash;
            HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
        }

        /// <summary>
        /// User id to update.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// New user's email. Should be unique.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// New hashed password. Leave empty to avoid update.
        /// </summary>
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordHash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Updated user's first name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Updated user's last name.
        /// </summary>
        [Required]
        [MaxLength(120)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// Updated user's country.
        /// </summary>
        [MaxLength(120)]
        public string Country { get; set; }

        /// <summary>
        /// Udpdated user's city.
        /// </summary>
        [MaxLength(120)]
        public string City { get; set; }

        /// <summary>
        /// Updated user's birthday.
        /// </summary>
        [Display(Name = "Birth Day")]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Need the field only for ASP.NET Identity.
        /// </summary>
        public bool HasPassword { get; set; }
    }
}
