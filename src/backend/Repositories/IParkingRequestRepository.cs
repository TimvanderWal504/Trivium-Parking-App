using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
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
        /// Deletes a parking request.
        /// </summary>
        void Delete(ParkingRequest request);

        // TODO: Add method to get pending requests for allocation if needed by AllocationService
        // Task<IEnumerable<ParkingRequest>> GetPendingRequestsForWeekAsync(DateOnly startDate, DateOnly endDate);
    }
}