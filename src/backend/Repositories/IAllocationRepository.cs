using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public interface IAllocationRepository
    {
        /// <summary>
        /// Adds multiple new allocations to the database.
        /// </summary>
        Task AddRangeAsync(IEnumerable<Allocation> allocations);

        /// <summary>
        /// Gets allocations for a specific user within a date range, including related entities.
        /// </summary>
        Task<IEnumerable<Allocation>> GetByUserIdAndDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate);

        // TODO: Add other methods if needed (e.g., GetAllocationsByDate)
    }
}