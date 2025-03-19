using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace MESS.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public List<int>? ProductionLogIds { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}


public class ApplicationUserValidator : AbstractValidator<ApplicationUser>
{
    public ApplicationUserValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(0, 1024);
        RuleFor(x => x.LastName).NotEmpty().Length(0, 1024);
    }
}