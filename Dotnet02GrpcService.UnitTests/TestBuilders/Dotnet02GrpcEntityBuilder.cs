using AutoBogus;
using Dotnet02GrpcService.Persistence.Entities;

namespace Dotnet02GrpcService.UnitTests.TestBuilders;

public class Dotnet02GrpcEntityBuilder : AutoFaker<Dotnet02GrpcEntity>
{
    public Dotnet02GrpcEntityBuilder()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Company.CompanyName());
        RuleFor(x => x.Created, f => f.Date.Past());
        RuleFor(x => x.Updated, f => f.Date.Recent());
    }

    public Dotnet02GrpcEntityBuilder WithName(string name)
    {
        RuleFor(x => x.Name, name);
        return this;
    }

    public Dotnet02GrpcEntityBuilder WithId(Guid id)
    {
        RuleFor(x => x.Id, id);
        return this;
    }
}