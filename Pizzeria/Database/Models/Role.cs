using System.ComponentModel.DataAnnotations;

namespace Pizzeria.Database.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public ICollection<UserRoles>? UserRoles { get; set; }
    }
}
