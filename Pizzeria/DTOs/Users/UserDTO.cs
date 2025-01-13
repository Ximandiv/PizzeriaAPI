using Pizzeria.Database.Models;
using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Users
{
    public class UserDTO
    {
        [Required]
        [StringLength(64)]
        [MinLength(3)]
        public required string Name { get; set; }

        [Required]
        [StringLength(64)]
        [MinLength(3)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(maximumLength: 32, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 32 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,32}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public required string Password { get; set; }

        [Required]
        [StringLength(20)]
        [MinLength(10)]
        public required string Phone { get; set; }

        [Required]
        [StringLength(40)]
        [MinLength(8)]
        public required string Address { get; set; }

        public User ToEntity()
            => new()
            {
                Name = Name,
                Email = Email,
                Password = BCrypt.Net.BCrypt.HashPassword(Password),
                Phone = Phone,
                Address = Address
            };
    }
}
