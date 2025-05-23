using Microsoft.Extensions.Logging;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;

namespace TriviumParkingApp.Backend.Services;

public class ParkingLotService : IParkingLotService
{
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly ILogger<ParkingLotService> _logger;

    public ParkingLotService(IParkingLotRepository parkingLotRepository, ILogger<ParkingLotService> logger)
    {
        _parkingLotRepository = parkingLotRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ParkingLotDto>> GetAllParkingLotsAsync(bool includeSpaces = false)
    {
        try
        {
            var parkingLots = await _parkingLotRepository.GetAllAsync(includeSpaces);

            return parkingLots.Select(pl => MapParkingLotToDto(pl, includeSpaces));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parking lots in service.");
            throw;
        }
    }

    private ParkingLotDto MapParkingLotToDto(ParkingLot lot, bool includeSpaces)
    {
        var dto = new ParkingLotDto
        {
            Id = lot.Id,
            Name = lot.Name,
            Address = lot.Address,
            Priority = lot.Priority
        };

        if (includeSpaces && lot.ParkingSpaces != null)
        {
            dto.ParkingSpaces = lot.ParkingSpaces.Select(ps => new ParkingSpaceDto
            {
                Id = ps.Id,
                SpaceNumber = ps.SpaceNumber,
                ParkingLotId = ps.ParkingLotId,
                Notes = ps.Notes
            }).ToList();
        }

        return dto;
    }
}