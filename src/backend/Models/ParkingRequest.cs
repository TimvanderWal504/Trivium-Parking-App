using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models
{
    // Represents a user's request for a parking space on a specific date
    public class ParkingRequest
    {
        public int Id { get; set; } // Primary Key

        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Navigation property

        [Required]
        public DateOnly RequestedDate { get; set; } // The specific date the parking is requested for

        public DateTimeOffset RequestTimestamp { get; set; } = DateTimeOffset.UtcNow; // Timestamp for FCFS logic

        // Optional: If users can request specific lots (might complicate allocation)
        // public int? PreferredParkingLotId { get; set; }
        // [ForeignKey("PreferredParkingLotId")]
        // public virtual ParkingLot? PreferredParkingLot { get; set; }

        // Optional: Status of the request (e.g., Pending, Fulfilled, Denied) - might be redundant if Allocation exists
        // public string Status { get; set; } = "Pending";
    }
}