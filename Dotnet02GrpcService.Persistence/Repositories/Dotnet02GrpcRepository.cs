using Microsoft.EntityFrameworkCore;
using Dotnet02GrpcService.Persistence.Context;
using Dotnet02GrpcService.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace Dotnet02GrpcService.Persistence.Repositories;

public class Dotnet02GrpcRepository : BaseRepository<Dotnet02GrpcEntity, Guid>, IDotnet02GrpcRepository
{
    public Dotnet02GrpcRepository(AppDbContext context, ILogger<Dotnet02GrpcRepository> logger) 
        : base(context, logger)
    {
    }
}