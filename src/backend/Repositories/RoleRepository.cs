using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;

    public RoleRepository(IDbContextFactory<ParkingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower().Equals(roleName.ToLower()));
    }
}