using Dotnet02GrpcService.API;
using Dotnet02GrpcService.Core;
using Dotnet02GrpcService.Core.Services;
using Dotnet02GrpcService.Core.Exceptions;
using Dotnet02GrpcService.Persistence.Entities;
using Dotnet02GrpcService.Persistence.Models;
using Dotnet02GrpcService.Persistence.Repositories;
using Dotnet02GrpcService.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dotnet02GrpcService.UnitTests.Core;

public class ErrorHandlingTests
{
    private readonly Mock<IDotnet02GrpcRepository> _mockRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<Dotnet02GrpcServiceCore>> _mockLogger;
    private readonly Dotnet02GrpcServiceCore _service;

    public ErrorHandlingTests()
    {
        _mockRepository = new Mock<IDotnet02GrpcRepository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<Dotnet02GrpcServiceCore>>();
        
        _service = new Dotnet02GrpcServiceCore(
            _mockRepository.Object, 
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateDotnet02Grpc_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var request = new Dotnet02GrpcDto { Name = "" };
        _mockValidationService
            .Setup(x => x.ValidateCreateRequest(It.IsAny<Dotnet02GrpcDto>()))
            .Throws(new ValidationException("Name", "Name is required"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateDotnet02Grpc(request));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task GetDotnet02Grpc_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var request = new GetDotnet02GrpcRequest { Id = Guid.NewGuid().ToString() };
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<Dotnet02GrpcEntity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetDotnet02Grpc(request));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("Dotnet02Grpc");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task GetDotnet02Grpc_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        // Arrange
        var request = new GetDotnet02GrpcRequest { Id = "invalid-guid" };
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ValidationException("Id", "Invalid GUID format"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.GetDotnet02Grpc(request));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("Id");
    }

    [Fact]
    public async Task UpdateDotnet02Grpc_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var request = new Dotnet02GrpcDto { Id = Guid.NewGuid().ToString(), Name = "Updated Name" };
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateUpdateRequest(It.IsAny<Dotnet02GrpcDto>()));
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<Dotnet02GrpcEntity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateDotnet02Grpc(request));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("Dotnet02Grpc");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task DeleteDotnet02Grpc_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var request = new DeleteDotnet02GrpcRequest { Id = Guid.NewGuid().ToString() };
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<Dotnet02GrpcEntity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteDotnet02Grpc(request));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("Dotnet02Grpc");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task GetDotnet02Grpcs_ShouldThrowValidationException_WhenPaginationIsInvalid()
    {
        // Arrange
        var request = new GetDotnet02GrpcsRequest { StartPage = -1, PageSize = 0 };
        
        _mockValidationService
            .Setup(x => x.ValidatePaginationRequest(It.IsAny<GetDotnet02GrpcsRequest>()))
            .Throws(new ValidationException(new Dictionary<string, string[]>
            {
                { "StartPage", new[] { "StartPage must be greater than 0" } },
                { "PageSize", new[] { "PageSize must be greater than 0" } }
            }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.GetDotnet02Grpcs(request));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("StartPage");
        exception.ValidationErrors.Should().ContainKey("PageSize");
    }
}