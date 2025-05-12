using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories;

public interface IParkingRequestRepository
{
    /// <summary>
    /// Adds a new parking request to the database.
    /// </summary>
    Task<ParkingRequest> AddAsync(ParkingRequest request);

    /// <summary>
    /// Gets a parking request by its ID.
    /// </summary>
    Task<ParkingRequest?> GetByIdAsync(int requestId);

    /// <summary>
    /// Gets all parking requests for a specific user within a date range.
    /// </summary>
    Task<IEnumerable<ParkingRequest>> GetByUserIdAndDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Gets all parking requests for a specific date. Only used by Allocation
    /// </summary>
    Task<IEnumerable<ParkingRequest>> GetByDateAsync(DateOnly startDate);

    /// <summary>
    /// Deletes a parking request.
    /// </summary>
    Task DeleteAsync(ParkingRequest request);
}