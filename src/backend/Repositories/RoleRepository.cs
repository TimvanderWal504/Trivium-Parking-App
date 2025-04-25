using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ParkingDbContext _context;

        public RoleRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }

            // Use case-insensitive comparison for flexibility
            return await _context.Roles
                                 .FirstOrDefaultAsync(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}