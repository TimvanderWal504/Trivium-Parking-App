using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ParkingDbContext _context;

        public UserRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid, bool includeRoles = false)
        {
            IQueryable<User> query = _context.Users;

            if (includeRoles)
            {
                query = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
            }

            return await query.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}