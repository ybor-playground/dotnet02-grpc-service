using Dotnet02GrpcService.Server.Services;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Dotnet02GrpcService.Server.Interceptors;

/// <summary>
/// Authorization interceptor optimized for API Gateway pattern
/// Validates JWT signature and performs claims-based authorization
/// Does NOT perform authentication (that's done by API Gateway)
/// </summary>
public class AuthorizationInterceptor : Interceptor
{
    private readonly IApiGatewayJwtValidator _jwtValidator;
    private readonly ILogger<AuthorizationInterceptor> _logger;

    // Methods that don't require authorization
    private static readonly HashSet<string> PublicMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "/grpc.health.v1.Health/Check",
        "/grpc.reflection.v1alpha.ServerReflection/ServerReflectionInfo"
    };

    public AuthorizationInterceptor(IApiGatewayJwtValidator jwtValidator, ILogger<AuthorizationInterceptor> logger)
    {
        _jwtValidator = jwtValidator;
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            // Skip authorization for public methods
            if (IsPublicMethod(context.Method))
            {
                _logger.LogDebug("Skipping authorization for public method: {Method}", context.Method);
                return await continuation(request, context);
            }

            // Extract JWT token from gRPC metadata
            var token = GetTokenFromMetadata(context.RequestHeaders);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization denied: No JWT token provided for method {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization token required"));
            }

            // Validate JWT signature and extract claims
            var principal = _jwtValidator.ValidateToken(token);
            if (principal == null)
            {
                _logger.LogWarning("Authorization denied: Invalid JWT signature for method {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid authorization token"));
            }

            // Set principal in HTTP context for downstream services
            var httpContext = context.GetHttpContext();
            if (httpContext != null)
            {
                httpContext.User = principal;
            }

            // Get user context for logging
            var userContext = _jwtValidator.GetUserContext(principal);
            
            // Add correlation metadata for logging
            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userContext.UserId,
                ["UserName"] = userContext.UserName ?? "unknown",
                ["ClientId"] = userContext.ClientId ?? "unknown",
                ["Method"] = context.Method
            });

            // Perform operation-based authorization
            var operation = GetOperationFromMethod(context.Method);
            var isAuthorized = _jwtValidator.IsAuthorized(principal, operation);
            
            if (!isAuthorized)
            {
                _logger.LogWarning("Authorization denied: Insufficient permissions for operation {Operation}", operation);
                throw new RpcException(new Status(StatusCode.PermissionDenied, 
                    $"Insufficient permissions for {operation} operation"));
            }

            _logger.LogDebug("Authorization successful for operation {Operation}", operation);
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            // Re-throw gRPC exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authorization for method: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Authorization service error"));
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await AuthorizeRequest(context);
        return await continuation(requestStream, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await AuthorizeRequest(context);
        await continuation(request, responseStream, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await AuthorizeRequest(context);
        await continuation(requestStream, responseStream, context);
    }

    private Task AuthorizeRequest(ServerCallContext context)
    {
        if (IsPublicMethod(context.Method)) return Task.CompletedTask;

        var token = GetTokenFromMetadata(context.RequestHeaders);
        if (string.IsNullOrEmpty(token))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization token required"));
        }

        var principal = _jwtValidator.ValidateToken(token);
        if (principal == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid authorization token"));
        }

        var httpContext = context.GetHttpContext();
        if (httpContext != null)
        {
            httpContext.User = principal;
        }

        var operation = GetOperationFromMethod(context.Method);
        var isAuthorized = _jwtValidator.IsAuthorized(principal, operation);
        
        if (!isAuthorized)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, 
                $"Insufficient permissions for {operation} operation"));
        }
        
        return Task.CompletedTask;
    }

    private static bool IsPublicMethod(string method)
    {
        return PublicMethods.Contains(method);
    }

    private static string? GetTokenFromMetadata(Metadata headers)
    {
        var authHeader = headers.FirstOrDefault(h => 
            string.Equals(h.Key, "authorization", StringComparison.OrdinalIgnoreCase));
        
        if (authHeader == null) return null;

        var headerValue = authHeader.Value;
        if (string.IsNullOrEmpty(headerValue)) return null;

        // Support both "Bearer <token>" and just "<token>" formats
        const string bearerPrefix = "Bearer ";
        if (headerValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return headerValue.Substring(bearerPrefix.Length).Trim();
        }

        return headerValue.Trim();
    }

    private static string GetOperationFromMethod(string method)
    {
        // Extract operation from gRPC method name
        // Examples: /package.service/CreateEntity -> create
        var methodName = method.Split('/').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
        
        return methodName switch
        {
            var name when name.StartsWith("create") => "create",
            var name when name.StartsWith("get") || name.StartsWith("list") || name.StartsWith("find") => "read",
            var name when name.StartsWith("update") || name.StartsWith("patch") => "update",
            var name when name.StartsWith("delete") || name.StartsWith("remove") => "delete",
            _ => "read" // Default to read for unknown operations
        };
    }
}