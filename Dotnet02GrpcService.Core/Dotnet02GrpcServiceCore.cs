using Grpc.Core;
using Dotnet02GrpcService.API;
using Dotnet02GrpcService.API.Logger;
using Dotnet02GrpcService.Core.Services;
using Dotnet02GrpcService.Core.Exceptions;
using Dotnet02GrpcService.Persistence.Entities;
using Dotnet02GrpcService.Persistence.Models;
using Dotnet02GrpcService.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; 

namespace Dotnet02GrpcService.Core;

public class Dotnet02GrpcServiceCore : IDotnet02GrpcService
{
    private readonly IDotnet02GrpcRepository _dotnet02GrpcRepository;
    private readonly IValidationService _validationService;
    private readonly ILogger<Dotnet02GrpcServiceCore> _logger;
       
    public Dotnet02GrpcServiceCore(
        IDotnet02GrpcRepository dotnet02GrpcRepository,
        IValidationService validationService,
        ILogger<Dotnet02GrpcServiceCore> logger) 
    {
        _dotnet02GrpcRepository = dotnet02GrpcRepository;
        _validationService = validationService;
        _logger = logger;
    }
    public async Task<CreateDotnet02GrpcResponse> CreateDotnet02Grpc(Dotnet02GrpcDto request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}", 
            "CreateDotnet02Grpc", "Dotnet02Grpc");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidateCreateRequest(request);
            
            _logger.LogDebug("Creating Dotnet02Grpc entity: {Name}", request.Name);
            
            try
            {
                var dotnet02Grpc = new Dotnet02GrpcEntity
                {
                    Name = request.Name.Trim()
                };

                _dotnet02GrpcRepository.Save(dotnet02Grpc);
                await _dotnet02GrpcRepository.SaveChangesAsync();
                
                stopwatch.Stop();
                _logger.LogInformation("Successfully created Dotnet02Grpc entity {Id} in {Duration}ms", 
                    dotnet02Grpc.Id, stopwatch.ElapsedMilliseconds);
                
                return new CreateDotnet02GrpcResponse
                {
                    Dotnet02Grpc = new Dotnet02GrpcDto
                    {
                        Id = dotnet02Grpc.Id.ToString(),
                        Name = dotnet02Grpc.Name
                    }
                };
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error creating Dotnet02Grpc entity {Name} after {Duration}ms", 
                    request.Name, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Create", "Failed to save entity to database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error creating Dotnet02Grpc entity {Name} after {Duration}ms", 
                    request.Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for CreateDotnet02Grpc {Name}: {Error} after {Duration}ms", 
                request?.Name, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CreateDotnet02Grpc failed for {Name} after {Duration}ms", 
                request?.Name, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Create", "An unexpected error occurred while creating the entity.", ex);
        }
    }

    public async Task<GetDotnet02GrpcsResponse> GetDotnet02Grpcs(GetDotnet02GrpcsRequest request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}", 
            "GetDotnet02Grpcs", "Dotnet02Grpc");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidatePaginationRequest(request);
            
            var startPage = Math.Max(1, request.StartPage);
            var pageSize = Math.Max(Math.Min(request.PageSize, 100), 1);
            
            _logger.LogDebug("Fetching Dotnet02Grpc entities: page {StartPage}, size {PageSize}", startPage, pageSize);
            
            try
            {
                PageRequest pageRequest = new PageRequest
                {
                    PageSize = pageSize,
                    StartPage = startPage
                };
                
                var page = await _dotnet02GrpcRepository.FindAsync(pageRequest);

                var response = new GetDotnet02GrpcsResponse
                {
                    TotalElements = page.TotalElements,
                    TotalPages = (int)Math.Ceiling((double)page.TotalElements / pageSize)
                };
                response.Dotnet02Grpcs.AddRange(page.Items.Select(dotnet02Grpc => new Dotnet02GrpcDto
                {
                    Id = dotnet02Grpc.Id.ToString(),
                    Name = dotnet02Grpc.Name
                }));

                stopwatch.Stop();
                _logger.LogInformation("Fetched {Count} Dotnet02Grpc entities (total: {Total}) in {Duration}ms", 
                    page.Items.Count, page.TotalElements, stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error fetching Dotnet02Grpc entities page {StartPage}, size {PageSize} after {Duration}ms", 
                    startPage, pageSize, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Read", "Failed to retrieve entities from database.", ex);
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for GetDotnet02Grpcs page {StartPage}, size {PageSize}: {Error} after {Duration}ms", 
                request?.StartPage, request?.PageSize, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetDotnet02Grpcs failed for page {StartPage}, size {PageSize} after {Duration}ms", 
                request?.StartPage, request?.PageSize, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Read", "An unexpected error occurred while retrieving entities.", ex);
        }
    }

    public async Task<GetDotnet02GrpcResponse> GetDotnet02Grpc(GetDotnet02GrpcRequest request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "GetDotnet02Grpc", "Dotnet02Grpc", request.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate and parse ID
            var entityId = _validationService.ValidateAndParseId(request.Id);
            
            _logger.LogDebug("Fetching Dotnet02Grpc entity by ID: {Id}", entityId);
            
            try
            {
                var dotnet02Grpc = await _dotnet02GrpcRepository.FindByIdAsync(entityId);
                if (dotnet02Grpc == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("Dotnet02Grpc entity not found: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("Dotnet02Grpc", entityId.ToString());
                }

                stopwatch.Stop();
                _logger.LogDebug("Found Dotnet02Grpc entity {Id} ({Name}) in {Duration}ms", 
                    dotnet02Grpc.Id, dotnet02Grpc.Name, stopwatch.ElapsedMilliseconds);
                
                return new GetDotnet02GrpcResponse
                {
                    Dotnet02Grpc = new Dotnet02GrpcDto
                    {
                        Id = dotnet02Grpc.Id.ToString(),
                        Name = dotnet02Grpc.Name
                    }
                };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error fetching Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Read", "Failed to retrieve entity from database.", ex);
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for GetDotnet02Grpc {Id}: {Error} after {Duration}ms", 
                request?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetDotnet02Grpc failed for {Id} after {Duration}ms", 
                request?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Read", "An unexpected error occurred while retrieving the entity.", ex);
        }
    }

    public async Task<UpdateDotnet02GrpcResponse> UpdateDotnet02Grpc(Dotnet02GrpcDto dotnet02Grpc)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "UpdateDotnet02Grpc", "Dotnet02Grpc", dotnet02Grpc.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidateUpdateRequest(dotnet02Grpc);
            var entityId = _validationService.ValidateAndParseId(dotnet02Grpc.Id);
            
            _logger.LogDebug("Updating Dotnet02Grpc entity: {Id} - {Name}", entityId, dotnet02Grpc.Name);
            
            try
            {
                var entity = await _dotnet02GrpcRepository.FindByIdAsync(entityId);
                if (entity == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("Dotnet02Grpc entity not found for update: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("Dotnet02Grpc", entityId.ToString());
                }

                // Check for business rules
                if (entity.Name == dotnet02Grpc.Name.Trim())
                {
                    stopwatch.Stop();
                    _logger.LogDebug("No changes detected for Dotnet02Grpc entity {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    
                    return new UpdateDotnet02GrpcResponse
                    {
                        Dotnet02Grpc = new Dotnet02GrpcDto
                        {
                            Id = entity.Id.ToString(),
                            Name = entity.Name
                        }
                    };
                }

                var oldName = entity.Name;
                entity.Name = dotnet02Grpc.Name.Trim();

                _dotnet02GrpcRepository.Update(entity);
                await _dotnet02GrpcRepository.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Updated Dotnet02Grpc entity {Id} from '{OldName}' to '{NewName}' in {Duration}ms", 
                    entity.Id, oldName, entity.Name, stopwatch.ElapsedMilliseconds);

                return new UpdateDotnet02GrpcResponse
                {
                    Dotnet02Grpc = new Dotnet02GrpcDto
                    {
                        Id = entity.Id.ToString(),
                        Name = entity.Name
                    }
                };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "Concurrency conflict updating Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Update", "The entity was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error updating Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Update", "Failed to update entity in database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error updating Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for UpdateDotnet02Grpc {Id}: {Error} after {Duration}ms", 
                dotnet02Grpc?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "UpdateDotnet02Grpc failed for {Id} after {Duration}ms", 
                dotnet02Grpc?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Update", "An unexpected error occurred while updating the entity.", ex);
        }
    }

    public async Task<DeleteDotnet02GrpcResponse> DeleteDotnet02Grpc(DeleteDotnet02GrpcRequest request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "DeleteDotnet02Grpc", "Dotnet02Grpc", request.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate and parse ID
            var entityId = _validationService.ValidateAndParseId(request.Id);
            
            _logger.LogDebug("Deleting Dotnet02Grpc entity by ID: {Id}", entityId);
            
            try
            {
                var dotnet02Grpc = await _dotnet02GrpcRepository.FindByIdAsync(entityId);
                if (dotnet02Grpc == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("Dotnet02Grpc entity not found for deletion: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("Dotnet02Grpc", entityId.ToString());
                }

                var entityName = dotnet02Grpc.Name; // Capture before deletion
                _dotnet02GrpcRepository.Delete(dotnet02Grpc);
                await _dotnet02GrpcRepository.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Deleted Dotnet02Grpc entity {Id} ('{Name}') in {Duration}ms", 
                    dotnet02Grpc.Id, entityName, stopwatch.ElapsedMilliseconds);

                return new DeleteDotnet02GrpcResponse { Deleted = true };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error deleting Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Delete", "Failed to delete entity from database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error deleting Dotnet02Grpc entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for DeleteDotnet02Grpc {Id}: {Error} after {Duration}ms", 
                request?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DeleteDotnet02Grpc failed for {Id} after {Duration}ms", 
                request?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Delete", "An unexpected error occurred while deleting the entity.", ex);
        }
    }
    
}