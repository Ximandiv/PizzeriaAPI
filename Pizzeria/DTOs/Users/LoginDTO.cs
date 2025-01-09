using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Users
{
    public class LoginDTO
    {
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
    }
}
