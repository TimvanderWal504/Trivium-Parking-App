using Microsoft.Extensions.Logging;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;

namespace TriviumParkingApp.Backend.Services;

public class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _allocationRepository;
    private readonly IParkingRequestRepository _requestRepository;
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly ILogger<AllocationService> _logger;

    public AllocationService(
        IAllocationRepository allocationRepository,
        IParkingRequestRepository requestRepository,
        IParkingLotRepository parkingLotRepository,
        ILogger<AllocationService> logger)
    {
        _allocationRepository = allocationRepository;
        _requestRepository = requestRepository;
        _parkingLotRepository = parkingLotRepository;
        _logger = logger;
    }

    public async Task<AllocationResponseDto?> GetUserAllocationsAsync(int userId, DateOnly startDate)
    {
        try
        {
            var allocation = await _allocationRepository.GetByUserIdAndByDateAsync(userId, startDate);
            return allocation == null ? null : new AllocationResponseDto(allocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations for user {UserId} for {StartDate} .", userId, startDate);
            throw;
        }
    }

    public async Task<IEnumerable<Allocation>> RunDailyAllocationAsync(DateOnly date)
    {
        // 1. Get all requests for that dag
        var requests = await _requestRepository.GetByDateAsync(date);

        // 2. Get existing allocations for that dag
        var existingAllocs = await _allocationRepository.GetByDateAsync(date);

        // 3. Filter requests: only the userId's not allocated yet, must be allocated.
        var unallocatedRequests = requests
            .Where(r => !existingAllocs.Select(x => x.UserId).ToList().Contains(r.UserId))
            .ToList();

        // 3. When all users has been allocated already, return emtpy
        if (unallocatedRequests.Count == 0)
            return [];

        var occupiedSpaceIds = new HashSet<int>(existingAllocs.Select(a => a.ParkingSpaceId));

        // 4. Get all Parkinglots + spaces
        var parkingLots = await _parkingLotRepository.GetAllAsync(true);

        // 5. Sort de requests on rol-priority en FIFO
        var sortedRequests = unallocatedRequests
            .OrderBy(r => r.User.UserRoles.Select(ur => ur.Role.Priority))
            .ThenBy(r => r.RequestTimestamp)
            .ToList();

        var remaining = new List<ParkingRequest>(sortedRequests);
        var newAllocs = new List<Allocation>();

        // 6. Loop through lots in order of priority
        foreach (var lot in parkingLots.OrderBy(l => l.Priority))
        {
            if (remaining.Count == 0)
                break;

            // 6a. Determine free spaces
            var freeSpaces = lot.ParkingSpaces
                .Where(s => !occupiedSpaceIds.Contains(s.Id))
                .ToList();

            // 6b. Determine how many people can be put here.
            var count = Math.Min(remaining.Count, freeSpaces.Count);
            var forThisLotReqs = remaining.Take(count).ToList();
            var forThisLotSpaces = freeSpaces.Take(count).ToList();

            // 6c. Create allocations
            var todayAllocs = forThisLotReqs
            .Select((req, idx) => new Allocation
                {
                    UserId = req.UserId,
                    ParkingSpaceId = forThisLotSpaces[idx].Id,
                    AllocatedDate = date,
                    AllocationTimestamp = DateTimeOffset.UtcNow
                })
            .ToList();

            newAllocs.AddRange(todayAllocs);

            // 6d. remove requests from remaining requestsn
            remaining = remaining.Skip(count).ToList();

            // 6e. Mark these spaces as occupied
            foreach (var a in todayAllocs)
                occupiedSpaceIds.Add(a.ParkingSpaceId);
        }

        // 7. Inject into the database and send notifications
        await _allocationRepository.AddRangeAsync(newAllocs);

        return newAllocs;
    }
}