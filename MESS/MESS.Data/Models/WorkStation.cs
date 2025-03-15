namespace MESS.Data.Models;

public class WorkStation : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    
    // Product navigation
    public List<Product> Products { get; set; } = [];
    public List<WorkInstruction> WorkInstructions { get; set; } = [];
}