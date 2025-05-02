using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<ParkingDbContext> _contextFactory;

        public UserRepository(IDbContextFactory<ParkingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid, bool includeRoles = false)
        {
            await using var ctx = _contextFactory.CreateDbContext();
            IQueryable<User> query = ctx.Users;

            if (includeRoles)
            {
                query = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
            }

            return await query.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        public async Task<User> AddUserAsync(User user)
        {
            await using var ctx = _contextFactory.CreateDbContext();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            await using var ctx = _contextFactory.CreateDbContext();
            ctx.Entry(user).State = EntityState.Modified;
            await ctx.SaveChangesAsync();
        }
    }
}