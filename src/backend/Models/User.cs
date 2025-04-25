using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(255)] // Firebase UIDs are typically shorter, but good to have a limit
        public string FirebaseUid { get; set; } = string.Empty; // Link to Firebase Auth User

        [MaxLength(100)]
        public string? Email { get; set; } // Store email for reference?

        [MaxLength(100)]
        public string? DisplayName { get; set; } // Store display name?

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<ParkingRequest> ParkingRequests { get; set; } = new List<ParkingRequest>();
        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}