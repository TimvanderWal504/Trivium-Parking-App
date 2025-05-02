using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories;

public class ParkingLotRepository : IParkingLotRepository
{
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;

    public ParkingLotRepository(IDbContextFactory<ParkingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ParkingLot>> GetAllAsync(bool includeSpaces = false)
    {
        await using var context = _contextFactory.CreateDbContext();

        IQueryable<ParkingLot> query = context.ParkingLots;

        if (includeSpaces)
            query = query.Include(pl => pl.ParkingSpaces);

        return await query.OrderBy(pl => pl.Priority)
            .ThenBy(pl => pl.Name)
            .ToListAsync();
    }
}
