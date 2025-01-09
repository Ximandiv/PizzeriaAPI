namespace Pizzeria.Database.Seeders;

public class TestContextSeeder(PizzeriaContext context)
{
    public async Task Seed()
    {
        if (!context.Users.Any())
        {
            await UserSeeder.Seed(context, 10);
            await RoleSeeder.Seed(context);
        }
    }
}