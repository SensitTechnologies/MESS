using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace MESS.Data.Models;

public class LineOperator : AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public int? ProductionLogId { get; set; }
    public ProductionLog? ProductionLog { get; set; }
}


public class LineOperatorValidator : AbstractValidator<LineOperator>
{
    public LineOperatorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(0, 1024);
        RuleFor(x => x.LastName).NotEmpty().Length(0, 1024);
    }
}