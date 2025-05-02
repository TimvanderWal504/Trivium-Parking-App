using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models;
public class ParkingLot
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public int Priority { get; set; } = 0;

    public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; } = new List<ParkingSpace>();
}