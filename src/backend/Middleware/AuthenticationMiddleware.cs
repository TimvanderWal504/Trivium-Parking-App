using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using backend.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using TriviumParkingApp.Backend.Services;
using TriviumParkingApp.Backend.DTOs;

namespace TriviumParkingApp.Backend.Middleware;

public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly IUserService _userService;

    public AuthenticationMiddleware(IUserService userService, ILogger<AuthenticationMiddleware> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    { 
        string firebaseUid;
        try
        {
            firebaseUid = context.GetClaimValue(ClaimTypes.NameIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication context not found.");
            var resp = context.GetHttpResponseData();
            resp!.StatusCode = HttpStatusCode.InternalServerError;
            await resp.WriteStringAsync("Authentication context not found.");
            return;
        }

        if (string.IsNullOrEmpty(firebaseUid))
        {
            var resp = context.GetHttpResponseData();
            resp!.StatusCode = HttpStatusCode.Unauthorized;
            return;
        }

        var user = await _userService
            .SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = firebaseUid });

        if (user == null)
        {
            var resp = context.GetHttpResponseData();
            resp!.StatusCode = HttpStatusCode.InternalServerError;
            await resp.WriteStringAsync("Could not find or create user record.");
            return;
        }

        context.Items[Constants.Constants.Context.UserId] = user.Id;

        await next(context);
    }
}
