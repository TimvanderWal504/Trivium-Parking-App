using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models;

public class ParkingRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [Required]
    public DateOnly RequestedDate { get; set; }

    public DateTimeOffset RequestTimestamp { get; set; } = DateTimeOffset.UtcNow;

}