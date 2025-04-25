using TriviumParkingApp.Backend.DTOs;

namespace TriviumParkingApp.Backend.Services
{
    public interface IParkingRequestService
    {
        /// <summary>
        /// Creates a new parking request for the specified user.
        /// </summary>
        /// <param name="userId">The internal database ID of the user making the request.</param>
        /// <param name="requestDto">DTO containing the request details.</param>
        /// <returns>The created ParkingRequestResponseDto or null if creation failed.</returns>
        Task<ParkingRequestResponseDto?> CreateRequestAsync(int userId, CreateParkingRequestDto requestDto);

        /// <summary>
        /// Gets parking requests for a user within a specific date range (e.g., upcoming week).
        /// </summary>
        /// <param name="userId">The internal database ID of the user.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>A collection of ParkingRequestResponseDto.</returns>
        Task<IEnumerable<ParkingRequestResponseDto>> GetUserRequestsAsync(int userId, DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Deletes a parking request if the user is authorized.
        /// </summary>
        /// <param name="requestId">The ID of the request to delete.</param>
        /// <param name="requestingUserId">The internal database ID of the user attempting deletion.</param>
        /// <returns>True if deletion was successful, false otherwise (not found, not authorized, error).</returns>
        Task<bool> DeleteRequestAsync(int requestId, int requestingUserId);
    }
}