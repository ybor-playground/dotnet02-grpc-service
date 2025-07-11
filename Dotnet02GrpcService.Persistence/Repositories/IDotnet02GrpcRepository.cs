using Dotnet02GrpcService.Persistence.Entities;
using Dotnet02GrpcService.Persistence.Models;

namespace Dotnet02GrpcService.Persistence.Repositories;

public interface IDotnet02GrpcRepository
{
    void Save(Dotnet02GrpcEntity entity);
    Task<Dotnet02GrpcEntity?> FindByIdAsync(Guid id);
    Task<Page<Dotnet02GrpcEntity>> FindAsync(PageRequest request);
    void Update(Dotnet02GrpcEntity entity);
    void Delete(Dotnet02GrpcEntity entity);
    Task SaveChangesAsync();
}