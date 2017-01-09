namespace Saritasa.BoringWarehouse.Domain.Users.Commands
{
    using System.ComponentModel.DataAnnotations;

    using Tools.Messages.Abstractions;

    using Entities;

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
