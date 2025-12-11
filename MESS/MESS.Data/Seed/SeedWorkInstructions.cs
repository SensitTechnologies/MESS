using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MESS.Data.Seed;
/// <summary>
/// Provides methods to seed sample work instructions into the database.
/// </summary>
public static class SeedWorkInstructions
{
    /// <summary>
    /// Seeder to generate sample work instructions for a new project. 
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static void Seed(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationContext>();

        if (!context.WorkInstructions.Any() || !context.Products.Any())
        {
            var product = new Product
            {
                PartDefinition = new PartDefinition
                {
                    Number = "ABC-001",
                    Name = "ABC Controller"
                },
                IsActive = true
            };

            context.Products.Add(product);
            context.SaveChanges();
            
            var workInstruction = new WorkInstruction
            {
                Title = "ABC Subassembly",
                IsActive = true,
                Version = "2.0",
                Nodes = new List<WorkInstructionNode>
                {
                    new Step
                    {
                        Position = 0,
                        NodeType = WorkInstructionNodeType.Step,
                        Name = "Test display and Humidity Sensor",
                        Body = "Test display and Humidity Sensor.",
                        DetailedBody = "Gather all required parts and tools",
                    },
                    new PartNode
                    {
                        NodeType = WorkInstructionNodeType.Part,
                        Position = 1,
                        PartDefinition = new PartDefinition
                            {
                                Name = "Primary Circuit Board",
                                Number = "1234-G321"
                            }
                    }, 
                    new PartNode
                    {
                        NodeType = WorkInstructionNodeType.Part,
                        Position = 2,
                        PartDefinition = new PartDefinition
                        {
                            Name = "Display Board",
                            Number = "5512-G221"
                        }
                    }, 
                    new PartNode
                    {
                        NodeType = WorkInstructionNodeType.Part,
                        Position = 3,
                        PartDefinition = new PartDefinition
                        {
                            Name = "Humidity Sensor",
                            Number = "1132-H341"
                        }
                    }, 
                    new Step
                    {
                        Position = 4,
                        NodeType = WorkInstructionNodeType.Step,
                        Name = "Attach the Display Board tp the Primary Circuit Board",
                        Body = "Attach the Display Board to the Primary Circuit Board.",
                        DetailedBody = "Attach the Display Board (5512-G221) to the Primary Circuit Board (1234-G231) using 4x T4 Screws."
                    }
                }
            };

            context.WorkInstructions.Add(workInstruction);
            context.SaveChanges();
        }
    }
}