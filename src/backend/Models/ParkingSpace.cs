using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models;

public class ParkingSpace
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SpaceNumber { get; set; } = string.Empty;

    public int ParkingLotId { get; set; }

    [ForeignKey("ParkingLotId")]
    public virtual ParkingLot ParkingLot { get; set; } = null!;

    public int IsPrioritySpace { get; set; }

    [MaxLength(255)]
    public string? Notes { get; set; }

    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
}