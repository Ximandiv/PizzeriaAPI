using Microsoft.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace Pizzeria.Services;

public class UserService(PizzeriaContext context)
{
    public async Task<IEnumerable<User>> GetAll() => await context.Users.ToListAsync();

    public async Task<User?> Get(int id) => await context.Users.FindAsync(id);
}
