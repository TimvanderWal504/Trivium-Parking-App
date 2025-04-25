using TriviumParkingApp.Backend.DTOs; // Use DTOs

namespace TriviumParkingApp.Backend.Services
{
    public interface IParkingLotService
    {
        /// <summary>
        /// Gets all parking lots, optionally including their spaces, mapped to DTOs.
        /// </summary>
        /// <param name="includeSpaces">Whether to include space details.</param>
        /// <returns>A collection of ParkingLotDto.</returns>
        Task<IEnumerable<ParkingLotDto>> GetAllParkingLotsAsync(bool includeSpaces = false);

        // TODO: Add methods for admin operations (Create, Update, Delete) later
    }
}