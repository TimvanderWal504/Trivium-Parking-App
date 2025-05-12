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

    public async Task<IEnumerable<ParkingLot>> GetAllParkingLotsAsync(bool includeSpaces = false)
    {
        try
        {
            return await _parkingLotRepository.GetAllAsync(includeSpaces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parking lots in service.");
            throw;
        }
    }

}