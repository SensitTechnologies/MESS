using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// Represents a production log that tracks steps and details of a production process.
/// </summary>
public class ProductionLog : AuditableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the production log.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the list of steps associated with the production log.
    /// </summary>
    public List<ProductionLogStep> LogSteps { get; set; } = [];
    
    // Navigation Fields

    /// <summary>
    /// Gets or sets the identifier of the operator associated with the production log.
    /// </summary>
    [ForeignKey("UserId")]
    public string? OperatorId { get; set; }

    /// <summary>
    /// Gets or sets the product associated with the production log.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Gets or sets the work instruction associated with the production log.
    /// </summary>
    public WorkInstruction? WorkInstruction { get; set; }
}