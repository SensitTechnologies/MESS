namespace MESS.Data.Models;

public class Step : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Body { get; set; }
    // public List<Action>? Actions { get; set; }
    public bool Success { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public List<Part>? PartsNeeded { get; set; }
    // public string? ImagePath { get; set; }
}