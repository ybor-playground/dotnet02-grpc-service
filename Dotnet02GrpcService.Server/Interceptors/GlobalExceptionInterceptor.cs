using Dotnet02GrpcService.Core.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dotnet02GrpcService.Server.Interceptors;

/// <summary>
/// Global interceptor for handling exceptions and converting them to appropriate gRPC status codes
/// </summary>
public class GlobalExceptionInterceptor : Interceptor
{
    private readonly ILogger<GlobalExceptionInterceptor> _logger;

    public GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    private RpcException HandleException(Exception exception, ServerCallContext context)
    {
        return exception switch
        {
            RpcException rpcEx => HandleRpcException(rpcEx, context),
            ValidationException validationEx => HandleValidationException(validationEx, context),
            EntityNotFoundException notFoundEx => HandleEntityNotFoundException(notFoundEx, context),
            BusinessRuleException businessEx => HandleBusinessRuleException(businessEx, context),
            DataAccessException dataEx => HandleDataAccessException(dataEx, context),
            DomainException domainEx => HandleDomainException(domainEx, context),
            _ => HandleGenericException(exception, context)
        };
    }

    private RpcException HandleRpcException(RpcException rpcException, ServerCallContext context)
    {
        // Log the original RPC exception but don't change it
        _logger.LogWarning("RPC exception occurred: {StatusCode} - {Detail}", 
            rpcException.StatusCode, rpcException.Status.Detail);
        
        return rpcException;
    }

    private RpcException HandleValidationException(ValidationException validationException, ServerCallContext context)
    {
        _logger.LogWarning("Validation error: {ErrorCode} - {Message} - Details: {@ValidationErrors}", 
            validationException.ErrorCode, validationException.Message, validationException.ValidationErrors);

        var metadata = new Metadata
        {
            { "error-code", validationException.ErrorCode },
            { "validation-errors", JsonSerializer.Serialize(validationException.ValidationErrors) }
        };

        return new RpcException(new Status(StatusCode.InvalidArgument, validationException.Message), metadata);
    }

    private RpcException HandleEntityNotFoundException(EntityNotFoundException notFoundException, ServerCallContext context)
    {
        _logger.LogWarning("Entity not found: {EntityType} with ID {EntityId}", 
            notFoundException.EntityType, notFoundException.EntityId);

        var metadata = new Metadata
        {
            { "error-code", notFoundException.ErrorCode },
            { "entity-type", notFoundException.EntityType },
            { "entity-id", notFoundException.EntityId }
        };

        return new RpcException(new Status(StatusCode.NotFound, notFoundException.Message), metadata);
    }

    private RpcException HandleBusinessRuleException(BusinessRuleException businessException, ServerCallContext context)
    {
        _logger.LogWarning("Business rule violation: {RuleName} - {Message}", 
            businessException.RuleName, businessException.Message);

        var metadata = new Metadata
        {
            { "error-code", businessException.ErrorCode },
            { "rule-name", businessException.RuleName }
        };

        return new RpcException(new Status(StatusCode.FailedPrecondition, businessException.Message), metadata);
    }

    private RpcException HandleDataAccessException(DataAccessException dataException, ServerCallContext context)
    {
        _logger.LogError(dataException, "Data access error during {Operation}: {Message}", 
            dataException.Operation, dataException.Message);

        var metadata = new Metadata
        {
            { "error-code", dataException.ErrorCode },
            { "operation", dataException.Operation }
        };

        // Don't expose internal database details to clients
        var publicMessage = dataException.Operation switch
        {
            "Create" => "Failed to create the entity. Please try again.",
            "Read" => "Failed to retrieve the entity. Please try again.",
            "Update" => "Failed to update the entity. Please try again.",
            "Delete" => "Failed to delete the entity. Please try again.",
            _ => "A data access error occurred. Please try again."
        };

        return new RpcException(new Status(StatusCode.Internal, publicMessage), metadata);
    }

    private RpcException HandleDomainException(DomainException domainException, ServerCallContext context)
    {
        _logger.LogWarning("Domain exception: {ErrorCode} - {Message}", 
            domainException.ErrorCode, domainException.Message);

        var metadata = new Metadata
        {
            { "error-code", domainException.ErrorCode }
        };

        if (domainException.ErrorDetails != null)
        {
            metadata.Add("error-details", JsonSerializer.Serialize(domainException.ErrorDetails));
        }

        return new RpcException(new Status(StatusCode.InvalidArgument, domainException.Message), metadata);
    }

    private RpcException HandleGenericException(Exception exception, ServerCallContext context)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {ExceptionType} - {Message}", 
            exception.GetType().Name, exception.Message);

        var metadata = new Metadata
        {
            { "error-code", "INTERNAL_ERROR" },
            { "exception-type", exception.GetType().Name }
        };

        return new RpcException(new Status(StatusCode.Internal, "An internal error occurred. Please try again later."), metadata);
    }
}