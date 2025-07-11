using Dotnet02GrpcService.Client;
using Dotnet02GrpcService.Server;

namespace Dotnet02GrpcService.IntegrationTests;

public class ApplicationFixture: IDisposable
{
    private readonly Dotnet02GrpcServiceServer _server;
    private readonly Dotnet02GrpcServiceClient _client;
    public ApplicationFixture()
    {
        _server = new Dotnet02GrpcServiceServer()
            .WithEphemeral()
            .WithRandomPorts()
            .Start();
        
        var grpcUrl = _server.getGrpcUrl();
        if (string.IsNullOrEmpty(grpcUrl))
        {
            throw new InvalidOperationException("Failed to get gRPC server URL");
        }
        
        _client = Dotnet02GrpcServiceClient.Of(grpcUrl);
    }
    
    public Dotnet02GrpcServiceClient GetClient() => _client;
    public Dotnet02GrpcServiceServer GetServer() => _server;

    public void Dispose()
    {
        _server.Stop();
    }
}

[CollectionDefinition("ApplicationCollection")]
public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    // This class has no code; it's just a marker for the test collection
}