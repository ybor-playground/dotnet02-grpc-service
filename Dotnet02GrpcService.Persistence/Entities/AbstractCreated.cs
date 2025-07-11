using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dotnet02GrpcService.Persistence.Entities;

public class AbstractCreated
{
    
    [Column("created")]
    [Required]
    public DateTime Created { get; set; }
    
}