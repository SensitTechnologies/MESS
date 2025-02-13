namespace MESS.Data.Models;

public class Problem
{
    public int Id { get; set; }
    public required string ProblemCode { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation fields
    public List<Product> Products { get; set; } = [];
}