using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Services;

public interface IAllocationService
{
    /// <summary>
    /// Gets allocations for a specific user within a date range.
    /// </summary>
    /// <param name="userId">The internal database ID of the user.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A collection of AllocationResponseDto.</returns>
    Task<AllocationResponseDto?> GetUserAllocationsAsync(int userId, DateOnly startDate);

    /// <summary>
    /// Runs the daily parking allocation process.
    /// </summary>
    /// <param name="startOfWeek">The date to allocate for</param>
    /// <returns>A collection of the newly created allocations.</returns>
    Task<IEnumerable<Allocation>> RunDailyAllocationAsync(DateOnly date);
}