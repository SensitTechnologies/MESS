using FluentValidation;

namespace MESS.Data.Models;

public class Step : WorkInstructionNode
{
    public required string Name { get; set; }
    public string? Body { get; set; }
    public List<string>? Content { get; set; }
    public DateTimeOffset SubmitTime { get; set; }
}