using Bogus;
using Pizzeria.Database.Models;

namespace Pizzeria.Database.Seeders;

internal static class UserSeeder
{
    public static async Task Seed(PizzeriaContext context, int amount = 1)
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Password, f => f.Internet.Password(32, false, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,32}$"))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber("(###) ###-####"))
            .RuleFor(u => u.Address, f => f.Address.StreetAddress());
        
        var users = faker.Generate(amount);
        
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    } 
}