using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories;

public class AllocationRepository : IAllocationRepository
{
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;

    public AllocationRepository(IDbContextFactory<ParkingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddRangeAsync(IEnumerable<Allocation> allocations)
    {
        await using var context = _contextFactory.CreateDbContext();
        await context.Allocations.AddRangeAsync(allocations);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Allocation>> GetByDateAsync(DateOnly startDate)
    {
        await using var context = _contextFactory.CreateDbContext();

        return await context.Allocations
            .Where(a => a.AllocatedDate == startDate)
            .Include(a => a.ParkingSpace)
                .ThenInclude(ps => ps.ParkingLot)
            .OrderBy(a => a.AllocatedDate)
            .ToListAsync();
    }

    public async Task<Allocation?> GetByUserIdAndByDateAsync(int userId, DateOnly startDate)
    {
        await using var context = _contextFactory.CreateDbContext();

        return await context.Allocations
            .AsNoTracking()
            .Where(a => a.AllocatedDate == startDate && a.UserId == userId)
            .Include(a => a.ParkingSpace)
                .ThenInclude(ps => ps.ParkingLot)
            .OrderBy(a => a.AllocatedDate)
            .FirstOrDefaultAsync();
    }
}
