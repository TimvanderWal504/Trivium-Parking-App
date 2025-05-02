using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Services;

namespace TriviumParkingApp.Backend.Functions;

public class UserManagementFunctions
{
    private readonly ILogger<UserManagementFunctions> _logger;
    private readonly IUserService _userService; 

    public UserManagementFunctions(ILogger<UserManagementFunctions> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("GetUsers")]
    public async Task<HttpResponseData> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to GetUsers.");

        HttpResponseData response;
        try
        {
            var users = await _userService.GetAllUsersAsync();

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users.");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving users.");
        }
        return response;
    }

    [Function("AssignUserRole")]
    public async Task<HttpResponseData> AssignUserRole(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/{userId}/role")] HttpRequestData req,
        string userId)
    {
         _logger.LogInformation("C# HTTP trigger function processed a request to AssignUserRole for user ID: {UserId}", userId);

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
            var requestData = JsonSerializer.Deserialize<AssignRoleRequestDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (requestData == null || string.IsNullOrWhiteSpace(requestData.RoleName))
            {
                 response = req.CreateResponse(HttpStatusCode.BadRequest);
                 await response.WriteStringAsync("Invalid request body. Required format: { \"roleName\": \"RoleName\" }");
                 return response;
            }

            bool success = await _userService.AssignRoleAsync(targetUserId, requestData.RoleName);

            if (success)
            {
                response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Role '{requestData.RoleName}' processed for user ID {targetUserId}.");
            }
            else
            {
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


    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to CreateUser.");
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

            if (createUserDto == null || string.IsNullOrWhiteSpace(createUserDto.Email) || string.IsNullOrWhiteSpace(createUserDto.Password))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid request body. Email and Password are required.");
                return response;
            }

            var createdUser = await _userService.CreateUserAsync(createUserDto);

            if (createdUser != null)
            {
                response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createdUser);
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
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