namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO representing a parking lot, potentially including its spaces.
/// </summary>
public class ParkingLotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int Priority { get; set; }
    public List<ParkingSpaceDto> ParkingSpaces { get; set; } = new List<ParkingSpaceDto>(); // Include spaces
}