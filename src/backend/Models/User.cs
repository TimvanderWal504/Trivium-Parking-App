using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FirebaseUid { get; set; } = string.Empty; 

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; } 

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<ParkingRequest> ParkingRequests { get; set; } = new List<ParkingRequest>();
    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
}