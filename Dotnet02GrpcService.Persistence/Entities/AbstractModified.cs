using System.ComponentModel.DataAnnotations.Schema;

namespace Dotnet02GrpcService.Persistence.Entities;

public class AbstractModified : AbstractCreated
{
            
    [Column("modified")]
    public DateTime? Updated { get; set; }
}