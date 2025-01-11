using Bogus;
using Pizzeria.Database.Models;

namespace Pizzeria.Database.Seeders;

internal static class RoleSeeder
{
    public static async Task Seed(PizzeriaContext context)
    {
        var roleUser = new Role()
        {
            Id = 1,
            Name = "user" 
        };

        var roleAdmin = new Role()
        {
            Id = 2,
            Name = "admin"
        };

        List<Role> roleList = new() { roleUser, roleAdmin };
        
        await context.Roles.AddRangeAsync(roleList);
        await context.SaveChangesAsync();
    } 
}