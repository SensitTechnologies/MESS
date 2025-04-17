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

        if (!context.WorkInstructions.Any()) {
            context.WorkInstructions.AddRange(
                new WorkInstruction
                {
                    Title = "Assembly Line Start-up Procedure",
                    Version = "1.0",
                    Nodes = new List<WorkInstructionNode>
                    {
                        new Step
                        {
                            Name = "Turn on the main power switch.",
                        },
                        new Step
                        {
                            Name = "Check all safety equipment.",
                        },
                        new Step
                        {
                            Name = "Start the conveyor belt.",
                        }
                    },
                    CreatedBy = "",
                    CreatedOn = default,
                    LastModifiedBy = "",
                    LastModifiedOn = default
                },
                new WorkInstruction
                {
                    Title = "Machine Calibration Process",
                    Version = "2.1",
                    Nodes = new List<WorkInstructionNode>
                    {
                        new Step
                        {
                            Name = "Ensure the machine is powered off before calibration.",
                        },
                        new Step
                        {
                            Name = "Adjust the alignment screws.",
                        },
                        new Step
                        {
                            Name = "Power on and test calibration.",
                        }
                    },
                    CreatedBy = "",
                    CreatedOn = default,
                    LastModifiedBy = "",
                    LastModifiedOn = default
                },
                new WorkInstruction
                {
                    Title = "Product Packaging Instructions",
                    Version = "3.0",
                    Nodes = new List<WorkInstructionNode>
                    {
                        new Step
                        {
                            Name = "Place the product in the designated box.",
                        },
                        new Step
                        {
                            Name = "Seal the box with the appropriate tape.",
                        },
                        new Step
                        {
                            Name = "Label the package correctly.",
                        }
                    }
                });
            context.SaveChanges();
        }
    }
}