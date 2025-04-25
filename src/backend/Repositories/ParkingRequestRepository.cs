using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class ParkingRequestRepository : IParkingRequestRepository
    {
        private readonly ParkingDbContext _context;

        public ParkingRequestRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingRequest> AddAsync(ParkingRequest request)
        {
            _context.ParkingRequests.Add(request);
            // Save handled by service layer's Unit of Work
            return await Task.FromResult(request); // Return the added entity
        }

        public async Task<ParkingRequest?> GetByIdAsync(int requestId)
        {
            return await _context.ParkingRequests
                                 .Include(r => r.User) // Include user if needed by service/caller
                                 .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<IEnumerable<ParkingRequest>> GetByUserIdAndDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.ParkingRequests
                                 .Where(pr => pr.UserId == userId && pr.RequestedDate >= startDate && pr.RequestedDate <= endDate)
                                 .OrderBy(pr => pr.RequestedDate)
                                 .ToListAsync();
        }

        public void Delete(ParkingRequest request)
        {
            _context.ParkingRequests.Remove(request);
            // Save handled by service layer's Unit of Work
        }
    }
}