namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for returning allocation details.
    /// </summary>
    public class AllocationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly AllocatedDate { get; set; }
        public DateTimeOffset AllocationTimestamp { get; set; }

        // Include details about the allocated space
        public int ParkingSpaceId { get; set; }
        public string ParkingSpaceNumber { get; set; } = string.Empty;
        public int ParkingLotId { get; set; }
        public string ParkingLotName { get; set; } = string.Empty;
        public string? ParkingLotAddress { get; set; }
    }
}