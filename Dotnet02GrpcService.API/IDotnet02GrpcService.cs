
namespace Dotnet02GrpcService.API;

public interface IDotnet02GrpcService
{
    Task<CreateDotnet02GrpcResponse> CreateDotnet02Grpc(Dotnet02GrpcDto dotnet02Grpc);
    Task<GetDotnet02GrpcsResponse> GetDotnet02Grpcs(GetDotnet02GrpcsRequest request);
    Task<GetDotnet02GrpcResponse> GetDotnet02Grpc(GetDotnet02GrpcRequest request);
    Task<UpdateDotnet02GrpcResponse> UpdateDotnet02Grpc(Dotnet02GrpcDto dotnet02Grpc);
    Task<DeleteDotnet02GrpcResponse> DeleteDotnet02Grpc(DeleteDotnet02GrpcRequest request);
    
}