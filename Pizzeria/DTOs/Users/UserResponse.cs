using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Users
{
    public class UserResponse
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
        [StringLength(20)]
        [MinLength(10)]
        public required string Phone { get; set; }

        [Required]
        [StringLength(40)]
        [MinLength(8)]
        public required string Address { get; set; }

    }
}
