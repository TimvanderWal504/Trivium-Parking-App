using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Keep for DbUpdateException handling
using System.Net;
using System.Text.Json;
using System.Security.Claims; // Added for ClaimsPrincipal
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Services;
using backend.Extensions;

namespace TriviumParkingApp.Backend.Functions;

public class AuthFunctions
{
    private readonly ILogger<AuthFunctions> _logger;
    private readonly IUserService _userService; // Inject IUserService

    public AuthFunctions(ILogger<AuthFunctions> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("SyncUser")]
    public async Task<HttpResponseData> SyncUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "auth/sync")] HttpRequestData req,
        FunctionContext context) 
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to SyncUser.");

        HttpResponseData response;

        // --- Get User Info from Middleware ---
        var firebaseUid = context.GetClaimValue(ClaimTypes.NameIdentifier);
        var email = context.GetClaimValue(ClaimTypes.Email);
        var displayName = context.GetClaimValue("name") ?? context.GetClaimValue(ClaimTypes.GivenName);

        if (string.IsNullOrEmpty(firebaseUid))
        {
             _logger.LogError("SyncUser: Firebase UID (NameIdentifier claim) not found in ClaimsPrincipal.");
             response = req.CreateResponse(HttpStatusCode.InternalServerError);
             await response.WriteStringAsync("User identifier not found after authentication.");
             return response;
        }

        try
        {
             var syncRequest = new UserSyncRequestDto
             {
                 FirebaseUid = firebaseUid,
                 Email = email,
                 DisplayName = displayName
             };

           // Call service - it returns UserResponseDto? directly
           var userResponse = await _userService.SyncFirebaseUserAsync(syncRequest, true);

           if (userResponse == null)
           {
               // Service layer logged the specific error
               response = req.CreateResponse(HttpStatusCode.InternalServerError);
               await response.WriteStringAsync("An error occurred during user synchronization.");
               return response;
           }

           // SyncUser always returns OK if successful (creation/update handled internally)
           // If we needed to know if it was new, the service would need to return that info differently.
           response = req.CreateResponse(HttpStatusCode.OK);
           await response.WriteAsJsonAsync(userResponse);

       }
        catch (JsonException jsonEx)
        {
             _logger.LogError(jsonEx, "Error deserializing sync request body.");
             response = req.CreateResponse(HttpStatusCode.BadRequest);
             await response.WriteStringAsync("Invalid JSON format in request body.");
        }
        catch (DbUpdateException dbEx)
        {
             _logger.LogError(dbEx, "Database error during user sync function execution.");
             response = req.CreateResponse(HttpStatusCode.InternalServerError);
             await response.WriteStringAsync("A database error occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user sync function execution.");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An unexpected error occurred.");
        }

        return response;
    }
}