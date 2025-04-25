using FirebaseAdmin; // For FirebaseApp
using FirebaseAdmin.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http; // For HttpRequestData extensions
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Text; // For Encoding

namespace TriviumParkingApp.Backend.Middleware
{
    /// <summary>
    /// Middleware to validate Firebase JWT tokens from the Authorization header.
    /// </summary>
    public class FirebaseAuthMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<FirebaseAuthMiddleware> _logger;
        private readonly FirebaseAuth _firebaseAuth; // Inject FirebaseAuth instance

        public FirebaseAuthMiddleware(ILogger<FirebaseAuthMiddleware> logger, FirebaseApp firebaseApp)
        {
            _logger = logger;
            _firebaseAuth = FirebaseAuth.GetAuth(firebaseApp); // Get FirebaseAuth from FirebaseApp
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // Try to get HttpRequestData using the ASYNC extension method
            var req = await context.GetHttpRequestDataAsync(); // Await the async method

            if (req == null)
            {
                 // Not an HTTP trigger or unable to get request data, skip middleware
                 _logger.LogDebug("FirebaseAuthMiddleware: Not an HTTP trigger or HttpRequestData not found, skipping.");
                 await next(context);
                 return;
            }

            // --- Get Token from Header ---
            string? authorizationHeader = req.Headers?.FirstOrDefault(h => string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("FirebaseAuthMiddleware: No Authorization header found or header is not Bearer type.");

                var unauthorizedResponse = await SetUnauthorizedResponse(context); // return the HttpResponseData
                context.GetInvocationResult().Value = unauthorizedResponse;

                return;
            }


            string idToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            // --- Verify Token ---
            try
            {
                FirebaseToken decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                _logger.LogInformation("FirebaseAuthMiddleware: Token verified successfully for UID: {Uid}", uid);

                // --- Create ClaimsPrincipal (Optional but recommended) ---
                // Attach the verified user information to the FunctionContext so functions can access it
                var claims = decodedToken.Claims.Select(c => new Claim(c.Key, c.Value.ToString() ?? "")).ToList();
                // Add standard claims if not present in custom claims
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier)) claims.Add(new Claim(ClaimTypes.NameIdentifier, uid));
                // Add email if available
                if (decodedToken.Claims.TryGetValue("email", out var email)) claims.Add(new Claim(ClaimTypes.Email, email.ToString() ?? ""));

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Firebase"));
                context.Features.Set(principal); // Make principal available via context.Features.Get<ClaimsPrincipal>()

                // Proceed to the next middleware or the function itself
                await next(context);
            }
            catch (FirebaseAuthException ex)
            {
                var unauthorizedResponse = await SetUnauthorizedResponse(context); // return the HttpResponseData
                context.GetInvocationResult().Value = unauthorizedResponse;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FirebaseAuthMiddleware: An unexpected error occurred during token verification.");
                context.GetInvocationResult().Value = await SetErrorResponse(context); // Set the 500 response
                return; // IMPORTANT: Stop processing
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
}