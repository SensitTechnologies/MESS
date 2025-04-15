namespace MESS.Data.Models;

public class Step : WorkInstructionNode
{
    public required string Name { get; set; }
    public string? Body { get; set; }
    public List<string> PrimaryMedia { get; set; } = [];
    public List<string> SecondaryMedia { get; set; } = [];
}