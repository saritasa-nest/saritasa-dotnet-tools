using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    public class RegisterUserCommand
    {
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(120)]
        public string Country { get; set; }

        [MaxLength(120)]
        public string City { get; set; }

        [Display(Name = "Birth Day")]
        public DateTime? BirthDay { get; set; }

        public IdentityResult Result { get; set; }
    }
}
