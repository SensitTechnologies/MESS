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
                    Id = 1,
                    Title = "Assembly Line Start-up Procedure",
                    Version = "1.0",
                    Operator = new LineOperator
                    {
                        Id = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        CreatedBy = "",
                        CreatedOn = default,
                        LastModifiedBy = "",
                        LastModifiedOn = default
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Id = 1,
                            Name = "Turn on the main power switch.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 2,
                            Name = "Check all safety equipment.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "null",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 3,
                            Name = "Start the conveyor belt.",
                            StartTime = default,
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
                            Id = 1,
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
                    Id = 2,
                    Title = "Machine Calibration Process",
                    Version = "2.1",
                    Operator = new LineOperator
                    {
                        Id = 2,
                        FirstName = "Jane",
                        LastName = "Smith",
                        CreatedBy = "",
                        CreatedOn = default,
                        LastModifiedBy = "",
                        LastModifiedOn = default
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Id = 4,
                            Name = "Ensure the machine is powered off before calibration.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 5,
                            Name = "Adjust the alignment screws.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 6,
                            Name = "Power on and test calibration.",
                            StartTime = default,
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
                            Id = 2,
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
                    Id = 3,
                    Title = "Product Packaging Instructions",
                    Version = "3.0",
                    Operator = new LineOperator
                    {
                        Id = 3,
                        FirstName = "Mike",
                        LastName = "Johnson",
                        CreatedBy = "",
                        CreatedOn = default,
                        LastModifiedBy = "",
                        LastModifiedOn = default
                    },
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Id = 7,
                            Name = "Place the product in the designated box.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 8,
                            Name = "Seal the box with the appropriate tape.",
                            StartTime = default,
                            CreatedBy = "",
                            CreatedOn = default,
                            LastModifiedBy = "",
                            LastModifiedOn = default
                        },
                        new Step
                        {
                            Id = 9,
                            Name = "Label the package correctly.",
                            StartTime = default,
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
                            Id = 3,
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
        }
    }
}