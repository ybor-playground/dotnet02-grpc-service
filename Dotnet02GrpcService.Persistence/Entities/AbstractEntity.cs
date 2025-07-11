using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dotnet02GrpcService.Persistence.Entities;

public class AbstractEntity <TEntityId> : AbstractModified
{
    [Key]
    [Column("id")]
    [Required]
    public TEntityId Id { get; set; } = default!;
    
}