using Grpc.Core;
using Grpc.Core.Interceptors;
using Dotnet02GrpcService.Server.Services;
using System.Diagnostics;

namespace Dotnet02GrpcService.Server.Interceptors;

/// <summary>
/// Interceptor for tracking gRPC request metrics
/// </summary>
public class MetricsInterceptor : Interceptor
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsInterceptor> _logger;

    public MetricsInterceptor(MetricsService metricsService, ILogger<MetricsInterceptor> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = GetMethodName(context.Method);
        var status = "success";
        
        _metricsService.RecordConnectionOpened();

        try
        {
            _logger.LogDebug("Starting request for method: {Method}", method);
            
            var response = await continuation(request, context);
            
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            
            _logger.LogDebug("Completed request for method: {Method} in {Duration}ms", 
                method, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            
            // Record business metrics based on method
            RecordBusinessMetrics(method);
            
            return response;
        }
        catch (RpcException ex)
        {
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            status = ex.StatusCode.ToString().ToLowerInvariant();
            
            _logger.LogWarning("Request failed for method: {Method} with status: {Status} in {Duration}ms", 
                method, status, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            _metricsService.RecordError(method, ex.StatusCode.ToString());
            
            // Record specific error metrics
            RecordErrorMetrics(method, ex);
            
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            status = "internal_error";
            
            _logger.LogError(ex, "Request failed for method: {Method} with unexpected error in {Duration}ms", 
                method, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            _metricsService.RecordError(method, ex.GetType().Name);
            
            throw;
        }
        finally
        {
            _metricsService.RecordConnectionClosed();
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        return await TrackStreamingRequest(
            () => continuation(requestStream, context),
            context.Method);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await TrackStreamingRequest(
            () => continuation(request, responseStream, context),
            context.Method);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await TrackStreamingRequest(
            () => continuation(requestStream, responseStream, context),
            context.Method);
    }

    private async Task<T> TrackStreamingRequest<T>(Func<Task<T>> operation, string methodPath)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = GetMethodName(methodPath);
        var status = "success";
        
        _metricsService.RecordConnectionOpened();

        try
        {
            _logger.LogDebug("Starting streaming request for method: {Method}", method);
            
            var result = await operation();
            
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            
            _logger.LogDebug("Completed streaming request for method: {Method} in {Duration}ms", 
                method, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            RecordBusinessMetrics(method);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            status = ex is RpcException rpcEx ? rpcEx.StatusCode.ToString().ToLowerInvariant() : "internal_error";
            
            _logger.LogWarning(ex, "Streaming request failed for method: {Method} with status: {Status} in {Duration}ms", 
                method, status, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            _metricsService.RecordError(method, ex.GetType().Name);
            
            throw;
        }
        finally
        {
            _metricsService.RecordConnectionClosed();
        }
    }

    private async Task TrackStreamingRequest(Func<Task> operation, string methodPath)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = GetMethodName(methodPath);
        var status = "success";
        
        _metricsService.RecordConnectionOpened();

        try
        {
            _logger.LogDebug("Starting streaming request for method: {Method}", method);
            
            await operation();
            
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            
            _logger.LogDebug("Completed streaming request for method: {Method} in {Duration}ms", 
                method, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            RecordBusinessMetrics(method);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            status = ex is RpcException rpcEx ? rpcEx.StatusCode.ToString().ToLowerInvariant() : "internal_error";
            
            _logger.LogWarning(ex, "Streaming request failed for method: {Method} with status: {Status} in {Duration}ms", 
                method, status, stopwatch.ElapsedMilliseconds);
            
            _metricsService.RecordRequest(method, status, durationSeconds);
            _metricsService.RecordError(method, ex.GetType().Name);
            
            throw;
        }
        finally
        {
            _metricsService.RecordConnectionClosed();
        }
    }

    private static string GetMethodName(string methodPath)
    {
        // Extract method name from path like "/package.service/MethodName"
        return methodPath.Split('/').LastOrDefault() ?? "unknown";
    }

    private void RecordBusinessMetrics(string method)
    {
        var methodLower = method.ToLowerInvariant();
        
        if (methodLower.StartsWith("create"))
        {
            _metricsService.RecordEntityCreated();
        }
        else if (methodLower.StartsWith("update"))
        {
            _metricsService.RecordEntityUpdated();
        }
        else if (methodLower.StartsWith("delete"))
        {
            _metricsService.RecordEntityDeleted();
        }
    }

    private void RecordErrorMetrics(string method, RpcException ex)
    {
        switch (ex.StatusCode)
        {
            case StatusCode.InvalidArgument:
                _metricsService.RecordValidationError("invalid_argument");
                break;
            case StatusCode.Unauthenticated:
            case StatusCode.PermissionDenied:
                var operation = GetOperationFromMethod(method);
                _metricsService.RecordAuthorizationFailure(operation, ex.StatusCode.ToString());
                break;
        }
    }

    private static string GetOperationFromMethod(string method)
    {
        var methodLower = method.ToLowerInvariant();
        return methodLower switch
        {
            var name when name.StartsWith("create") => "create",
            var name when name.StartsWith("get") || name.StartsWith("list") => "read",
            var name when name.StartsWith("update") => "update",
            var name when name.StartsWith("delete") => "delete",
            _ => "unknown"
        };
    }
}