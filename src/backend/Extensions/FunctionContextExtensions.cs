using Microsoft.Azure.Functions.Worker;
using System.Security.Claims;

namespace TriviumParkingApp.Backend.Extensions;

public static class FunctionContextExtensions
{
    /// <summary>
    /// Retrieves the value of the specified claim from the ClaimsPrincipal found in the context features.
    /// </summary>
    /// <param name="context">The function context that contains the features.</param>
    /// <param name="claimType">The claim type to search for (e.g., ClaimTypes.NameIdentifier).</param>
    /// <returns>The value of the claim if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the context is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the ClaimsPrincipal cannot be retrieved from the context features.</exception>
    public static string? GetClaimValue(this FunctionContext context, string claimType)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Retrieve the ClaimsPrincipal from the context features.
        var principal = context.Features.Get<ClaimsPrincipal>();
        if (principal == null)
        {
            throw new InvalidOperationException("ClaimsPrincipal not found in FunctionContext features. Ensure that your authentication middleware is correctly configured.");
        }

        // Return the value of the specified claim if it exists.
        return principal.FindFirst(claimType)?.Value;
    }
}
