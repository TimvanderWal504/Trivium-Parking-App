using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Keep for DbUpdateException
using System.Net;
using System.Text.Json;
using TriviumParkingApp.Backend.DTOs; // Use DTOs
using TriviumParkingApp.Backend.Services; // Use Services

namespace TriviumParkingApp.Backend.Functions
{
    public class UserManagementFunctions
    {
        private readonly ILogger<UserManagementFunctions> _logger;
        private readonly IUserService _userService; // Inject IUserService

        public UserManagementFunctions(ILogger<UserManagementFunctions> logger, IUserService userService) // Updated constructor
        {
            _logger = logger;
            _userService = userService; // Assign injected service
        }

        // --- TODO: IMPORTANT ---
        // Implement proper authentication and authorization middleware/filters.
        // All functions in this class should require ADMIN privileges.

        [Function("GetUsers")]
        public async Task<HttpResponseData> GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequestData req) // Changed AuthLevel
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to GetUsers.");

            // --- Authorization Placeholder ---
            // if (!IsAdminRequest(req)) return req.CreateResponse(HttpStatusCode.Forbidden);
            // --- End Authorization Placeholder ---

            HttpResponseData response;
            try
            {
                // Delegate to service layer
                var users = await _userService.GetAllUsersAsync();

                response = req.CreateResponse(HttpStatusCode.OK);
                // Use WriteAsJsonAsync with UserResponseDto list
                await response.WriteAsJsonAsync(users);
            }
            catch (Exception ex) // Catch broader exceptions as service layer might throw others
            {
                _logger.LogError(ex, "Error retrieving users.");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while retrieving users.");
            }
            return response;
        }

        [Function("AssignUserRole")]
        public async Task<HttpResponseData> AssignUserRole(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/{userId}/role")] HttpRequestData req, // Changed AuthLevel
            string userId)
        {
             _logger.LogInformation("C# HTTP trigger function processed a request to AssignUserRole for user ID: {UserId}", userId);

            // --- Authorization Placeholder ---
             // if (!IsAdminRequest(req)) return req.CreateResponse(HttpStatusCode.Forbidden);
            // --- End Authorization Placeholder ---

            if (!int.TryParse(userId, out int targetUserId))
            {
                 var badReqResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                 await badReqResponse.WriteStringAsync("Invalid user ID format.");
                 return badReqResponse;
            }

            HttpResponseData response;
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;
            if (string.IsNullOrEmpty(requestBody))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Request body is empty. Required format: { \"roleName\": \"RoleName\" }");
                return response;
            }

            try
            {
                // Use DTO from DTOs namespace
                var requestData = JsonSerializer.Deserialize<AssignRoleRequestDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Basic validation on DTO (can be enhanced with FluentValidation)
                if (requestData == null || string.IsNullOrWhiteSpace(requestData.RoleName))
                {
                     response = req.CreateResponse(HttpStatusCode.BadRequest);
                     await response.WriteStringAsync("Invalid request body. Required format: { \"roleName\": \"RoleName\" }");
                     return response;
                }

                // Delegate logic to the UserService
                bool success = await _userService.AssignRoleAsync(targetUserId, requestData.RoleName);

                if (success)
                {
                    response = req.CreateResponse(HttpStatusCode.OK);
                    // Optionally return the updated user/role info by calling GetUserById if needed
                    await response.WriteStringAsync($"Role '{requestData.RoleName}' processed for user ID {targetUserId}.");
                }
                else
                {
                    // Assume service layer logged the specific reason (user/role not found, db error)
                    // Return a generic bad request or not found depending on expected failure modes
                    // For simplicity, returning BadRequest, but could check service response more granularly
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    await response.WriteStringAsync($"Could not assign role '{requestData.RoleName}' to user ID {targetUserId}. User or role might not exist.");
                }
            }
            catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error deserializing request body for AssignUserRole.");
                 response = req.CreateResponse(HttpStatusCode.BadRequest);
                 await response.WriteStringAsync("Invalid JSON format in request body.");
            }
             // Catching specific exceptions might be handled better in middleware or service layer
            catch (DbUpdateException dbEx)
            {
                 _logger.LogError(dbEx, "Database error during AssignUserRole function execution for user ID {UserId}.", userId);
                 response = req.CreateResponse(HttpStatusCode.InternalServerError);
                 await response.WriteStringAsync("A database error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}.", userId);
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while assigning the role.");
            }
            return response;
        }

        // Internal DTO and placeholder auth methods removed
        // } // This closing brace is removed

        [Function("CreateUser")]
        public async Task<HttpResponseData> CreateUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequestData req) // Requires Admin role check
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to CreateUser.");

            // --- Authorization Placeholder ---
            // if (!IsAdminRequest(req)) return req.CreateResponse(HttpStatusCode.Forbidden);
            // --- End Authorization Placeholder ---

            HttpResponseData response;
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;
            if (string.IsNullOrEmpty(requestBody))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Request body is empty.");
                return response;
            }

            try
            {
                var createUserDto = JsonSerializer.Deserialize<CreateUserRequestDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // TODO: Add proper DTO validation here (e.g., using FluentValidation or DataAnnotations attributes)
                if (createUserDto == null || string.IsNullOrWhiteSpace(createUserDto.Email) || string.IsNullOrWhiteSpace(createUserDto.Password))
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    await response.WriteStringAsync("Invalid request body. Email and Password are required.");
                    return response;
                }

                // Delegate to service
                var createdUser = await _userService.CreateUserAsync(createUserDto);

                if (createdUser != null)
                {
                    response = req.CreateResponse(HttpStatusCode.Created);
                    await response.WriteAsJsonAsync(createdUser);
                }
                else
                {
                    // Service layer logged the specific error (e.g., email exists, Firebase error)
                    response = req.CreateResponse(HttpStatusCode.BadRequest); // Or Conflict (409) if email exists
                    await response.WriteStringAsync("Failed to create user. Email might already exist or another error occurred.");
                }
            }
            catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error deserializing request body for CreateUser.");
                 response = req.CreateResponse(HttpStatusCode.BadRequest);
                 await response.WriteStringAsync("Invalid JSON format in request body.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateUser function execution.");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An unexpected error occurred while creating the user.");
            }

            return response;

}
        }
}