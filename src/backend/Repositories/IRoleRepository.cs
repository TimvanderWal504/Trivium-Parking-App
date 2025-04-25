using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public interface IRoleRepository
    {
        /// <summary>
        /// Gets a role by its name.
        /// </summary>
        /// <param name="roleName">The name of the role (case-insensitive).</param>
        /// <returns>The Role entity or null if not found.</returns>
        Task<Role?> GetRoleByNameAsync(string roleName);

        // TODO: Add other methods if needed (e.g., GetAllRoles)
    }
}