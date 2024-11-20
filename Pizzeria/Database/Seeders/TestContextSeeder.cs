using Microsoft.EntityFrameworkCore;

namespace Pizzeria.Database.Seeders;

internal class TestContextSeeder(PizzeriaContext context)
{
    public void Seed()
    {
        if (!context.Users.Any())
        {
            UserSeeder.Seed(context, 10);
        }
    }
}