using Pizzeria.DTOs.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pizzeria.Database.Models;

public class User
{
    public int Id { get; set; }
    
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
    [StringLength(60)]
    public required string Password { get; set; }
    
    [Required]
    [StringLength(20)]
    [MinLength(10)]
    public required string Phone { get; set; }
    
    [Required]
    [StringLength(40)]
    [MinLength(8)]
    public required string Address { get; set; }
    
    [StringLength(64)]
    [JsonIgnore]
    public string? RememberMeToken { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserRoles>? UserRoles { get; set; }

    public UserResponse ToResponse()
        => new(Name, Email, Phone, Address);
}