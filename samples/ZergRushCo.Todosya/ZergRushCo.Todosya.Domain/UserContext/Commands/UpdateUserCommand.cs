using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.UserContext.Commands
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
        public string UserId { get; set; }

        /// <summary>
        /// New user email. Should be unique in system.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// New user first name.
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
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Need the field only for ASP.NET Identity.
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// ASP.NET MVC identity result of update.
        /// </summary>
        public IdentityResult Result { get; set; }
    }
}
