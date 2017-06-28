using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Saritasa.Tools.Common.Utils;
using Saritasa.Tools.Common.Extensions;

namespace ZergRushCo.Todosya.Domain.UserContext.Entities
{
    /// <summary>
    /// Application user entity.
    /// </summary>
    public class User : IdentityUser
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

        public void Clean()
        {
            Email = StringUtils.NullSafe(Email).ToLowerInvariant().Trim();
            UserName = Email;
            FirstName = StringUtils.NullSafe(FirstName).Trim();
            LastName = StringUtils.NullSafe(LastName).Trim();
        }
    }
}
