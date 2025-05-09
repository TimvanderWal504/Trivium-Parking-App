using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TriviumParkingApp.Backend.Repositories;

public class ParkingRequestRepository : IParkingRequestRepository
{
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;

    public ParkingRequestRepository(IDbContextFactory<ParkingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ParkingRequest> AddAsync(ParkingRequest request)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.ParkingRequests.Add(request);
        await ctx.SaveChangesAsync();
        return request;
    }

    public async Task<ParkingRequest?> GetByIdAsync(int requestId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.ParkingRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<IEnumerable<ParkingRequest>> GetByUserIdAndDateRangeAsync(
        int userId,
        DateOnly startDate,
        DateOnly endDate)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.ParkingRequests
            .Where(pr => pr.UserId == userId && pr.RequestedDate >= startDate && pr.RequestedDate <= endDate)
            .OrderBy(pr => pr.RequestedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingRequest>> GetByDateAsync(DateOnly startDate)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.ParkingRequests
            .Include(pr => pr.User)
            .Where(pr => pr.RequestedDate == startDate)
            .OrderBy(pr => pr.RequestTimestamp)
            .ToListAsync();
    }

    public async Task DeleteAsync(ParkingRequest request)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.ParkingRequests.Remove(request);
        await ctx.SaveChangesAsync();
    }
}