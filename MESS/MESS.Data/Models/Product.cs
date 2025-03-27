namespace MESS.Data.Models;

public class Product : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation fields
    public List<WorkInstruction>? WorkInstructions { get; set; } = [];
    public List<Problem>? Problems { get; set; } = [];
}