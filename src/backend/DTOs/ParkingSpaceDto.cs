namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO representing a parking space.
/// </summary>
public class ParkingSpaceDto
{
    public int Id { get; set; }
    public string SpaceNumber { get; set; } = string.Empty;
    public int ParkingLotId { get; set; } // Keep FK for reference if needed client-side
    public int IsPrioritySpace { get; set; }
    public string? Notes { get; set; }
}