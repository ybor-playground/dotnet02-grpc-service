using System.Security.Claims;

namespace Dotnet02GrpcService.Server.Services;

/// <summary>
/// Interface for validating JWT tokens that have already been authenticated by an API Gateway
/// Focuses on signature verification and claims extraction for authorization
/// </summary>
public interface IApiGatewayJwtValidator
{
    /// <summary>
    /// Validates JWT signature and extracts claims (no full authentication needed)
    /// </summary>
    /// <param name="token">JWT token from API Gateway</param>
    /// <returns>ClaimsPrincipal if signature is valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Determines if the user has permission to perform the specified operation
    /// </summary>
    /// <param name="principal">Claims principal from validated token</param>
    /// <param name="operation">Operation being performed (create, read, update, delete)</param>
    /// <returns>True if authorized, false otherwise</returns>
    bool IsAuthorized(ClaimsPrincipal principal, string operation);

    /// <summary>
    /// Extracts user context information from claims for logging and auditing
    /// </summary>
    /// <param name="principal">Claims principal</param>
    /// <returns>User context information</returns>
    UserContext GetUserContext(ClaimsPrincipal principal);
}

/// <summary>
/// User context information extracted from JWT claims
/// </summary>
public record UserContext(
    string UserId,
    string? UserName,
    string? ClientId,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions);