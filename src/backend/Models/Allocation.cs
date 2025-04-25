using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviumParkingApp.Backend.Models
{
    // Represents the result of the allocation process: a user assigned to a space for a date
    public class Allocation
    {
        public int Id { get; set; } // Primary Key

        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Navigation property

        public int ParkingSpaceId { get; set; } // Foreign Key
        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace ParkingSpace { get; set; } = null!; // Navigation property

        [Required]
        public DateOnly AllocatedDate { get; set; } // The specific date the space is allocated for

        public DateTimeOffset AllocationTimestamp { get; set; } = DateTimeOffset.UtcNow; // When the allocation was made

        // Optional: Could link back to the specific ParkingRequest if needed, though potentially redundant
        // public int? ParkingRequestId { get; set; }
        // [ForeignKey("ParkingRequestId")]
        // public virtual ParkingRequest? ParkingRequest { get; set; }
    }
}