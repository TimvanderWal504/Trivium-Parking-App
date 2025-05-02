using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for returning allocation details.
/// </summary>
public class AllocationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly AllocatedDate { get; set; }
    public DateTimeOffset AllocationTimestamp { get; set; }
    public int ParkingSpaceId { get; set; }
    public string? ParkingSpaceNotes { get; set; }
    public string ParkingSpaceNumber { get; set; } = string.Empty;
    public int ParkingLotId { get; set; }
    public string ParkingLotName { get; set; } = string.Empty;
    public string? ParkingLotAddress { get; set; }

    public AllocationResponseDto(Allocation allocation)
    {
        Id = allocation.Id;
        UserId = allocation.UserId;
        AllocatedDate = allocation.AllocatedDate;
        AllocationTimestamp = allocation.AllocationTimestamp;
        ParkingSpaceId = allocation.ParkingSpaceId;
        ParkingSpaceNotes = allocation.ParkingSpace?.Notes;
        ParkingSpaceNumber = allocation.ParkingSpace?.SpaceNumber ?? "N/A";
        ParkingLotId = allocation.ParkingSpace?.ParkingLotId ?? 0;
        ParkingLotName = allocation.ParkingSpace?.ParkingLot?.Name ?? "N/A";
        ParkingLotAddress = allocation.ParkingSpace?.ParkingLot?.Address;
    }
}