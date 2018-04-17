using System.ComponentModel.DataAnnotations;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

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
