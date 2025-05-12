using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;

namespace TriviumParkingApp.Backend.Services;

public class ParkingRequestService : IParkingRequestService
{
    private readonly IParkingRequestRepository _requestRepository;
    private readonly ILogger<ParkingRequestService> _logger;

    public ParkingRequestService(
        IParkingRequestRepository requestRepository,
        ILogger<ParkingRequestService> logger)
    {
        _requestRepository = requestRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ParkingRequest>> GetUserRequestsAsync(int userId, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            return await _requestRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving requests for user {UserId} between {StartDate} and {EndDate}.", userId, startDate, endDate);
            throw;
        }
    }

    public async Task<ParkingRequest?> CreateRequestAsync(int userId, CreateParkingRequestDto requestDto)
    {
        if (requestDto.RequestedDate == null)
        {
             _logger.LogWarning("CreateRequestAsync called with null RequestedDate for user {UserId}.", userId);
             return null;
        }

        var newRequest = new ParkingRequest
        {
            UserId = userId,
            RequestedDate = requestDto.RequestedDate.Value,
            RequestTimestamp = DateTimeOffset.UtcNow,
            City = requestDto.City,
            CountryIsoCode = requestDto.CountryIsoCode,
        };

        try
        {
            newRequest = await _requestRepository.AddAsync(newRequest);

            _logger.LogInformation("Created parking request ID {RequestId} for user {UserId} for date {RequestedDate}.", newRequest.Id, userId, newRequest.RequestedDate);

            return newRequest;
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

            await _requestRepository.DeleteAsync(request);

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
}