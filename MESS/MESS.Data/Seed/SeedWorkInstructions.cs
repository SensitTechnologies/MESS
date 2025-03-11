using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MESS.Data.Seed;

public static class SeedWorkInstructions
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationContext>();

        if (!context.WorkInstructions.Any()) {
            context.WorkInstructions.AddRange(
                new WorkInstruction
                {
                    Title = "Product Packaging Instructions",
                    Version = "3.0",
                    Operator = new LineOperator
                    {
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
                            LastModifiedOn = default,
                            PartsNeeded = new List<Part>
                            {
                                new Part
                                {
                                    PartName = "Package",
                                    PartNumber = string.Empty,
                                    CreatedBy = "",
                                    CreatedOn = default,
                                    LastModifiedBy = "",
                                    LastModifiedOn = default,
                                    LogId = 1,
                                    Log = new SerialNumberLog()
                                },
                                new Part
                                {
                                    PartName = "Tape",
                                    PartNumber = string.Empty,
                                    CreatedBy = "",
                                    CreatedOn = default,
                                    LastModifiedBy = "",
                                    LastModifiedOn = default,
                                    LogId = 1,
                                    Log = new SerialNumberLog()

                                },
                                new Part
                                {
                                    PartName = "Label",
                                    PartNumber = string.Empty,
                                    CreatedBy = "",
                                    CreatedOn = default,
                                    LastModifiedBy = "",
                                    LastModifiedOn = default,
                                    LogId = 1,
                                    Log = new SerialNumberLog()

                                }
                            }
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