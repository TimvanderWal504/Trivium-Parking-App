using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models;

public class Allocation
{
    public int Id { get; set; }

    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public int ParkingSpaceId { get; set; } 
    
    [ForeignKey("ParkingSpaceId")]
    public virtual ParkingSpace ParkingSpace { get; set; } = null!; 

    [Required]
    public DateOnly AllocatedDate { get; set; }

    public DateTimeOffset AllocationTimestamp { get; set; } = DateTimeOffset.UtcNow; 

}

