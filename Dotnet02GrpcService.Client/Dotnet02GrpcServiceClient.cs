using Grpc.Net.Client;
using Dotnet02GrpcService.API;

namespace Dotnet02GrpcService.Client;

public class Dotnet02GrpcServiceClient : IDotnet02GrpcService
{
    private readonly API.Dotnet02GrpcService.Dotnet02GrpcServiceClient stub;

    private Dotnet02GrpcServiceClient(GrpcChannel channel)
    {
        stub = new API.Dotnet02GrpcService.Dotnet02GrpcServiceClient(channel);
    }

    public static Dotnet02GrpcServiceClient Of(string host)
    {
        return new Dotnet02GrpcServiceClient(GrpcChannel.ForAddress(host));
    }

    public async Task<CreateDotnet02GrpcResponse> CreateDotnet02Grpc(Dotnet02GrpcDto dotnet02Grpc) {
        return await stub.CreateDotnet02GrpcAsync(dotnet02Grpc);
    }

    public async Task<GetDotnet02GrpcsResponse> GetDotnet02Grpcs(GetDotnet02GrpcsRequest request) {
        return await stub.GetDotnet02GrpcsAsync(request);
    }

    public async Task<GetDotnet02GrpcResponse> GetDotnet02Grpc(GetDotnet02GrpcRequest request) {
        return await stub.GetDotnet02GrpcAsync(request);
    }

    public async Task<UpdateDotnet02GrpcResponse> UpdateDotnet02Grpc(Dotnet02GrpcDto dotnet02Grpc) {
        return await stub.UpdateDotnet02GrpcAsync(dotnet02Grpc);
    }

    public async Task<DeleteDotnet02GrpcResponse> DeleteDotnet02Grpc(DeleteDotnet02GrpcRequest request) {
        return await stub.DeleteDotnet02GrpcAsync(request);
    }
    

}