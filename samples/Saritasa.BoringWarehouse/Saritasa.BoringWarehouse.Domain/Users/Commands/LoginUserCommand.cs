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
    /// Login user with email and password.
    /// </summary>
    public class LoginUserCommand
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [CommandOut]
        public User User { get; set; }

        [CommandOut]
        public bool IsSuccess { get; set; }
    }
}
