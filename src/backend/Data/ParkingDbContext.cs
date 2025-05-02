using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Models; // Using the models namespace

namespace TriviumParkingApp.Backend.Data;

public class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<ParkingLot> ParkingLots { get; set; } = null!;
    public DbSet<ParkingSpace> ParkingSpaces { get; set; } = null!;
    public DbSet<ParkingRequest> ParkingRequests { get; set; } = null!;
    public DbSet<Allocation> Allocations { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.FirebaseUid)
            .IsUnique();
    }
}