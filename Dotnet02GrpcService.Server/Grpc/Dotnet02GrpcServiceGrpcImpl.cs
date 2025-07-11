using Grpc.Core;
using Dotnet02GrpcService.API;
using Dotnet02GrpcService.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dotnet02GrpcService.Server.Grpc;

public class Dotnet02GrpcServiceGrpcImpl : API.Dotnet02GrpcService.Dotnet02GrpcServiceBase
{
    private readonly ILogger<Dotnet02GrpcServiceGrpcImpl> _logger;
    private readonly Dotnet02GrpcServiceCore _service;
    
    public Dotnet02GrpcServiceGrpcImpl(Dotnet02GrpcServiceCore service, ILogger<Dotnet02GrpcServiceGrpcImpl> logger)
    {
        _service = service;
        _logger = logger;
    }

    public override async Task<CreateDotnet02GrpcResponse> CreateDotnet02Grpc(Dotnet02GrpcDto request, ServerCallContext context)
    {
        using var scope = _logger.BeginScope("gRPC: {Method}, User: {UserId}", 
            nameof(CreateDotnet02Grpc), GetUserId(context));
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("gRPC CreateDotnet02Grpc started for {Name}", request.Name);
            
            var response = await _service.CreateDotnet02Grpc(request);
            
            stopwatch.Stop();
            _logger.LogInformation("gRPC CreateDotnet02Grpc completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "gRPC CreateDotnet02Grpc failed after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public override async Task<GetDotnet02GrpcResponse> GetDotnet02Grpc(GetDotnet02GrpcRequest request, ServerCallContext context)
    {
        using var scope = _logger.BeginScope("gRPC: {Method}, User: {UserId}, Id: {Id}", 
            nameof(GetDotnet02Grpc), GetUserId(context), request.Id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("gRPC GetDotnet02Grpc started for ID {Id}", request.Id);
            
            var response = await _service.GetDotnet02Grpc(request);
            
            stopwatch.Stop();
            _logger.LogInformation("gRPC GetDotnet02Grpc completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "gRPC GetDotnet02Grpc failed for ID {Id} after {Duration}ms", 
                request.Id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public override async Task<GetDotnet02GrpcsResponse> GetDotnet02Grpcs(GetDotnet02GrpcsRequest request, ServerCallContext context)
    {
        using var scope = _logger.BeginScope("gRPC: {Method}, User: {UserId}, Page: {StartPage}, Size: {PageSize}", 
            nameof(GetDotnet02Grpcs), GetUserId(context), request.StartPage, request.PageSize);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("gRPC GetDotnet02Grpcs started for page {StartPage}, size {PageSize}", 
                request.StartPage, request.PageSize);
            
            var response = await _service.GetDotnet02Grpcs(request);
            
            stopwatch.Stop();
            _logger.LogInformation("gRPC GetDotnet02Grpcs completed successfully in {Duration}ms - returned {Count}/{Total} items", 
                stopwatch.ElapsedMilliseconds, response.Dotnet02Grpcs.Count, response.TotalElements);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "gRPC GetDotnet02Grpcs failed for page {StartPage}, size {PageSize} after {Duration}ms", 
                request.StartPage, request.PageSize, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public override async Task<UpdateDotnet02GrpcResponse> UpdateDotnet02Grpc(Dotnet02GrpcDto request, ServerCallContext context)
    {
        using var scope = _logger.BeginScope("gRPC: {Method}, User: {UserId}, Id: {Id}", 
            nameof(UpdateDotnet02Grpc), GetUserId(context), request.Id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("gRPC UpdateDotnet02Grpc started for ID {Id}", request.Id);
            
            var response = await _service.UpdateDotnet02Grpc(request);
            
            stopwatch.Stop();
            _logger.LogInformation("gRPC UpdateDotnet02Grpc completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "gRPC UpdateDotnet02Grpc failed for ID {Id} after {Duration}ms", 
                request.Id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public override async Task<DeleteDotnet02GrpcResponse> DeleteDotnet02Grpc(DeleteDotnet02GrpcRequest request, ServerCallContext context)
    {
        using var scope = _logger.BeginScope("gRPC: {Method}, User: {UserId}, Id: {Id}", 
            nameof(DeleteDotnet02Grpc), GetUserId(context), request.Id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("gRPC DeleteDotnet02Grpc started for ID {Id}", request.Id);
            
            var response = await _service.DeleteDotnet02Grpc(request);
            
            stopwatch.Stop();
            _logger.LogInformation("gRPC DeleteDotnet02Grpc completed successfully in {Duration}ms - deleted: {Deleted}", 
                stopwatch.ElapsedMilliseconds, response.Deleted);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "gRPC DeleteDotnet02Grpc failed for ID {Id} after {Duration}ms", 
                request.Id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    /// <summary>
    /// Extracts user ID from gRPC context metadata.
    /// </summary>
    private static string? GetUserId(ServerCallContext context)
    {
        // Try to get user ID from various possible metadata entries
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h => 
            string.Equals(h.Key, "user-id", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(h.Key, "x-user-id", StringComparison.OrdinalIgnoreCase));
        
        return userIdEntry?.Value;
    }
}