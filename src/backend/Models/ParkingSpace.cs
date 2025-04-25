using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models
{
    // Represents an individual parking space within a ParkingLot
    public class ParkingSpace
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(50)]
        public string SpaceNumber { get; set; } = string.Empty; // e.g., "A101", "12", "Visitor 3"

        public int ParkingLotId { get; set; } // Foreign Key
        [ForeignKey("ParkingLotId")]
        public virtual ParkingLot ParkingLot { get; set; } = null!; // Navigation property

        public bool IsPrioritySpace { get; set; } = false; // Indicates if this space is prioritized for specific roles

        [MaxLength(255)]
        public string? Notes { get; set; } // Any specific notes about the space (e.g., EV charging, compact only)

        // Navigation property
        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}