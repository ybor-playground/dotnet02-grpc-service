using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dotnet02GrpcService.Server.Services;

/// <summary>
/// JWT validator optimized for API Gateway pattern where authentication happens at the gateway
/// This service focuses on signature verification and claims extraction for authorization
/// </summary>
public class ApiGatewayJwtValidator : IApiGatewayJwtValidator
{
    private readonly TokenValidationParameters _validationParameters;
    private readonly ILogger<ApiGatewayJwtValidator> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public ApiGatewayJwtValidator(IConfiguration configuration, ILogger<ApiGatewayJwtValidator> logger)
    {
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        var jwtKey = configuration["Authentication:Jwt:SecretKey"] ?? 
                    throw new InvalidOperationException("JWT SecretKey is required");

        // Optimized for API Gateway pattern - only validate signature
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            
            // API Gateway already validated these, so we can skip them for performance
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireExpirationTime = false,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero
        };

        _logger.LogInformation("ApiGatewayJwtValidator initialized for signature-only validation");
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            _logger.LogDebug("Validating JWT signature for authorization");
            
            var principal = _tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Token is not a valid JWT");
                return null;
            }

            _logger.LogDebug("JWT signature validation successful. User: {UserId}", 
                GetClaimValue(principal, ClaimTypes.NameIdentifier));
            
            return principal;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "JWT signature validation failed: {Reason}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JWT validation");
            return null;
        }
    }

    public bool IsAuthorized(ClaimsPrincipal principal, string operation)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Authorization denied: User not authenticated");
            return false;
        }

        var userContext = GetUserContext(principal);
        _logger.LogDebug("Authorizing operation {Operation} for user {UserId}", 
            operation, userContext.UserId);

        var authorized = operation.ToLowerInvariant() switch
        {
            "create" => HasPermissionOrRole(userContext, "create", "admin", "write"),
            "read" => HasPermissionOrRole(userContext, "read", "admin", "write", "read"),
            "update" => HasPermissionOrRole(userContext, "update", "admin", "write"),
            "delete" => HasPermissionOrRole(userContext, "delete", "admin"),
            _ => false
        };

        if (!authorized)
        {
            _logger.LogWarning("Authorization denied for operation {Operation}. User: {UserId}, Roles: {Roles}, Permissions: {Permissions}",
                operation, userContext.UserId, string.Join(",", userContext.Roles), string.Join(",", userContext.Permissions));
        }
        else
        {
            _logger.LogDebug("Authorization granted for operation {Operation}. User: {UserId}", 
                operation, userContext.UserId);
        }

        return authorized;
    }

    public UserContext GetUserContext(ClaimsPrincipal principal)
    {
        var userId = GetClaimValue(principal, ClaimTypes.NameIdentifier) ?? "unknown";
        var userName = GetClaimValue(principal, ClaimTypes.Name);
        var clientId = GetClaimValue(principal, "client_id");
        
        var roles = GetClaimValues(principal, ClaimTypes.Role);
        var permissions = GetClaimValues(principal, "permission");

        return new UserContext(userId, userName, clientId, roles, permissions);
    }

    private static bool HasPermissionOrRole(UserContext userContext, string permission, params string[] allowedRoles)
    {
        // Check for explicit permission
        if (userContext.Permissions.Contains(permission))
        {
            return true;
        }

        // Check for required roles
        return allowedRoles.Any(role => userContext.Roles.Contains(role));
    }

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    private static IEnumerable<string> GetClaimValues(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindAll(claimType).Select(c => c.Value);
    }
}