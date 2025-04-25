using TriviumParkingApp.Backend.DTOs; // Use DTOs

namespace TriviumParkingApp.Backend.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Ensures a user exists in the database corresponding to the Firebase user.
        /// Creates the user with a default role if they don't exist.
        /// Updates user details if necessary.
        /// </summary>
        /// <param name="syncRequest">DTO containing Firebase user details.</param>
        /// <returns>A DTO representing the synced user details, or null if an error occurred.</returns>
        Task<UserResponseDto?> SyncFirebaseUserAsync(UserSyncRequestDto syncRequest, bool createNewUser = false);

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        /// <param name="userId">The internal database ID of the user.</param>
        /// <param name="roleName">The name of the role to assign.</param>
        /// <returns>True if the role was assigned or already existed, false otherwise (e.g., user or role not found).</returns>
        Task<bool> AssignRoleAsync(int userId, string roleName);

        /// <summary>
        /// Gets all users with their roles.
        /// </summary>
        /// <returns>A list of UserResponseDto.</returns>
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

        /// <summary>
        /// Creates a new user in Firebase Auth and the local database.
        /// </summary>
        /// <param name="createUserDto">DTO containing new user details (email, password, name, role).</param>
        /// <returns>The created UserResponseDto or null if creation failed.</returns>
        Task<UserResponseDto?> CreateUserAsync(CreateUserRequestDto createUserDto);

        // TODO: Add other user management methods as needed
    }
}