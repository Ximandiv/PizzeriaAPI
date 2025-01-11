using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Users
{
    public class UserResponse
    {
        [Required]
        [StringLength(64)]
        [MinLength(3)]
        public string Name { get; set; }

        [Required]
        [StringLength(64)]
        [MinLength(3)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(20)]
        [MinLength(10)]
        public string Phone { get; set; }

        [Required]
        [StringLength(40)]
        [MinLength(8)]
        public string Address { get; set; }

        public UserResponse(string name, string email, string phone, string address)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
        }
    }
}
