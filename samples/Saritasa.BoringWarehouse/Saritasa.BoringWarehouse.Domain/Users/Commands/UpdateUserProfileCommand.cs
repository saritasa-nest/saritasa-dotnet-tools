using Saritasa.BoringWarehouse.Domain.Users.Entities;
using Saritasa.Tools.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.BoringWarehouse.Domain.Users.Commands
{
    /// <summary>
    /// Create user.
    /// </summary>
    public class UpdateUserProfileCommand
    {
        public UpdateUserProfileCommand()
        {

        }

        public UpdateUserProfileCommand(User user)
        {
            UserId = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Phone = user.Phone;
        }

        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Phone { get; set; }
    }
}
