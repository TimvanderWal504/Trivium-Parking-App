using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Services
{
    public interface IParkingLotService
    {
        /// <summary>
        /// Gets all parking lots, optionally including their spaces, mapped to DTOs.
        /// </summary>
        /// <param name="includeSpaces">Whether to include space details.</param>
        /// <returns>A collection of ParkingLotDto.</returns>
        Task<IEnumerable<ParkingLot>> GetAllParkingLotsAsync(bool includeSpaces = false);
    }
}