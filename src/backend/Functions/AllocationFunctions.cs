using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TriviumParkingApp.Backend.Services; // Use Services namespace

namespace TriviumParkingApp.Backend.Functions
{
    public class AllocationFunctions
    {
        private readonly ILogger<AllocationFunctions> _logger;
        private readonly IAllocationService _allocationService; // Inject Service
        private readonly IUserService _userService; // Inject UserService for auth checks

        public AllocationFunctions(
            ILogger<AllocationFunctions> logger,
            IAllocationService allocationService,
            IUserService userService) // Updated constructor
        {
            _logger = logger;
            _allocationService = allocationService;
            _userService = userService;
        }

        // TODO: Implement proper authentication and authorization

        [Function("GetUserAllocations")]
        public async Task<HttpResponseData> GetUserAllocations(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "allocations/user/{userId}")] HttpRequestData req, // Changed AuthLevel
            string userId) // Using route parameter for now, replace with authenticated user ID
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to GetUserAllocations for user: {UserId}", userId);

            // --- Authentication/Authorization Placeholder ---
            // string firebaseUserId = GetFirebaseUserIdFromPrincipal(req.FunctionContext.BindingContext.BindingData["User"] as ClaimsPrincipal);
            // if (string.IsNullOrEmpty(firebaseUserId)) return req.CreateResponse(HttpStatusCode.Unauthorized);
            // var (user, _) = await _userService.SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = firebaseUserId });
            // if (user == null) return req.CreateResponse(HttpStatusCode.InternalServerError, "Could not find user record.");
            // int internalUserId = user.Id;
            // // Ensure requested userId matches authenticated user or user is admin
            // if (internalUserId.ToString() != userId && !user.Roles.Contains("Admin")) return req.CreateResponse(HttpStatusCode.Forbidden);
            // For now, parse the route parameter:
            if (!int.TryParse(userId, out int targetUserId))
            {
                 var badReqResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                 await badReqResponse.WriteStringAsync("Invalid user ID format.");
                 return badReqResponse;
            }
            // --- End Placeholder ---

            HttpResponseData response;
            try
            {
                // TODO: Define date range properly based on business rules (e.g., next Mon-Fri)
                DateOnly startOfWeek = DateOnly.FromDateTime(DateTime.UtcNow); // Placeholder
                DateOnly endOfWeek = startOfWeek.AddDays(7); // Placeholder

                // Delegate to service
                var allocationsDto = await _allocationService.GetUserAllocationsAsync(targetUserId, startOfWeek, endOfWeek);

                response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(allocationsDto); // Return DTO list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocations for user {UserId}.", userId);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while retrieving allocations.");
            }
            return response;
        }

        // Placeholder method to get user ID from ClaimsPrincipal (implement properly later)
        // private string GetFirebaseUserIdFromPrincipal(ClaimsPrincipal principal)
        // {
        //     // Example: return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     return principal?.Identity?.Name ?? string.Empty; // Adjust based on actual claims
        // }
    }
}