using Dotnet02GrpcService.API;

namespace Dotnet02GrpcService.Core.Services;

/// <summary>
/// Service for validating requests and business rules
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a Dotnet02Grpc creation request
    /// </summary>
    void ValidateCreateRequest(Dotnet02GrpcDto request);

    /// <summary>
    /// Validates a Dotnet02Grpc update request
    /// </summary>
    void ValidateUpdateRequest(Dotnet02GrpcDto request);

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    void ValidatePaginationRequest(GetDotnet02GrpcsRequest request);

    /// <summary>
    /// Validates an entity ID
    /// </summary>
    Guid ValidateAndParseId(string id, string fieldName = "Id");
}