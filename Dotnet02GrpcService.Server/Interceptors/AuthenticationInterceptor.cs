using Grpc.Core;
using Grpc.Core.Interceptors;
using Dotnet02GrpcService.Server.Services;

namespace Dotnet02GrpcService.Server.Interceptors;

public class AuthenticationInterceptor : Interceptor
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthenticationInterceptor> _logger;

    public AuthenticationInterceptor(
        IAuthenticationService authService, 
        ILogger<AuthenticationInterceptor> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            // Skip authentication for health checks and certain methods
            if (IsPublicMethod(context.Method))
            {
                return await continuation(request, context);
            }

            var token = GetTokenFromMetadata(context.RequestHeaders);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Missing authentication token for method: {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication token required"));
            }

            var principal = await _authService.ValidateTokenAsync(token);
            if (principal == null)
            {
                _logger.LogWarning("Invalid authentication token for method: {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid authentication token"));
            }

            // Add principal to context for authorization
            var httpContext = context.GetHttpContext();
            if (httpContext != null)
            {
                httpContext.User = principal;
            }

            // Check authorization
            var operation = GetOperationFromMethod(context.Method);
            var isAuthorized = await _authService.IsAuthorizedAsync(principal, operation);
            
            if (!isAuthorized)
            {
                _logger.LogWarning("Authorization failed for user {UserId} on operation {Operation}", 
                    principal.Identity?.Name, operation);
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Insufficient permissions"));
            }

            _logger.LogDebug("Authentication and authorization successful for user: {UserId}", 
                principal.Identity?.Name);

            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for method: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Authentication service error"));
        }
    }

    private static string? GetTokenFromMetadata(Metadata headers)
    {
        var authHeader = headers.FirstOrDefault(h => h.Key.Equals("authorization", StringComparison.OrdinalIgnoreCase));
        if (authHeader == null) return null;

        const string bearerPrefix = "Bearer ";
        return authHeader.Value.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase) 
            ? authHeader.Value[bearerPrefix.Length..] 
            : authHeader.Value;
    }

    private static bool IsPublicMethod(string method)
    {
        // Allow health checks and server reflection without authentication
        return method.Contains("grpc.reflection") || 
               method.Contains("grpc.health") ||
               method.Contains("Health");
    }

    private static string GetOperationFromMethod(string method)
    {
        // Extract operation type from gRPC method name
        var methodName = method.Split('/').LastOrDefault()?.ToLowerInvariant() ?? "";
        
        return methodName switch
        {
            var m when m.Contains("create") => "create",
            var m when m.Contains("get") || m.Contains("find") || m.Contains("list") => "read",
            var m when m.Contains("update") || m.Contains("modify") => "update",
            var m when m.Contains("delete") || m.Contains("remove") => "delete",
            _ => "read" // Default to read permission
        };
    }
}