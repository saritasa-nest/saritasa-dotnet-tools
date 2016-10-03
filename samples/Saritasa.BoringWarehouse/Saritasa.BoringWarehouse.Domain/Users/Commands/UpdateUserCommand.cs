using Saritasa.BoringWarehouse.Domain.Users.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.BoringWarehouse.Domain.Users.Commands
{
    public class UpdateUserCommand
    {
        public UpdateUserCommand()
        {

        }

        public UpdateUserCommand(User user)
        {
            UserId = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Phone = user.Phone;
            IsActive = user.IsActive;
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

        public string Phone { get; set; }

        public bool IsActive { get; set; }
    }
}
