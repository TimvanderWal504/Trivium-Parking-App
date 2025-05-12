using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Models; // Using the models namespace

namespace TriviumParkingApp.Backend.Data;

public class ParkingDbContext : IdentityDbContext<User, Role, int>
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ParkingLot> ParkingLots { get; set; } = null!;
    public DbSet<ParkingSpace> ParkingSpaces { get; set; } = null!;
    public DbSet<ParkingRequest> ParkingRequests { get; set; } = null!;
    public DbSet<Allocation> Allocations { get; set; } = null!;
    public DbSet<RoleParkingLot> RoleParkingLots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.FirebaseUid)
            .IsUnique();

        modelBuilder.Entity<RoleParkingLot>()
            .HasKey(rpl => new { rpl.RoleId, rpl.ParkingLotId });

        modelBuilder.Entity<RoleParkingLot>()
            .HasOne(rpl => rpl.Role)
            .WithMany(r => r.RoleParkingLots)
            .HasForeignKey(rpl => rpl.RoleId);

        modelBuilder.Entity<RoleParkingLot>()
            .HasOne(rpl => rpl.ParkingLot)
            .WithMany(pl => pl.RoleParkingLots)
            .HasForeignKey(rpl => rpl.ParkingLotId);

    }
}