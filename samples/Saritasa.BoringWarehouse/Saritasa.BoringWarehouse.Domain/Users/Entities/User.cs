namespace Saritasa.BoringWarehouse.Domain.Users.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// User roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Regular user can just view what product are in warehouse.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Manager can add/edit products.
        /// </summary>
        Manager = 1,

        /// <summary>
        /// Admin can remove products and add/edit/disable users.
        /// </summary>
        Admin = 2
    }

    /// <summary>
    /// User.
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string PasswordHashed { get; set; }

        public UserRole Role { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
