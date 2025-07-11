using Dotnet02GrpcService.Persistence.Context;
using Dotnet02GrpcService.Persistence.Entities;
using Dotnet02GrpcService.Persistence.Models;
using Dotnet02GrpcService.Persistence.Repositories;
using Dotnet02GrpcService.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dotnet02GrpcService.UnitTests.Persistence;

public class Dotnet02GrpcRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Dotnet02GrpcRepository _repository;
    private readonly ILogger<Dotnet02GrpcRepository> _logger;

    public Dotnet02GrpcRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _logger = new Mock<ILogger<Dotnet02GrpcRepository>>().Object;
        _repository = new Dotnet02GrpcRepository(_context, _logger);
    }

    [Fact]
    public void Save_ShouldAddEntityToContext()
    {
        // Arrange
        var entity = new Dotnet02GrpcEntityBuilder().Generate();

        // Act
        _repository.Save(entity);

        // Assert
        _context.Entry(entity).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntity()
    {
        // Arrange
        var entity = new Dotnet02GrpcEntityBuilder().Generate();
        _repository.Save(entity);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var savedEntity = await _context.Dotnet02Grpcs.FindAsync(entity.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var entities = new Dotnet02GrpcEntityBuilder().Generate(15);
        await _context.Dotnet02Grpcs.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var pageRequest = new PageRequest { StartPage = 1, PageSize = 10 };

        // Act
        var result = await _repository.FindAsync(pageRequest);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalElements.Should().Be(15);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmptyPage_WhenNoEntities()
    {
        // Arrange
        var pageRequest = new PageRequest { StartPage = 1, PageSize = 10 };

        // Act
        var result = await _repository.FindAsync(pageRequest);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalElements.Should().Be(0);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var entity = new Dotnet02GrpcEntityBuilder().Generate();
        await _context.Dotnet02Grpcs.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.FindByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}