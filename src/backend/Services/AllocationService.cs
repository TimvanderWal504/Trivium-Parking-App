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
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<AllocationService> _logger;

    public AllocationService(
        IAllocationRepository allocationRepository,
        IParkingRequestRepository requestRepository,
        IParkingLotRepository parkingLotRepository,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<AllocationService> logger)
    {
        _allocationRepository = allocationRepository;
        _requestRepository = requestRepository;
        _parkingLotRepository = parkingLotRepository;
        _userManager = userManager;
        _roleManager = roleManager;
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
        var requests = await _requestRepository.GetByDateAsync(date);
        var existing = await _allocationRepository.GetByDateAsync(date);

        var unallocated = requests
            .Where(r => !existing.Select(a => a.UserId).Contains(r.UserId))
            .ToList();

        if (unallocated.Count == 0)
            return [];

        return await AllocateRequestsAsync(date, unallocated, existing);
    }

    public async Task<AllocationResponseDto?> CreateUserAllocationAsync(ParkingRequest request)
    {
        var date = request.RequestedDate;
        var existing = await _allocationRepository.GetByDateAsync(date);

        if (existing.Any(a => a.UserId == request.UserId))
            return null;

        var newAllocs = await AllocateRequestsAsync(date, [request], existing);
        var alloc = newAllocs.FirstOrDefault();
        if (alloc == null)
            return null;

        return new AllocationResponseDto(alloc);
    }

    private async Task<List<Allocation>> AllocateRequestsAsync(
        DateOnly date,
        IEnumerable<ParkingRequest> requests,
        IEnumerable<Allocation> existing)
    {
        // 1) Determine which requests to allocate (already filtered by caller)
        var unallocated = requests.ToList();

        // 2) Bulk-fetch roles and compute priority per user
        var rolesByUser = new Dictionary<int, IList<string>>();
        var userPriority = new Dictionary<int, int>();
        foreach (var uid in unallocated.Select(r => r.UserId).Distinct())
        {
            var appUser = await _userManager.FindByIdAsync($"{uid}");
            var roleNames = await _userManager.GetRolesAsync(appUser);
            rolesByUser[uid] = roleNames;

            var roleEntities = await Task.WhenAll(
                roleNames.Select(rn => _roleManager.FindByNameAsync(rn))
            );
            userPriority[uid] = roleEntities.Min(re => re.Priority);
        }

        // 3) Load parking lots and prepare lookup
        var parkingLots = await _parkingLotRepository.GetAllAsync(includeSpaces: true);
        var lotAllowedRoles = parkingLots.ToDictionary(
            lot => lot.Id,
            lot => new HashSet<string>(lot.RoleParkingLots.Select(rpl => rpl.Role.Name))
        );
        var lotsByRegion = parkingLots
            .GroupBy(l => new { l.CountryIsoCode, l.City })
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(lot => lot.Priority).ToList()
            );

        // 4) Group by region and sort by priority + FIFO
        var byRegion = unallocated
            .GroupBy(r => new { r.CountryIsoCode, r.City })
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(r => userPriority[r.UserId])
                    .ThenBy(r => r.RequestTimestamp)
                    .ToList()
            );

        var newAllocs = new List<Allocation>();
        var occupiedIds = existing.Select(a => a.ParkingSpaceId).ToHashSet();

        // 5) Allocate per region and lot
        foreach (var region in byRegion.Keys)
        {
            var regionReqs = byRegion[region];
            var regionUserIds = regionReqs.Select(r => r.UserId).ToHashSet();

            if (!lotsByRegion.TryGetValue(region, out var regionLots))
                continue;

            foreach (var lot in regionLots)
            {
                if (regionUserIds.Count == 0)
                    break;

                var freeSpaces = lot.ParkingSpaces
                    .Where(s => !occupiedIds.Contains(s.Id))
                    .ToList();
                if (!freeSpaces.Any())
                    continue;

                var eligible = regionReqs
                    .Where(req =>
                        regionUserIds.Contains(req.UserId) &&
                        rolesByUser[req.UserId].Any(role => lotAllowedRoles[lot.Id].Contains(role)))
                    .ToList();
                if (!eligible.Any())
                    continue;

                var assignCount = Math.Min(eligible.Count, freeSpaces.Count);
                var toAssignReqs = eligible.Take(assignCount).ToList();
                var toAssignSpaces = freeSpaces.Take(assignCount).ToList();

                var allocs = toAssignReqs.Select((r, i) => new Allocation
                {
                    UserId = r.UserId,
                    ParkingSpaceId = toAssignSpaces[i].Id,
                    AllocatedDate = date,
                    AllocationTimestamp = DateTimeOffset.UtcNow
                }).ToList();

                newAllocs.AddRange(allocs);
                foreach (var a in allocs)
                    occupiedIds.Add(a.ParkingSpaceId);

                var assignedIds = toAssignReqs.Select(r => r.UserId).ToHashSet();
                regionUserIds.ExceptWith(assignedIds);
                regionReqs = regionReqs.Where(r => regionUserIds.Contains(r.UserId)).ToList();
            }
        }

        // 6) Persist and return
        await _allocationRepository.AddRangeAsync(newAllocs);
        return newAllocs;
    }
}