using Microsoft.EntityFrameworkCore;
using Pizzeria.Database.Models;

namespace Pizzeria.Database;

public class PizzeriaContext : DbContext
{
    public PizzeriaContext(DbContextOptions<PizzeriaContext> options)
        : base(options)
    { }

    public PizzeriaContext()
    { }
    
    public DbSet<User> Users { get; set; }
}