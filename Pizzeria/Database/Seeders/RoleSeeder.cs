using Bogus;
using Pizzeria.Database.Models;

namespace Pizzeria.Database.Seeders;

internal static class RoleSeeder
{
    public static async Task Seed(PizzeriaContext context)
    {
        var roleUser = new Role()
        {
            Name = "user" 
        };

        var roleAdmin = new Role()
        {
            Name = "admin"
        };

        List<Role> roleList = new() { roleUser, roleAdmin };
        
        await context.Roles.AddRangeAsync(roleList);
        await context.SaveChangesAsync();
    } 
}