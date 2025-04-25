using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For DbUpdateException
using TriviumParkingApp.Backend.Data; // For DbContext/SaveChanges
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;

namespace TriviumParkingApp.Backend.Services
{
    public class ParkingRequestService : IParkingRequestService
    {
        private readonly IParkingRequestRepository _requestRepository;
        private readonly ParkingDbContext _context; // Inject DbContext for Unit of Work
        private readonly ILogger<ParkingRequestService> _logger;

        public ParkingRequestService(
            IParkingRequestRepository requestRepository,
            ParkingDbContext context,
            ILogger<ParkingRequestService> logger)
        {
            _requestRepository = requestRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ParkingRequestResponseDto?> CreateRequestAsync(int userId, CreateParkingRequestDto requestDto)
        {
            // TODO: Add validation logic here or use FluentValidation
            // - Check if RequestedDate is valid (e.g., within next week, not in the past)
            // - Check if user already has a request for that date? (Depends on business rules)

            if (requestDto.RequestedDate == null)
            {
                 _logger.LogWarning("CreateRequestAsync called with null RequestedDate for user {UserId}.", userId);
                 return null; // Or throw validation exception
            }

            var newRequest = new ParkingRequest
            {
                UserId = userId,
                RequestedDate = requestDto.RequestedDate.Value,
                RequestTimestamp = DateTimeOffset.UtcNow
                // Set PreferredParkingLotId if implemented
            };

            try
            {
                await _requestRepository.AddAsync(newRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created parking request ID {RequestId} for user {UserId} for date {RequestedDate}.", newRequest.Id, userId, newRequest.RequestedDate);

                return MapRequestToDto(newRequest);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating parking request for user {UserId}.", userId);
                return null;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error creating parking request for user {UserId}.", userId);
                 return null;
            }
        }

        public async Task<IEnumerable<ParkingRequestResponseDto>> GetUserRequestsAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var requests = await _requestRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
                return requests.Select(MapRequestToDto);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error retrieving requests for user {UserId} between {StartDate} and {EndDate}.", userId, startDate, endDate);
                 return Enumerable.Empty<ParkingRequestResponseDto>();
            }
        }

        public async Task<bool> DeleteRequestAsync(int requestId, int requestingUserId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);

                if (request == null)
                {
                    _logger.LogWarning("DeleteRequestAsync: Request ID {RequestId} not found.", requestId);
                    return false;
                }

                // Authorization check: Ensure the user deleting the request is the one who made it
                // TODO: Allow admins to delete any request? Requires role check.
                if (request.UserId != requestingUserId)
                {
                    _logger.LogWarning("User {RequestingUserId} attempted to delete request ID {RequestId} owned by user {OwnerUserId}.", requestingUserId, requestId, request.UserId);
                    return false;
                }

                // TODO: Add business logic check: Prevent deletion if allocation already occurred for this date?

                _requestRepository.Delete(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted parking request ID {RequestId} by user {RequestingUserId}.", requestId, requestingUserId);
                return true;
            }
             catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error deleting parking request ID {RequestId}.", requestId);
                return false;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error deleting parking request ID {RequestId}.", requestId);
                 return false;
            }
        }

        private ParkingRequestResponseDto MapRequestToDto(ParkingRequest request)
        {
            return new ParkingRequestResponseDto
            {
                Id = request.Id,
                UserId = request.UserId,
                RequestedDate = request.RequestedDate,
                RequestTimestamp = request.RequestTimestamp
            };
        }
    }
}