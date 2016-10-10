namespace Saritasa.BoringWarehouse.Domain.Users.Commands
{
    using System.ComponentModel.DataAnnotations;

    using Tools.Commands;

    using Entities;

    /// <summary>
    /// Create user.
    /// </summary>
    public class CreateUserCommand
    {
        [Key]
        [CommandOut]
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

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public UserRole Role { get; set; }

        public string Phone { get; set; }
    }
}
