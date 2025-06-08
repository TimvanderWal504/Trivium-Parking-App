using Microsoft.Extensions.Logging;
using TriviumParkingApp.Backend.DTOs;
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

    public async Task<IEnumerable<ParkingLotLocationResponseDTO>> GetAllParkingLotLocationsAsync(bool includeSpaces = false)
    {
        try
        {
            var result = await _parkingLotRepository.GetAllAsync(includeSpaces);
            return result
                .Select(x => new ParkingLotLocationResponseDTO
                {
                    City = x.City,
                    CountryIsoCode = x.CountryIsoCode
                })
                .DistinctBy(dto => new { dto.City, dto.CountryIsoCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parking lots in service.");
            throw;
        }
    }

}