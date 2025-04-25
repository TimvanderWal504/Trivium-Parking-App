using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly ParkingDbContext _context;

        public AllocationRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Allocation> allocations)
        {
            await _context.Allocations.AddRangeAsync(allocations);
            // Save handled by service layer's Unit of Work
        }

        public async Task<IEnumerable<Allocation>> GetByUserIdAndDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.Allocations
                                 .Where(a => a.UserId == userId && a.AllocatedDate >= startDate && a.AllocatedDate <= endDate)
                                 .Include(a => a.ParkingSpace) // Include space details
                                     .ThenInclude(ps => ps.ParkingLot) // Include lot details from space
                                 .OrderBy(a => a.AllocatedDate)
                                 .ToListAsync();
        }
    }
}