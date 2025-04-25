using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models
{
    // Represents a physical parking location (e.g., Building A Garage, North Lot)
    public class ParkingLot
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; } // Optional address

        public int Priority { get; set; } = 0; // Lower number = higher priority for allocation

        // Navigation property
        public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; } = new List<ParkingSpace>();
    }
}