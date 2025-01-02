namespace Pizzeria.Database.Seeders;

public class TestContextSeeder(PizzeriaContext context)
{
    public void Seed()
    {
        if (!context.Users.Any())
        {
            UserSeeder.Seed(context, 10);
        }
    }
}