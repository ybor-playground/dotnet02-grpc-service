using System.Security.Claims;

namespace Dotnet02GrpcService.Server.Services;

public interface IAuthenticationService
{
    Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> roles);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    Task<bool> IsAuthorizedAsync(ClaimsPrincipal principal, string operation);
}