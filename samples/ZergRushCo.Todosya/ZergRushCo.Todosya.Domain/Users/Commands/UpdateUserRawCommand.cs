using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Commands
{
    public class UpdateUserRawCommand
    {
        public UpdateUserRawCommand()
        {
        }

        public UpdateUserRawCommand(User user)
        {
            UserId = user.Id;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Country = user.Country;
            City = user.City;
            BirthDay = user.BirthDay;
            PasswordHash = user.PasswordHash;
            HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
        }

        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordHash")]
        public string PasswordHash { get; set; }

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
