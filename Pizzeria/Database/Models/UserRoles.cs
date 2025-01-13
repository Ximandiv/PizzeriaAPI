using System.ComponentModel.DataAnnotations.Schema;

namespace Pizzeria.Database.Models
{
    public class UserRoles
    {
        [ForeignKey("RoleId")]
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
