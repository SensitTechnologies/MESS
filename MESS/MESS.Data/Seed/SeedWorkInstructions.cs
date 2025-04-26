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
                Name = "ABC Controller",
                IsActive = true,
            };

            context.Products.Add(product);
            context.SaveChanges();
            
            var workInstruction = new WorkInstruction
            {
                Title = "ABC Subassembly",
                IsActive = true,
                Version = "1.2",
                Nodes = new List<WorkInstructionNode>
                {
                    new Step
                    {
                        Position = 0,
                        Name = "Test display and Humidity Sensor.",
                        Body = "Gather all required parts and tools",
                    },
                    new PartNode
                    {
                        Position = 1,
                        Parts = new List<Part>
                        {
                            new Part
                            {
                                PartName = "Primary Circuit Board",
                                PartNumber = "1234-G321"
                            },
                            new Part
                            {
                                PartName = "Display Board",
                                PartNumber = "5512-G221"
                            },
                            new Part
                            {
                                PartName = "Humidity Sensor",
                                PartNumber = "1132-H341"
                            }
                        }
                    },
                    new Step
                    {
                        Position = 2,
                        Name = "Attach the Display Board to the Primary Circuit Board.",
                        Body = "Attach the Display Board (5512-G221) to the Primary Circuit Board (1234-G231) using 4x T4 Screws."
                    }
                }
            };

            context.WorkInstructions.Add(workInstruction);
            context.SaveChanges();
        }
    }
}