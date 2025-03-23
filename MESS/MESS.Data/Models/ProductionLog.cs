using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace MESS.Data.Models;

public class ProductionLog : AuditableEntity
{
    public int Id { get; set; }
    
    public List<ProductionLogStep> LogSteps { get; set; } = [];
    
    // Navigation Fields
    [ForeignKey("UserId")]
    public string? OperatorId { get; set; }
    public Product? Product { get; set; }
    public WorkStation? WorkStation { get; set; }
    public WorkInstruction? WorkInstruction { get; set; }
}