using Dotnet02GrpcService.API;
using Dotnet02GrpcService.Core;
using Dotnet02GrpcService.Core.Services;
using Dotnet02GrpcService.Persistence.Entities;
using Dotnet02GrpcService.Persistence.Models;
using Dotnet02GrpcService.Persistence.Repositories;
using Dotnet02GrpcService.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dotnet02GrpcService.UnitTests.Core;

public class Dotnet02GrpcServiceCoreTests
{
    private readonly Mock<IDotnet02GrpcRepository> _mockRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<Dotnet02GrpcServiceCore>> _mockLogger;
    private readonly Dotnet02GrpcServiceCore _service;

    public Dotnet02GrpcServiceCoreTests()
    {
        _mockRepository = new Mock<IDotnet02GrpcRepository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<Dotnet02GrpcServiceCore>>();
        
        // Setup validation service to allow valid inputs by default
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((id, field) => Guid.Parse(id));

        _service = new Dotnet02GrpcServiceCore(
            _mockRepository.Object, 
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateDotnet02Grpc_ShouldReturnCreatedEntity_WhenValidRequest()
    {
        // Arrange
        var request = new Dotnet02GrpcDto { Name = "Test Entity" };
        var savedEntity = new Dotnet02GrpcEntityBuilder()
            .WithName(request.Name)
            .WithId(Guid.NewGuid())
            .Generate();

        _mockRepository.Setup(x => x.Save(It.IsAny<Dotnet02GrpcEntity>()))
            .Callback<Dotnet02GrpcEntity>(entity => entity.Id = savedEntity.Id);
        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateDotnet02Grpc(request);

        // Assert
        result.Should().NotBeNull();
        result.Dotnet02Grpc.Should().NotBeNull();
        result.Dotnet02Grpc.Name.Should().Be(request.Name);
        result.Dotnet02Grpc.Id.Should().NotBeNullOrEmpty();

        _mockRepository.Verify(x => x.Save(It.Is<Dotnet02GrpcEntity>(e => e.Name == request.Name)), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDotnet02Grpcs_ShouldReturnPagedResults_WhenValidRequest()
    {
        // Arrange
        var request = new GetDotnet02GrpcsRequest { StartPage = 1, PageSize = 10 };
        var entities = new Dotnet02GrpcEntityBuilder().Generate(5);
        var page = new Page<Dotnet02GrpcEntity>
        {
            Items = entities,
            TotalElements = 5
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.GetDotnet02Grpcs(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalElements.Should().Be(5);
        result.Dotnet02Grpcs.Should().HaveCount(5);
        result.Dotnet02Grpcs.Should().AllSatisfy(dto => 
        {
            dto.Id.Should().NotBeNullOrEmpty();
            dto.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData(0, 10)] // StartPage 0 should be normalized to 1
    [InlineData(-1, 10)] // Negative StartPage should be normalized to 1
    [InlineData(1, 0)] // PageSize 0 should be normalized to 1
    [InlineData(1, 150)] // PageSize > 100 should be normalized to 100
    public async Task GetDotnet02Grpcs_ShouldNormalizeParameters_WhenInvalidValues(
        int startPage, int pageSize)
    {
        // Arrange
        var request = new GetDotnet02GrpcsRequest { StartPage = startPage, PageSize = pageSize };
        var page = new Page<Dotnet02GrpcEntity>
        {
            Items = new List<Dotnet02GrpcEntity>(),
            TotalElements = 0
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.GetDotnet02Grpcs(request);

        // Assert
        _mockRepository.Verify(x => x.FindAsync(It.Is<PageRequest>(pr => 
            pr.StartPage >= 1 && pr.PageSize >= 1 && pr.PageSize <= 100)), Times.Once);
    }
}