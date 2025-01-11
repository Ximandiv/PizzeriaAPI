using Microsoft.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace Pizzeria.Services;

public class UserService(PizzeriaContext context)
{
    public async Task<IEnumerable<User>> GetAll() => await context.Users.ToListAsync();

    public async Task<User?> Get(int id) => await context.Users.FindAsync(id);

    public async Task<User?> GetByEmail(string email) => await context.Users
                                                                        .Include(u => u.UserRoles)!
                                                                        .ThenInclude(ur => ur.Role)
                                                                        .Where(u => u.Email == email)
                                                                        .FirstOrDefaultAsync();

    public async Task Create(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task AddDefaultRole(User user)
    {
        UserRoles userRoles = new();
        userRoles.UserId = user.Id;
        userRoles.RoleId = 1;

        await context.UserRoles.AddAsync(userRoles);
        await context.SaveChangesAsync();
    }
}
