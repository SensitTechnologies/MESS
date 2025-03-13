using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MESS.Data.Seed;

public static class SeedWorkInstructions
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationContext>();

        if (!context.WorkInstructions.Any())
        {
            context.WorkInstructions.AddRange(
                new WorkInstruction
                {
                    Title = "Assembly Line Start-up Procedure",
                    Version = "1.0",
                    Operator = new LineOperator
                    {
                        FirstName = "John",
                        LastName = "Doe",
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Name = "Turn on the main power switch.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Check all safety equipment.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "null",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Start the conveyor belt.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        }
                    },
                    RelatedDocumentation = new List<Documentation>
                    {
                        new Documentation
                        {
                            Title = "Safety Guidelines",
                            ContentType = "",
                            Content = "",
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
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
                    Operator = new LineOperator
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Name = "Ensure the machine is powered off before calibration.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Adjust the alignment screws.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Power on and test calibration.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        }
                    },
                    RelatedDocumentation = new List<Documentation>
                    {
                        new Documentation
                        {
                            Title = "Calibration Manual",
                            Content = "https://example.com/calibration",
                            ContentType = "",
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
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
                    Operator = new LineOperator
                    {
                        FirstName = "Mike",
                        LastName = "Johnson",
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Name = "Place the product in the designated box.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Seal the box with the appropriate tape.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Name = "Label the package correctly.",
                            SubmitTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        }
                    },
                    RelatedDocumentation = new List<Documentation>
                    {
                        new Documentation
                        {
                            Title = "Packaging Standards",
                            Content = "https://example.com/packaging",
                            ContentType = "",
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        }
                    },
                    CreatedBy = "",
                    CreatedOn = default,
                    LastModifiedBy = "",
                    LastModifiedOn = default
                });
            context.SaveChanges();
        }
    }
}