using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.Users.Entities
{
    /// <summary>
    /// Application user entity.
    /// </summary>
    public class User : IUser<int>
    {
        #region Application related

        /// <summary>
        /// User's first name.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string LastName { get; set; }

        /// <summary>
        /// User's country.
        /// </summary>
        [MaxLength(125)]
        public string Country { get; set; }

        /// <summary>
        /// User's city.
        /// </summary>
        [MaxLength(125)]
        public string City { get; set; }

        /// <summary>
        /// User's birthday.
        /// </summary>
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Is account active. Disabled account cannot login.
        /// </summary>
        public bool IsActive { get; set; } = true;

        #endregion

        #region IUser

        /// <summary>
        /// User ID (Primary Key)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User name. Usually email address is used.
        /// </summary>
        [EmailAddress]
        [Required]
        [MaxLength(125)]
        public string UserName { get; set; }

        #endregion

        #region Identity related

        /// <summary>
        /// The salted/hashed form of the user password.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Used to record failures for the purposes of lockout.
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// PhoneNumber for the user.
        /// </summary>
        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// True if the phone number is confirmed, default is false.
        /// </summary>
        public bool IsPhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// True if the email is confirmed, default is false.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// A random value that should change whenever a users credentials have changed (password
        /// changed, login removed).
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Is lockout enabled for this user.
        /// </summary>
        public bool IsLockoutEnabled { get; set; }

        /// <summary>
        /// DateTime in UTC when lockout ends, any time in the past is considered not locked.
        /// </summary>
        public DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// Is two factor enabled for the user.
        /// </summary>
        public bool IsTwoFactorEnabled { get; set; }

        #endregion

        #region Misc

        /// <summary>
        /// Date and time when user has been registered.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Date and time when user's account has been update.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        #endregion
    }
}
