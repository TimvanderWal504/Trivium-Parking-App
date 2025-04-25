using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public interface IParkingLotRepository
    {
        /// <summary>
        /// Gets all parking lots, ordered by priority then name.
        /// </summary>
        /// <param name="includeSpaces">Whether to include the associated parking spaces.</param>
        /// <returns>A collection of ParkingLot entities.</returns>
        Task<IEnumerable<ParkingLot>> GetAllAsync(bool includeSpaces = false);

        // TODO: Add methods for GetById, Add, Update, Delete if needed for admin functionality
    }
}