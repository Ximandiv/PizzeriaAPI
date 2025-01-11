namespace Pizzeria.Database.Seeders;

public class TestContextSeeder(PizzeriaContext context)
{
    public async Task Seed()
    {
        if(!context.Roles.Any())
            await RoleSeeder.Seed(context);

        if (!context.Users.Any())
            await UserSeeder.Seed(context, 4);
    }
}