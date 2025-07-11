namespace Dotnet02GrpcService.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("dotnet02_grpc")]
public class Dotnet02GrpcEntity : AbstractEntity<Guid>
{
    [Column("name")]
    [Required]
    public string? Name { get; set; }
}