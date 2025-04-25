using backend.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using TriviumParkingApp.Backend.DTOs; // Use DTOs
using TriviumParkingApp.Backend.Services; // Use Services

namespace TriviumParkingApp.Backend.Functions
{
    public class ParkingRequestFunctions
    {
        private readonly ILogger<ParkingRequestFunctions> _logger;
        private readonly IParkingRequestService _requestService; // Inject Service
        private readonly IUserService _userService; // Inject UserService to get internal ID from Firebase UID

        public ParkingRequestFunctions(
            ILogger<ParkingRequestFunctions> logger,
            IParkingRequestService requestService,
            IUserService userService) // Updated constructor
        {
            _logger = logger;
            _requestService = requestService;
            _userService = userService;
        }

        [Function("CreateParkingRequest")]
        public async Task<HttpResponseData> CreateParkingRequest(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "requests")] HttpRequestData req, FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to CreateParkingRequest.");
            HttpResponseData response;

            string firebaseUid;
            try
            {
                firebaseUid = context.GetClaimValue(ClaimTypes.NameIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateParkingRequest: Unable to retrieve ClaimsPrincipal. {Message}", ex.Message);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Authentication context not found.");
                return response;
            }

            if (string.IsNullOrEmpty(firebaseUid))
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var user = await _userService.SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = firebaseUid });
            if (user == null)
            {
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Could not find or create user record.");
                return response;
            }

            var internalUserId = user.Id;
            var requestBody = await req.ReadAsStringAsync() ?? string.Empty;
            if (string.IsNullOrEmpty(requestBody))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Request body is empty.");
                return response;
            }

            try
            {
                var requestDto = JsonSerializer.Deserialize<CreateParkingRequestDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (requestDto == null || requestDto.RequestedDate == null)
                {
                     response = req.CreateResponse(HttpStatusCode.BadRequest);
                     await response.WriteStringAsync("Invalid request body. Required format: { \"requestedDate\": \"YYYY-MM-DD\" }");
                     return response;
                }

                var createdRequestDto = await _requestService.CreateRequestAsync(internalUserId, requestDto);

                if (createdRequestDto != null)
                {
                    response = req.CreateResponse(HttpStatusCode.Created);
                    await response.WriteAsJsonAsync(createdRequestDto);
                }
                else
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    await response.WriteStringAsync("Failed to create parking request.");
                }
            }
            catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error deserializing request body for CreateParkingRequest.");
                 response = req.CreateResponse(HttpStatusCode.BadRequest);
                 await response.WriteStringAsync("Invalid JSON format in request body.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateParkingRequest function.");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An unexpected error occurred.");
            }
            return response;
        }


        [Function("GetUserParkingRequests")]
        public async Task<HttpResponseData> GetUserParkingRequests(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "requests")] HttpRequestData req, // Removed userId from route
            FunctionContext context)
        {
             _logger.LogInformation("C# HTTP trigger function processed a request to GetUserParkingRequests.");
            HttpResponseData response;

            string? firebaseUid; // Nullable
            try
            {
                firebaseUid = context.GetClaimValue(ClaimTypes.NameIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateParkingRequest: Unable to retrieve ClaimsPrincipal. {Message}", ex.Message);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Authentication context not found.");
                return response;
            }

            if (string.IsNullOrEmpty(firebaseUid))
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var user = await _userService.SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = firebaseUid });
            if (user == null)
            {
                 response = req.CreateResponse(HttpStatusCode.InternalServerError);
                 await response.WriteStringAsync("Could not retrieve user data.");
                 return response;
            }
            int targetUserId = user.Id;

             try
             {
                 var startOfWeek = DateOnly.FromDateTime(DateTime.UtcNow);
                 var endOfWeek = startOfWeek.AddDays(7);

                 var requestsDto = await _requestService.GetUserRequestsAsync(targetUserId, startOfWeek, endOfWeek);

                 response = req.CreateResponse(HttpStatusCode.OK);
                 await response.WriteAsJsonAsync(requestsDto);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error retrieving parking requests for user {UserId}.", targetUserId);
                 response = req.CreateResponse(HttpStatusCode.InternalServerError);
                 await response.WriteStringAsync("An error occurred while retrieving parking requests.");
             }
             return response;
        }


        [Function("DeleteParkingRequest")]
        public async Task<HttpResponseData> DeleteParkingRequest(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "requests/{requestId}")] HttpRequestData req,
            string requestId, FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to DeleteParkingRequest for request ID: {RequestId}", requestId);
            HttpResponseData response;

            string firebaseUid;
            try
            {
                firebaseUid = context.GetClaimValue(ClaimTypes.NameIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateParkingRequest: Unable to retrieve ClaimsPrincipal. {Message}", ex.Message);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Authentication context not found.");
                return response;
            }

            if (string.IsNullOrEmpty(firebaseUid))
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var user = await _userService.SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = firebaseUid });
            if (user == null)
            {
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Could not find or create user record.");
                return response;
            }

            if (!int.TryParse(requestId, out int targetRequestId))
            {
                var badReqResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReqResponse.WriteStringAsync("Invalid request ID format.");
                return badReqResponse;
            }

            try
            {
                bool success = await _requestService.DeleteRequestAsync(targetRequestId, user.Id);

                if (success)
                {
                    response = req.CreateResponse(HttpStatusCode.NoContent);
                }
                else
                {
                    response = req.CreateResponse(HttpStatusCode.NotFound);
                    await response.WriteStringAsync("Could not delete parking request. It may not exist or you may not have permission.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parking request {RequestId}.", requestId);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while deleting the parking request.");
            }
            return response;
        }
    }
}