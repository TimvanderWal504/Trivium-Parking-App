using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AllocationService> _logger;

    public AllocationService(
        IAllocationRepository allocationRepository,
        IParkingRequestRepository requestRepository,
        IParkingLotRepository parkingLotRepository, 
        UserManager<User> userManager,
        ILogger<AllocationService> logger)
    {
        _allocationRepository = allocationRepository;
        _requestRepository = requestRepository;
        _parkingLotRepository = parkingLotRepository;
        _userManager = userManager;
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
        // 1. Retrieve all parking requests (including user) for the specified date
        var requests = await _requestRepository.GetByDateAsync(date);

        // 2. Retrieve existing allocations for that date
        var existingAllocs = await _allocationRepository.GetByDateAsync(date);

        // 3. Filter out requests for users who already have an allocation
        var unallocated = requests
            .Where(r => !existingAllocs.Select(a => a.UserId).Contains(r.UserId))
            .ToList();

        if (unallocated.Count == 0)
            return Array.Empty<Allocation>();

        // 4. Determine each user's roles via UserManager
        var rolesByUser = new Dictionary<int, IList<string>>();
        var userIds = unallocated.Select(r => r.UserId).Distinct();
        foreach (var uid in userIds)
        {
            // The User.UserName matches FirebaseUid
            var appUser = unallocated.First(r => r.UserId == uid).User;
            var roles = await _userManager.GetRolesAsync(appUser);
            rolesByUser[uid] = roles;
        }

        // 5. Track occupied parking space IDs
        var occupiedSpaceIds = existingAllocs
            .Select(a => a.ParkingSpaceId)
            .ToHashSet();

        // 6. Retrieve all parking lots (with spaces and role mappings)
        var parkingLots = await _parkingLotRepository.GetAllAsync(includeSpaces: true);
        // Ensure RoleParkingLots is also included in the repository implementation

        // 7. Order unallocated requests by timestamp (FIFO)
        var remaining = unallocated
            .OrderBy(r => r.RequestTimestamp)
            .ToList();

        var newAllocs = new List<Allocation>();

        // 8. Iterate through lots by ascending lot.Priority (lower = higher priority)
        foreach (var lot in parkingLots.OrderBy(l => l.Priority))
        {
            if (remaining.Count == 0)
                break;

            // 8a. Identify free spaces in the current lot
            var freeSpaces = lot.ParkingSpaces
                .Where(s => !occupiedSpaceIds.Contains(s.Id))
                .ToList();
            if (freeSpaces.Count == 0)
                continue;

            // 8b. Filter requests eligible for this lot based on role mappings
            var eligible = remaining
                .Where(req => rolesByUser[req.UserId]
                    .Any(roleName =>
                        lot.RoleParkingLots
                           .Select(rpl => rpl.Role.Name)
                           .Contains(roleName)))
                .ToList();

            if (eligible.Count == 0)
                continue;

            // 8c. Determine how many allocations can be made here
            var assignCount = Math.Min(eligible.Count, freeSpaces.Count);
            var toAssignRequests = eligible.Take(assignCount).ToList();
            var toAssignSpaces = freeSpaces.Take(assignCount).ToList();

            // 8d. Create allocation objects for this lot
            var todayAllocs = toAssignRequests
                .Select((req, idx) => new Allocation
                {
                    UserId = req.UserId,
                    ParkingSpaceId = toAssignSpaces[idx].Id,
                    AllocatedDate = date,
                    AllocationTimestamp = DateTimeOffset.UtcNow
                })
                .ToList();

            newAllocs.AddRange(todayAllocs);

            // 8e. Update remaining requests and occupied spaces
            var assignedUserIds = toAssignRequests.Select(r => r.UserId).ToHashSet();
            remaining = remaining
                .Where(r => !assignedUserIds.Contains(r.UserId))
                .ToList();

            foreach (var alloc in todayAllocs)
                occupiedSpaceIds.Add(alloc.ParkingSpaceId);
        }

        // 9. Persist new allocations and return the list
        await _allocationRepository.AddRangeAsync(newAllocs);
        return newAllocs;
    }
}