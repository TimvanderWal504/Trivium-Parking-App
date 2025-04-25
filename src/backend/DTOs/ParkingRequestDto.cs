using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for creating a parking request.
    /// </summary>
    public class CreateParkingRequestDto
    {
        [Required(ErrorMessage = "Requested date is required.")]
        // TODO: Add custom validation attribute to ensure date is in the future / next week?
        public DateOnly? RequestedDate { get; set; }

        // Optional: Add PreferredParkingLotId if that feature is implemented
        // public int? PreferredParkingLotId { get; set; }
    }

     /// <summary>
    /// DTO for returning parking request details.
    /// </summary>
    public class ParkingRequestResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Keep user ID for reference
        public DateOnly RequestedDate { get; set; }
        public DateTimeOffset RequestTimestamp { get; set; }
        // Add other relevant details if needed, e.g., status
    }
}