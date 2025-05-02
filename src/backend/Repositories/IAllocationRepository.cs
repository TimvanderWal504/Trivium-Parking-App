using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories;

public interface IAllocationRepository
{
    /// <summary>
    /// Adds multiple new allocations to the database.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Allocation> allocations);

    /// <summary>
    /// Gets allocations for a all users for a specific date, including related entities.
    /// </summary>
    Task<IEnumerable<Allocation>> GetByDateAsync(DateOnly startDate);
    /// <summary>
    /// Gets allocations for a specific user for a specific date, including related entities.
    /// </summary>
    Task<Allocation?> GetByUserIdAndByDateAsync(int userId, DateOnly startDate);
}