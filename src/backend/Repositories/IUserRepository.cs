using TriviumParkingApp.Backend.Models;

namespace TriviumParkingApp.Backend.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their Firebase UID, optionally including related entities.
        /// </summary>
        /// <param name="firebaseUid">The Firebase UID.</param>
        /// <param name="includeRoles">Whether to include the user's roles.</param>
        /// <returns>The User entity or null if not found.</returns>
        Task<User?> GetUserByFirebaseUidAsync(string firebaseUid, bool includeRoles = false);

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <returns>The added User entity (with potential ID populated).</returns>
        Task<User> AddUserAsync(User user);

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">The user entity with updated values.</param>
        Task UpdateUserAsync(User user);

        // TODO: Add other necessary methods later (e.g., GetUserById, GetAllUsers)
    }
}