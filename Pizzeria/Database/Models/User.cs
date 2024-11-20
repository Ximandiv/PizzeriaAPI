using System.ComponentModel.DataAnnotations;

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
    public required string Email { get; set; }
    
    [Required]
    [StringLength(20)]
    [MinLength(10)]
    public required string Phone { get; set; }
    
    [Required]
    [StringLength(40)]
    [MinLength(8)]
    public required string Address { get; set; }
    
    [StringLength(64)]
    public string? RememberMeToken { get; set; }
}