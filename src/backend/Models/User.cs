using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models;

public class User : IdentityUser<int>
{
    [Required]
    [MaxLength(255)]
    public string FirebaseUid { get; set; } = string.Empty;

    [NotMapped]
    public override string UserName => FirebaseUid;   

    [MaxLength(100)]
    public override string? Email { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; } 

    public virtual ICollection<ParkingRequest> ParkingRequests { get; set; } = new List<ParkingRequest>();
    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
}