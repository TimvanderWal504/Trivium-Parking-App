using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Text;
using TriviumParkingApp.Backend.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TriviumParkingApp.Backend.Middleware;

/// <summary>
/// Middleware to validate Firebase JWT tokens from the Authorization header.
/// </summary>
public class FirebaseAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<FirebaseAuthMiddleware> _logger;
    private readonly UserManager<Models.User> _userManager;
    private readonly FirebaseAuth _firebaseAuth;

    public FirebaseAuthMiddleware(ILogger<FirebaseAuthMiddleware> logger, FirebaseApp firebaseApp, UserManager<Models.User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        _firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var req = await context.GetHttpRequestDataAsync();

        if (req == null)
        {
             _logger.LogDebug("FirebaseAuthMiddleware: Not an HTTP trigger or HttpRequestData not found, skipping.");
             await next(context);
             return;
        }

        var authorizationHeader = req.Headers?.FirstOrDefault(h => string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("FirebaseAuthMiddleware: No Authorization header found or header is not Bearer type.");

            var unauthorizedResponse = await SetUnauthorizedResponse(context);
            context.GetInvocationResult().Value = unauthorizedResponse;

            return;
        }


        var idToken = authorizationHeader.Substring("Bearer ".Length).Trim();

        try
        {
            FirebaseToken decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken);
            var uid = decodedToken.Uid;
            _logger.LogInformation("FirebaseAuthMiddleware: Token verified successfully for UID: {Uid}", uid);

            var claims = decodedToken.Claims.Select(c => new Claim(c.Key, c.Value.ToString() ?? "")).ToList();
            var user = await _userManager.FindByNameAsync(uid);
            if (user != null)
            {
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                if (decodedToken.Claims.TryGetValue("email", out var email))
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString() ?? ""));
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Firebase"));
            context.Features.Set(principal); 

            await next(context);
        }
        catch (FirebaseAuthException ex)
        {
            var unauthorizedResponse = await SetUnauthorizedResponse(context);
            context.GetInvocationResult().Value = unauthorizedResponse;
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FirebaseAuthMiddleware: An unexpected error occurred during token verification.");
            context.GetInvocationResult().Value = await SetErrorResponse(context);
            return;
        }
    }

    private async Task<HttpResponseData> SetUnauthorizedResponse(FunctionContext context)
    {
        var requestData = await context.GetHttpRequestDataAsync();
        var response = requestData.CreateResponse(HttpStatusCode.Unauthorized);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync("Unauthorized: Invalid or missing authentication token.", Encoding.UTF8);
        return response;
    }


    private async Task<HttpResponseData> SetErrorResponse(FunctionContext context)
    {
        var requestData = await context.GetHttpRequestDataAsync();
        var response = requestData.CreateResponse(HttpStatusCode.InternalServerError);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync("Internal Server Error during authentication.", Encoding.UTF8);
        return response;
    }
}