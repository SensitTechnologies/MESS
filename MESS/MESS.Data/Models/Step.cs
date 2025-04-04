using FluentValidation;

namespace MESS.Data.Models;

public class Step : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Body { get; set; }
    public List<string>? Content { get; set; }
    public DateTimeOffset SubmitTime { get; set; }
    public List<Part>? PartsNeeded { get; set; }
}