using Grpc.Core;
using Dotnet02GrpcService.API;
using Dotnet02GrpcService.Client;
using Xunit.Abstractions;
using Xunit;

namespace Dotnet02GrpcService.IntegrationTests;

[Collection("ApplicationCollection")]
public class Dotnet02GrpcServiceGrpcIt(ITestOutputHelper testOutputHelper, ApplicationFixture applicationFixture)
{
    private readonly ApplicationFixture _applicationFixture = applicationFixture;
    private readonly Dotnet02GrpcServiceClient _client = applicationFixture.GetClient();
    [Fact]
    public async Task Test_CreateDotnet02Grpc()
    {
        //Arrange
    
        //Act
        var createRequest = new Dotnet02GrpcDto { Name = Guid.NewGuid().ToString() };
        var response = await _client.CreateDotnet02Grpc(createRequest);
        
        //Assert
        var dto = response.Dotnet02Grpc;
        Assert.NotNull(dto.Id);
        Assert.Equal(createRequest.Name, dto.Name);
    }
    
    [Fact]
    public async Task Test_GetDotnet02Grpcs()
    {
        testOutputHelper.WriteLine("Test_GetDotnet02Grpcs");
        
        //Arrange
        var beforeTotal = (await _client.GetDotnet02Grpcs(new GetDotnet02GrpcsRequest {StartPage = 1, PageSize = 4})).TotalElements;
        
        //Act
        var createRequest = new Dotnet02GrpcDto { Name = Guid.NewGuid().ToString() };
        await _client.CreateDotnet02Grpc(createRequest);
        var response = await _client.GetDotnet02Grpcs(new GetDotnet02GrpcsRequest {StartPage = 1, PageSize = 4});
        
        //Assert
        
        Assert.Equal(beforeTotal + 1, response.TotalElements);
    }
    
    [Fact]
    public async Task Test_GetDotnet02Grpc()
    {
        //Arrange
        var request = new Dotnet02GrpcDto { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.CreateDotnet02Grpc(request);
    
        //Act
        var response = await _client.GetDotnet02Grpc(new GetDotnet02GrpcRequest {Id = createResponse.Dotnet02Grpc.Id});
        
        //Assert
        var dto = response.Dotnet02Grpc;
        Assert.NotNull(dto.Id);
        Assert.Equal(request.Name, dto.Name);
    }

    [Fact]
    public async Task Test_UpdateDotnet02Grpc()
    {
        //Arrange
        var request = new Dotnet02GrpcDto { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.CreateDotnet02Grpc(request);
    
        //Act
        var response = await _client.UpdateDotnet02Grpc(new Dotnet02GrpcDto() {Id = createResponse.Dotnet02Grpc.Id, Name = "Updated"});
        
        //Assert
        var dto = response.Dotnet02Grpc;
        Assert.NotNull(dto.Id);
        Assert.Equal("Updated", response.Dotnet02Grpc.Name);
    }
    
    [Fact]
    public async Task Test_DeleteDotnet02Grpc()
    {
        //Arrange
        var request = new Dotnet02GrpcDto { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.CreateDotnet02Grpc(request);
    
        //Act
        var response = await _client.DeleteDotnet02Grpc(new DeleteDotnet02GrpcRequest{Id = createResponse.Dotnet02Grpc.Id});
        
        //Assert
        Assert.True(response.Deleted);
    }

    [Fact]
    public async Task Test_DeleteDotnet02Grpc_NotFound()
    {
        //Arrange

        //Act
        var exception = await Assert.ThrowsAsync<RpcException>(async () => 
        {
            await _client.DeleteDotnet02Grpc(new DeleteDotnet02GrpcRequest{Id = Guid.NewGuid().ToString()});
        });
       
        //Assert
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Dotnet02Grpc with ID", exception.Status.Detail);
        Assert.Contains("was not found", exception.Status.Detail);
    }

}