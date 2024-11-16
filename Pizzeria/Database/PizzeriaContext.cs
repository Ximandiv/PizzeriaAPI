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
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(64);
        modelBuilder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(64);
        modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(64);
        modelBuilder.Entity<User>().Property(u => u.Phone).IsRequired().HasMaxLength(64);
        modelBuilder.Entity<User>().Property(u => u.Address).IsRequired().HasMaxLength(64);
    }
}