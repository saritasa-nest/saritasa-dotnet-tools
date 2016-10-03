using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    public class UpdateUserCommand
    {
        public UpdateUserCommand()
        {
        }

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

        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordHash")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("PasswordHash", ErrorMessage = "The password and confirmation password do not match.")]
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

        public bool HasPassword { get; set; }
    }
}
