namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for returning parking requests via the API.
/// </summary>
public class ParkingRequestResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly RequestedDate { get; set; }
    public DateTimeOffset RequestTimestamp { get; set; }
}
