using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class ParkingLotRepository : IParkingLotRepository
    {
        private readonly ParkingDbContext _context;

        public ParkingLotRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingLot>> GetAllAsync(bool includeSpaces = false)
        {
            IQueryable<ParkingLot> query = _context.ParkingLots;

            if (includeSpaces)
            {
                query = query.Include(pl => pl.ParkingSpaces);
            }

            return await query.OrderBy(pl => pl.Priority)
                              .ThenBy(pl => pl.Name)
                              .ToListAsync();
        }
    }
}