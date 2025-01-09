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
    
    public required virtual DbSet<User> Users { get; set; }
    public required virtual DbSet<Role> Roles { get; set; }
    public required virtual DbSet<UserRoles> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasDatabaseName("IX_User_Email")
            .IsUnique();

        modelBuilder.Entity<UserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRoles>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRoles>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        base.OnModelCreating(modelBuilder);
    }
}