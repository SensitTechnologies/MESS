using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace MESS.Data.Models;

public class LineOperator : IdentityUser
{
    [Key]
    public new int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public List<int>? ProductionLogIds { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}


public class LineOperatorValidator : AbstractValidator<LineOperator>
{
    public LineOperatorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(0, 1024);
        RuleFor(x => x.LastName).NotEmpty().Length(0, 1024);
    }
}