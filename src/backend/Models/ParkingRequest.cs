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
    
    [Required, MaxLength(2)]
    public string CountryIsoCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

}