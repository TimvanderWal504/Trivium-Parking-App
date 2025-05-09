using backend.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using TriviumParkingApp.Backend.Services;

namespace TriviumParkingApp.Backend.Functions;

public class AllocationFunctions
{
    private readonly ILogger<AllocationFunctions> _logger;
    private readonly IAllocationService _allocationService;

    public AllocationFunctions(
        ILogger<AllocationFunctions> logger,
        IAllocationService allocationService)
    {
        _logger = logger;
        _allocationService = allocationService;
    }


    [Function("GetUserAllocation")]
    public async Task<HttpResponseData> GetUserAllocations(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "allocations")] HttpRequestData req, FunctionContext context)
    {
        var userId = int.Parse(context.GetClaimValue(ClaimTypes.NameIdentifier));
        _logger.LogInformation("C# HTTP trigger function processed a request to GetUserAllocations for user: {UserId}", userId);   
        HttpResponseData response;

        try
        {
            DateOnly startOfWeek = DateOnly.FromDateTime(DateTime.UtcNow);
            var allocationsDto = await _allocationService.GetUserAllocationsAsync(userId, startOfWeek);

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(allocationsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations for user {UserId}.", userId);
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving allocations.");
        }
        return response;
    }
}