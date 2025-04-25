using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For DbUpdateException and Include/ThenInclude
using TriviumParkingApp.Backend.Data; // For DbContext/SaveChanges
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;

namespace TriviumParkingApp.Backend.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IParkingRequestRepository _requestRepository; // Need requests for allocation
        private readonly IParkingLotRepository _parkingLotRepository; // Need lots/spaces for allocation
        private readonly ParkingDbContext _context; // For SaveChanges Unit of Work
        private readonly ILogger<AllocationService> _logger;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IParkingRequestRepository requestRepository,
            IParkingLotRepository parkingLotRepository,
            ParkingDbContext context,
            ILogger<AllocationService> logger)
        {
            _allocationRepository = allocationRepository;
            _requestRepository = requestRepository;
            _parkingLotRepository = parkingLotRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AllocationResponseDto>> GetUserAllocationsAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var allocations = await _allocationRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
                return allocations.Select(MapAllocationToDto); // Use mapping function
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocations for user {UserId} between {StartDate} and {EndDate}.", userId, startDate, endDate);
                return Enumerable.Empty<AllocationResponseDto>();
            }
        }

        public async Task<IEnumerable<Allocation>> RunWeeklyAllocationAsync(DateOnly startOfWeek, DateOnly endOfWeek)
        {
            _logger.LogInformation("Starting weekly allocation process for {StartOfWeek} to {EndOfWeek}", startOfWeek, endOfWeek);
            var newAllocations = new List<Allocation>();

            try
            {
                // Fetch all necessary data upfront
                // TODO: Consider fetching requests with user+roles included if not already done by repo
                // For now, assume repo fetches necessary includes or add them here/in repo.
                // Let's refine the request fetching - need user roles!
                 var pendingRequests = await _context.ParkingRequests // Using context directly for complex include
                    .Where(r => r.RequestedDate >= startOfWeek && r.RequestedDate <= endOfWeek)
                    .Include(r => r.User)
                        .ThenInclude(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                    .OrderBy(r => r.RequestTimestamp) // FCFS order
                    .ToListAsync();

                if (!pendingRequests.Any())
                {
                    _logger.LogInformation("No pending parking requests found for the target week.");
                    return newAllocations; // Return empty list
                }

                // Fetch available spaces, ordered by lot priority
                var availableSpaces = (await _parkingLotRepository.GetAllAsync(includeSpaces: true))
                                      .SelectMany(lot => lot.ParkingSpaces) // Flatten spaces
                                      .OrderBy(ps => ps.ParkingLot.Priority) // Ensure ordering
                                      .ThenBy(ps => ps.Id)
                                      .ToList();


                if (!availableSpaces.Any())
                {
                    _logger.LogWarning("No parking spaces found in the system. Cannot perform allocation.");
                     return newAllocations; // Return empty list
                }

                _logger.LogInformation("Processing {RequestCount} requests and {SpaceCount} spaces...", pendingRequests.Count, availableSpaces.Count);
                var allocatedSpaceIdsForDay = new HashSet<int>();

                // Iterate through each day
                for (DateOnly currentDate = startOfWeek; currentDate <= endOfWeek; currentDate = currentDate.AddDays(1))
                {
                    _logger.LogDebug("Processing allocations for date: {CurrentDate}", currentDate);
                    allocatedSpaceIdsForDay.Clear();

                    var requestsForDay = pendingRequests.Where(r => r.RequestedDate == currentDate).ToList();
                    if (!requestsForDay.Any()) continue; // Skip day if no requests

                    var rolePriority = new List<string> { "Visitor", "Management", "Employee" };

                    foreach (var roleName in rolePriority)
                    {
                        var requestsForRole = requestsForDay
                            .Where(r => r.User.UserRoles.Any(ur => ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                            .ToList(); // Materialize for removal

                        foreach (var request in requestsForRole)
                        {
                            // Determine if priority space is preferred/required for this role
                            bool isPrioritySpaceNeeded = roleName != "Employee"; // Example logic

                            ParkingSpace? assignedSpace = FindAvailableSpace(availableSpaces, allocatedSpaceIdsForDay, isPrioritySpaceNeeded);

                            if (assignedSpace != null)
                            {
                                var allocation = new Allocation
                                {
                                    UserId = request.UserId,
                                    ParkingSpaceId = assignedSpace.Id,
                                    AllocatedDate = currentDate,
                                    AllocationTimestamp = DateTimeOffset.UtcNow
                                };
                                newAllocations.Add(allocation);
                                allocatedSpaceIdsForDay.Add(assignedSpace.Id);
                                _logger.LogInformation("ALLOCATED: User {UserId} ({Role}) -> Space {SpaceNumber} (Lot: {LotName}) for {Date}", request.UserId, roleName, assignedSpace.SpaceNumber, assignedSpace.ParkingLot.Name, currentDate);

                                // Remove request from list to avoid re-processing (optional optimization)
                                // Note: Modifying list while iterating can be tricky, ensure correctness
                                // It might be safer to just rely on allocatedSpaceIdsForDay check
                            }
                            else
                            {
                                _logger.LogWarning("Could not find available space for User {UserId} ({Role}) for {Date}", request.UserId, roleName, currentDate);
                            }
                        } // End foreach request in role
                    } // End foreach role priority
                } // End foreach day

                // Save all new allocations in one transaction
                if (newAllocations.Any())
                {
                    await _allocationRepository.AddRangeAsync(newAllocations);
                    await _context.SaveChangesAsync(); // Commit Unit of Work
                    _logger.LogInformation("Successfully saved {AllocationCount} new allocations.", newAllocations.Count);
                }
                else
                {
                    _logger.LogInformation("No new allocations were made in this run.");
                }

                 // TODO: Trigger notifications based on newAllocations list
                 if (newAllocations.Any())
                 {
                     _logger.LogInformation("Placeholder: Trigger notifications for {Count} allocated users.", newAllocations.Count);
                     // await _notificationService.SendAllocationNotificationsAsync(newAllocations);
                 }

            }
            catch (DbUpdateException dbEx)
            {
                 _logger.LogError(dbEx, "Database error during RunWeeklyAllocationAsync.");
                 // Rethrow or handle as appropriate
                 throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during RunWeeklyAllocationAsync.");
                 // Rethrow or handle as appropriate
                 throw;
            }

            _logger.LogInformation("Weekly allocation process finished.");
            return newAllocations; // Return the created entities
        }


        // --- Private Helpers ---

        // Helper function to find an available space (Refined logic might be needed)
        private ParkingSpace? FindAvailableSpace(List<ParkingSpace> allSpaces, HashSet<int> usedSpaceIdsThisDay, bool isPrioritySpaceNeeded)
        {
            // Try finding a priority space first if needed
            if (isPrioritySpaceNeeded)
            {
                var prioritySpace = allSpaces.FirstOrDefault(space =>
                    space.IsPrioritySpace && !usedSpaceIdsThisDay.Contains(space.Id));
                if (prioritySpace != null) return prioritySpace;
            }

            // If priority not needed, or no priority space found, find any available space
            var availableSpace = allSpaces.FirstOrDefault(space =>
                !usedSpaceIdsThisDay.Contains(space.Id));

            return availableSpace;
        }

        private AllocationResponseDto MapAllocationToDto(Allocation allocation)
        {
             // Ensure related data is loaded if not already included by the repository call
            if (allocation.ParkingSpace == null || allocation.ParkingSpace.ParkingLot == null)
            {
                _logger.LogWarning("Mapping Allocation ID {AllocationId} to DTO with missing related ParkingSpace/ParkingLot data.", allocation.Id);
                // Handle appropriately - throw, return partial DTO, or ensure data is always loaded
            }

            return new AllocationResponseDto
            {
                Id = allocation.Id,
                UserId = allocation.UserId,
                AllocatedDate = allocation.AllocatedDate,
                AllocationTimestamp = allocation.AllocationTimestamp,
                ParkingSpaceId = allocation.ParkingSpaceId,
                ParkingSpaceNumber = allocation.ParkingSpace?.SpaceNumber ?? "N/A",
                ParkingLotId = allocation.ParkingSpace?.ParkingLotId ?? 0,
                ParkingLotName = allocation.ParkingSpace?.ParkingLot?.Name ?? "N/A",
                ParkingLotAddress = allocation.ParkingSpace?.ParkingLot?.Address
            };
        }
    }
}