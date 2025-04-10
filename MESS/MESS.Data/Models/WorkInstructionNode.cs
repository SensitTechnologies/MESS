namespace MESS.Data.Models;

public abstract class WorkInstructionNode
{
    public int Id { get; set; }
    public int Position { get; set; }
    public WorkInstructionNodeType NodeType { get; set; }
}

public enum WorkInstructionNodeType
{
    Part,
    Step
}