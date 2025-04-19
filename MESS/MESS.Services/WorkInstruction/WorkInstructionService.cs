using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MESS.Data.Context;
using MESS.Data.DTO;
using MESS.Services.Product;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Drawing;
using System.Net;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace MESS.Services.WorkInstruction;
using MESS.Data.Models;
/// <inheritdoc />
public partial class WorkInstructionService : IWorkInstructionService
{
    private readonly IProductService _productService;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    // The following attributes define the current expected XLSX structure as of 3/28/2025
    private const string INSTRUCTION_TITLE_CELL = "B1";
    private const string VERSION_CELL = "D1";
    private const string PRODUCT_NAME_CELL = "B2";
    private const string QR_CODE_REQUIRED_CELL = "D2";
    private const string STEPS_PARTS_LIST_CELL = "B3";
    
    
    // Using int values here since there is an indeterminate amount of steps per Work Instruction
    private const int STEP_START_ROW = 7;
    private const int STEP_START_COLUMN = 1;
    private const int STEP_TITLE_COLUMN = 2;
    private const int STEP_DESCRIPTION_COLUMN = 3;
    private const int STEP_PRIMARY_MEDIA_COLUMN = 4;
    private const int STEP_SECONDARY_MEDIA_COLUMN = 5;

    private const string WORK_INSTRUCTION_IMAGES_DIRECTORY = "WorkInstructionImages";
    const string WORK_INSTRUCTION_CACHE_KEY = "AllWorkInstructions";

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionService"/> class.
    /// </summary>
    /// <param name="productService">The service for managing product-related operations.</param>
    /// <param name="cache">The memory cache for caching work instructions.</param>
    /// <param name="webHostEnvironment">The hosting environment for accessing web root paths.</param>
    /// <param name="contextFactory">The factory for creating database contexts.</param>
    public WorkInstructionService(IProductService productService, IMemoryCache cache, IWebHostEnvironment webHostEnvironment, IDbContextFactory<ApplicationContext> contextFactory)
    {
        _productService = productService;
        _cache = cache;
        _webHostEnvironment = webHostEnvironment;
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public string? ExportToXlsx(WorkInstruction workInstructionToExport)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Work Instruction");

            // Set basic data in the header
            worksheet.Cell(INSTRUCTION_TITLE_CELL).Value = workInstructionToExport.Title;
            worksheet.Cell(VERSION_CELL).Value = workInstructionToExport.Version;
            worksheet.Cell(QR_CODE_REQUIRED_CELL).Value = workInstructionToExport.PartsRequired;
            
            // Since a work instruction can be associated with multiple Products it is stored as a comma seperated list
            if (workInstructionToExport.Products.Count > 0)
            {
                var productNames = string.Join(", ", workInstructionToExport.Products.Select(p => p.Name));
                worksheet.Cell(PRODUCT_NAME_CELL).Value = productNames;
            }
            
            // Build parts list string in format "(PART_NAME, PART_NUMBER), ..."
            var partNodes = workInstructionToExport.Nodes
                .Where(n => n is PartNode)
                .Cast<PartNode>()
                .ToList();
            
            if (partNodes.Count > 0)
            {
                var allParts = partNodes.SelectMany(pn => pn.Parts).ToList();
                var partsListStrings = allParts.Select(p => $"({p.PartName}, {p.PartNumber})");
                worksheet.Cell(STEPS_PARTS_LIST_CELL).Value = string.Join(", ", partsListStrings);
            }
            
            // Setup headers
            worksheet.Cell("A1").Value = "Title:";
            worksheet.Cell("C1").Value = "Version:";
            worksheet.Cell("A2").Value = "Product:";
            worksheet.Cell("C2").Value = "QR Code Required:";
            worksheet.Cell("A3").Value = "Required Parts:";
            
            // Step Header row. With bold.
            var currentRow = 6;
            worksheet.Cell(currentRow, 1).Value = "#";
            worksheet.Cell(currentRow, 2).Value = "Title";
            worksheet.Cell(currentRow, 3).Value = "Description";
            worksheet.Cell(currentRow, 4).Value = "Primary Media";
            worksheet.Cell(currentRow, 5).Value = "Secondary Media";

            var stepNodes = workInstructionToExport.Nodes
                .Where(n => n is Step)
                .Cast<Step>()
                .OrderBy(s => s.Position)
                .ToList();

            currentRow++;
            var stepNumber = 1;
        
            foreach (var step in stepNodes)
            {
                worksheet.Cell(currentRow, 1).Value = stepNumber++;
                
                // Currently just stripping the html off of the steps to get the content out.
                worksheet.Cell(currentRow, 2).Value = StripHtmlTags(step.Name);
                worksheet.Cell(currentRow, 3).Value = StripHtmlTags(step.Body);
                
                // ApplyFormattingToCells(worksheet.Cell(currentRow, 2), step.Name); 
                // ApplyFormattingToCells(worksheet.Cell(currentRow, 3), step.Body);
            
                // Handle primary media
                if (step.PrimaryMedia.Count > 0)
                {
                    worksheet.Cell(currentRow, 4).Value = string.Join(", ", step.PrimaryMedia);
                }
            
                // Handle secondary media
                if (step.SecondaryMedia.Count > 0)
                {
                    worksheet.Cell(currentRow, 5).Value = string.Join(", ", step.SecondaryMedia);
                }
            
                currentRow++;
            }
            
            // Auto-fit column widths
            worksheet.Columns().AdjustToContents();
        
            // Create directory for exports if it doesn't exist
            var exportDir = Path.Combine(_webHostEnvironment.WebRootPath, "WorkInstructionExports");
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }
        
            // Save the workbook
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            var safeTitle = string.Join("_", workInstructionToExport.Title.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{safeTitle}_{timestamp}.xlsx";
            var filePath = Path.Combine(exportDir, fileName);
        
            workbook.SaveAs(filePath);
        
            Log.Information("Successfully exported work instruction '{Title}' to '{FilePath}'", 
                workInstructionToExport.Title, filePath);
            
            return filePath;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to export work instruction: {workInstructionTitle} to xlsx. Exception type: {exceptionType}", workInstructionToExport.Title, e.GetType());
            return null;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="htmlContent"></param>
    /// <returns></returns>
    private void ApplyFormattingToCells(IXLCell cell, string? htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            return;
        }

        htmlContent = htmlContent.Trim();
        
        // Remove div wrapper as it should never have any styles associated with it
        if (htmlContent.StartsWith("<div>") && htmlContent.EndsWith("</div>"))
        {
            htmlContent = htmlContent.Substring(5, htmlContent.Length - 11);
        }

        var plainText = StripHtmlTags(htmlContent);
        cell.Value = plainText;
        
        // TODO finish full implementation
    }

    /// <summary>
    /// Removes HTML tags from a string.
    /// </summary>
    private string StripHtmlTags(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        return RemoveHTMLRegex().Replace(input, string.Empty);
    }

    /// <inheritdoc />
    public async Task<WorkInstruction?> DuplicateAsync(WorkInstruction workInstructionToDuplicate)
    {
        if (workInstructionToDuplicate == null)
        {
            Log.Warning("Cannot duplicate null work instruction");
            return null;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.ChangeTracker.Clear();
            
            // Use the execution strategy pattern instead of manual transactions
            return await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var freshWorkInstruction = await context.WorkInstructions
                        .Include(w => w.Products)
                        .Include(w => w.Nodes)
                        .ThenInclude(partNode => ((PartNode)partNode).Parts)
                        .FirstOrDefaultAsync(w => w.Id == workInstructionToDuplicate.Id);

                    if (freshWorkInstruction == null)
                    {
                        return null;
                    }
                    
                    var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
                    var copiedTitle = $"{freshWorkInstruction.Title}-copy-{timestamp}";

                    var newWorkInstruction = new WorkInstruction
                    {
                        Title = copiedTitle,
                        Version = freshWorkInstruction.Version,
                        IsActive = false,
                        PartsRequired = freshWorkInstruction.PartsRequired
                    };

                    await context.WorkInstructions.AddAsync(newWorkInstruction);
                    await context.SaveChangesAsync();

                    foreach (var product in freshWorkInstruction.Products)
                    {
                        var trackedProduct = await context.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == product.Id);
                        
                        if (trackedProduct != null)
                        {
                            var attachedProduct = await context.Products.FindAsync(product.Id);
                            if (attachedProduct != null)
                            {
                                attachedProduct.WorkInstructions?.Add(newWorkInstruction);
                                newWorkInstruction.Products.Add(attachedProduct);
                            }
                        }
                    }

                    foreach (var originalNode in freshWorkInstruction.Nodes)
                    {
                        WorkInstructionNode newNode;
            
                        if (originalNode is Step originalStep)
                        {
                            var newStep = new Step
                            {
                                Name = originalStep.Name,
                                Body = originalStep.Body,
                                Position = originalStep.Position,
                                NodeType = originalStep.NodeType,
                                PrimaryMedia = [..originalStep.PrimaryMedia],
                                SecondaryMedia = [..originalStep.SecondaryMedia]
                            };
                            
                            newNode = newStep;
                        }
                        else if (originalNode is PartNode originalPartNode)
                        {
                            var newPartNode = new PartNode
                            {
                                Position = originalPartNode.Position,
                                NodeType = originalPartNode.NodeType,
                                Parts = []
                            };
                
                            foreach (var part in originalPartNode.Parts)
                            {
                                newPartNode.Parts.Add(part);
                            }
                
                            newNode = newPartNode;
                        }
                        else
                        {
                            return null;
                        }
            
                        newWorkInstruction.Nodes.Add(newNode);
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);

                    Log.Information("Successfully duplicated WorkInstruction {OriginalId} to new ID {NewId}",
                        freshWorkInstruction.Id, newWorkInstruction.Id);

                    return newWorkInstruction;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    Log.Error("An exception occurred while duplicating the work instruction: {Message}", e.Message);
                    return null;
                }
            });
        }
        catch (Exception e)
        {
            Log.Error("Failed to execute duplication strategy: {Message}", e.Message);
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> IsEditable(WorkInstruction workInstruction)
    {
        if (workInstruction is not { Id: > 0 })
        {
            Log.Warning("Cannot check editability: Work instruction is null or has invalid ID");
            return false;
        }
        
        try
        {
            await using var localContext = await _contextFactory.CreateDbContextAsync();
            
            // Check if any production logs reference this work instruction. If so the Instruction is not editable.
            var hasProductionLogs = await localContext.ProductionLogs
                .AsNoTracking()
                .AnyAsync(p => p.WorkInstruction != null && p.WorkInstruction.Id == workInstruction.Id);

            return !hasProductionLogs;
        }
        catch (Exception e)
        {
            Log.Information("Unable to determine if WorkInstruction: {WorkInstructionTitle} is editable. Exception Type: {ExceptionType}", workInstruction.Title, e.GetType());
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<WorkInstructionImportResult> ImportFromXlsx(List<IBrowserFile> files)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (files.Count == 0)
            {
                Log.Warning("No files provided for import.");
                return WorkInstructionImportResult.NoFilesProvided();
            }
            
            var file = files.First();
            using var memoryStream = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheet(1);
            
            var versionString = worksheet.Cell(VERSION_CELL).GetString();
            var workInstructionTitle = worksheet.Cell(INSTRUCTION_TITLE_CELL).GetString();
            
            // Check for pre-existing WorkInstruction that have matching title + version
            var preexistingWorkInstruction = await context.WorkInstructions
                .Where(w => w.Title == workInstructionTitle && w.Version == versionString)
                .AnyAsync();

            if (preexistingWorkInstruction)
            {
                Log.Information("Unable to import Work Instruction. Pre-existing work instruction found for Title: {Title}, Version: {Version}. ", workInstructionTitle, versionString);
                return WorkInstructionImportResult.DuplicateWorkInstructionFound(file.Name, workInstructionTitle, versionString);
            }
            var partsRequired = worksheet.Cell(QR_CODE_REQUIRED_CELL).GetValue<bool>();
            
            var workInstruction = new WorkInstruction
            {
                Title = workInstructionTitle,
                Version = versionString,
                Products = [],
                Nodes = [],
                PartsRequired = partsRequired
            };
            
            // Retrieve Product and assign relationship
            var productString = worksheet.Cell(PRODUCT_NAME_CELL).GetString();
            var product = await _productService.FindByTitleAsync(productString);

            if (product == null)
            {
                Log.Information("Product not found. Cannot create Work Instruction");
                return WorkInstructionImportResult.NoProductFound(files.First().Name, productString);
            }
            
            workInstruction.Products.Add(product);

            // Create all steps within instruction
            // Add any required parts to the work instruction
            var partsNode = new PartNode();
            partsNode.NodeType = WorkInstructionNodeType.Part;
            var partsList = await GetPartsListFromString(worksheet.Cell(STEPS_PARTS_LIST_CELL).GetString());

            if (partsList != null)
            {
                partsNode.Parts.AddRange(partsList);
                // Making PartsNode position 0 since there is no inherent way to get the desired position from
                // an Excel spreadsheet. It is possible, but with the ability to alter Node positions this feature
                // was placed on the backlog.
                partsNode.Position = 0;
                workInstruction.Nodes.Add(partsNode);
            }


            // Start from row 7 (assuming header row is 6)
            var stepStartRow = STEP_START_ROW;
            while (!worksheet.Cell(stepStartRow, STEP_START_COLUMN).IsEmpty())
            {
                var step = new Step
                {
                    Name = ProcessCellText(worksheet.Cell(stepStartRow, STEP_TITLE_COLUMN), workbook),
                    Body = ProcessCellText(worksheet.Cell(stepStartRow, STEP_DESCRIPTION_COLUMN), workbook),
                };
                
                await ProcessStepMedia(worksheet, step, stepStartRow);
                
                // Step extends WorkInstructionNode for ordering purposes
                // Calculation 0 Indexes the step order. 1st step has position 1 if there are parts, otherwise position 0.
                var rowPositionCalculation = stepStartRow - STEP_START_ROW;
                step.Position = partsList != null ? rowPositionCalculation + 1 : rowPositionCalculation;
                step.NodeType = WorkInstructionNodeType.Step;
                workInstruction.Nodes.Add(step);
                stepStartRow++;
            }
            
            var fileNames = files.Select(browserFile => browserFile.Name).ToList();

            if (await Create(workInstruction))
            {
                Log.Information("Successfully imported WorkInstruction from Excel: {title}", workInstruction.Title);
                // Not invalidating cache here, since it gets invalidated on successful creation
                return WorkInstructionImportResult.Success(fileNames, workInstruction);
            }

            return WorkInstructionImportResult.Failure(fileNames);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to import WorkInstruction from uploaded Excel file");
            var fileNames = files.Select(f => f.Name).ToList();
            var errorResult = WorkInstructionImportResult.Failure(fileNames);

            var error = new ImportError
            {
                File = fileNames.Count > 0 ? fileNames.First() : "Unknown",
                Message = e.Message,
                ErrorType = e.GetType().Name
            };

            switch (e)
            {
                case IOException or UnauthorizedAccessException:
                    error.ErrorType = "File Access Error";
                    error.Message = $"Could not access file: {e.Message}";
                    break;
                case ArgumentException or FormatException:
                    error.ErrorType = "Excel Format Error";
                    error.Message = $"Invalid format in Excel file: {e.Message}";
                    break;
                case IndexOutOfRangeException:
                    error.ErrorType = "Excel Structure Error";
                    error.Message = "Could not find expected worksheet or cell references";
                    break;
                case NullReferenceException:
                    error.ErrorType = "Missing Data";
                    error.Message = "Required data is missing from the Excel file";
                    break;
                case DbUpdateException:
                    error.ErrorType = "Database Error";
                    error.Message = "Failed to save work instruction to database";
                    break;
                case InvalidOperationException:
                    error.ErrorType = "Processing Error";
                    error.Message = "Failed to process file data";
                    break;
            }

            errorResult.ImportError = error;
            return errorResult;
        }
    }

    /// <summary>
    /// Processes media (images) associated with a step in a work instruction from an Excel worksheet.
    /// </summary>
    /// <param name="worksheet">The Excel worksheet containing the step data.</param>
    /// <param name="step">The step object to which the media will be added.</param>
    /// <param name="row">The row number in the worksheet corresponding to the step.</param>
    /// <remarks>
    /// This method extracts images from specific columns in the worksheet, saves them to the server, 
    /// and associates their paths with the step object. It handles both primary and secondary media.
    /// </remarks>
    /// <exception cref="Exception">
    /// Logs any exceptions that occur during the processing of step media, such as file I/O errors or 
    /// issues with the Excel worksheet structure.
    /// </exception>
    private async Task ProcessStepMedia(IXLWorksheet worksheet, Step step, int row)
    {
        try
        {
            var primaryPictures = worksheet.Pictures
                .Where(p => p.TopLeftCell.Address.RowNumber == row 
                            && p.TopLeftCell.Address.ColumnNumber == STEP_PRIMARY_MEDIA_COLUMN)
                .ToList();
                
            var secondaryPictures = worksheet.Pictures
                .Where(p => p.TopLeftCell.Address.RowNumber == row 
                            && p.TopLeftCell.Address.ColumnNumber == STEP_SECONDARY_MEDIA_COLUMN)
                .ToList();
            
            var imageDir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);
            
            // Verify that directory exists. If not create it.
            if (!Directory.Exists(imageDir))
            {
                Directory.CreateDirectory(imageDir);
            }
            
            foreach (var picture in primaryPictures)
            {
                var fileName = $"{Guid.NewGuid()}.{picture.Format.ToString()}";
                
                var imagePath = Path.Combine(imageDir, fileName);
                    
                
                using (var ms = new MemoryStream())
                {
                    await picture.ImageStream.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    await File.WriteAllBytesAsync(imagePath, imageBytes);
                }
                
                step.PrimaryMedia.Add(Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName));
            }
            
            foreach (var picture in secondaryPictures)
            {
                var fileName = $"{Guid.NewGuid()}.{picture.Format.ToString()}";
                
                var imagePath = Path.Combine(imageDir, fileName);
                    
                
                using (var ms = new MemoryStream())
                {
                    await picture.ImageStream.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    await File.WriteAllBytesAsync(imagePath, imageBytes);
                }
                
                step.SecondaryMedia.Add(Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName));
            }
        }
        catch (Exception e)
        {
            Log.Information("Exception thrown when attempting to process Step media for Workbook: {workbookName}. Exception: {ExceptionMessage}", worksheet.ToString(), e.Message);
        }
    }

    /// <summary>
    /// Converts rich text from an Excel cell into HTML format.
    /// </summary>
    /// <param name="cell">The Excel cell <see cref="IXLCell"/> containing text to convert.</param>
    /// <param name="workbook">The Excel Workbook <see cref="XLWorkbook"/> containing text to convert.</param>
    /// <returns>
    /// HTML-formatted string representing the cell's content. If the cell contains rich text,
    /// the method generates HTML with appropriate styling to preserve formatting.
    /// If the cell does not contain rich text, it returns the plain string value.
    /// </returns>
    /// <remarks>
    /// Supported rich text attributes:
    /// - Bold text is styled with font-weight: bold
    /// - Italic text is styled with font-style: italic
    /// - Font size is applied with font-size in points
    /// - Font color is preserved
    /// 
    /// All text is HTML-encoded to prevent XSS vulnerabilities.
    /// The result is wrapped in a div element.
    /// </remarks>
    private string ProcessCellText(IXLCell cell, XLWorkbook workbook)
    {
        var sb = new StringBuilder();
        sb.Append("<div>");

        var hasHyperLink = cell.HasHyperlink;
        string? hyperLinkUri = null;
        
        
        if (hasHyperLink)
        {
            var hyperLink = cell.GetHyperlink();
            hyperLinkUri = hyperLink.ExternalAddress.AbsoluteUri;
        }

        if (!cell.HasRichText)
        {
            string content = cell.GetString();

            string cellColor = "";
            
            if (cell.Style.Font.FontColor.ColorType == XLColorType.Color)
            {
                var color = cell.Style.Font.FontColor.Color;
                string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                cellColor = $"color: {hexColor};";
            } else
            {
                var themeColor = workbook.Theme.ResolveThemeColor(cell.Style.Font.FontColor.ThemeColor);
                cellColor = themeColor.Color.ToString();
            }
            
            if (hasHyperLink)
            {
                sb.Append($"<a href=\"{hyperLinkUri}\" target=\"_blank\" style=\"{cellColor}\">{WebUtility.HtmlEncode(content)}</a>");
            }
            else
            {
                sb.Append($"<span style=\"{cellColor}\">{WebUtility.HtmlEncode(content)}</span>");
            }
        }
        else
        {
            StringBuilder richTextContent = new StringBuilder();
            foreach (var richText in cell.GetRichText())
            {
                var styles = new List<string>();

                if (richText.Bold) styles.Add("font-weight: bold");
                if (richText.Italic) styles.Add("font-style: italic");
                if (richText.Underline != XLFontUnderlineValues.None) styles.Add("text-decoration: underline");
                if (richText.Strikethrough) styles.Add("text-decoration: line-through");
                if (richText.FontSize > 0) styles.Add($"font-size: {richText.FontSize}pt");

                if (richText.FontColor != XLColor.NoColor && richText.FontColor.HasValue)
                {
                    if (richText.FontColor.ColorType == XLColorType.Color)
                    {
                        var color = richText.FontColor.Color;
                        string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                        styles.Add($"color: {hexColor}");
                    } 
                    else if (richText.FontColor.ColorType == XLColorType.Theme)
                    {
                        var themeColor = workbook.Theme.ResolveThemeColor(richText.FontColor.ThemeColor);
                        var color = themeColor.Color;
                        string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                        styles.Add($"color: {hexColor}");
                    }
                }
                
                var text = WebUtility.HtmlEncode(richText.Text);
        
                if (styles.Any())
                {
                    richTextContent.Append($"<span style=\"{string.Join(";", styles)}\">{text}</span>");
                }
                else
                {
                    richTextContent.Append(text);
                }
            }

            if (hasHyperLink)
            {
                sb.Append($"<a href=\"{hyperLinkUri}\" target=\"_blank\">{richTextContent}</a>");
            }
            else
            {
                sb.Append(richTextContent);
            }
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    /// <summary>
    /// Extracts a list of parts from a formatted string.
    /// </summary>
    /// <param name="partsListString">String in format: "(PART_NAME, PART_SERIAL_NUMBER), (PART_NAME, PART_SERIAL_NUMBER), ..."</param>
    /// <returns>List of Part objects if successful, null otherwise.</returns>
    /// <remarks>
    /// Uses regex to parse the parts list string and either retrieve existing parts
    /// from the database or create new ones as needed.
    /// </remarks>
    private async Task<List<Part>?> GetPartsListFromString(string partsListString)
    {
        try
        {
            if (string.IsNullOrEmpty(partsListString))
            {
                return null;
            }

            var parts = new List<Part>();
            var regexFilter = PartsListRegex();
            var partStringMatches = regexFilter.Matches(partsListString);

            foreach (System.Text.RegularExpressions.Match match in partStringMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    var partToAdd = new Part
                    {
                        PartName = match.Groups[1].Value.Trim(),
                        PartNumber = match.Groups[2].Value.Trim()
                    };

                    // This will always retrieve a persisted part in the database,
                    // whether it retrieves an existing Part or creates a new one
                    var persistedPart = await GetOrAddPart(partToAdd);

                    if (persistedPart != null)
                    {
                        parts.Add(persistedPart);
                    }
                    else
                    {
                        Log.Warning("Unable to find or create Part when attempting to import a XLSX file. Parts List: {PartsList}", partsListString);
                    }
                }
            }

            return parts.Count > 0 ? parts : null;
        }
        catch (Exception e)
        {
            Log.Warning("Exception Caught when attempting to use a Regex pattern on a Parts List on Work Instruction Import. Message: {Message}", e.Message);
            return null;
        }
    }

    /// <summary>
    /// Retrieves an existing part from the database or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="partToAdd">The part to retrieve or create.</param>
    /// <returns>The persisted part from the database, or null if an error occurs.</returns>
    private async Task<Part?> GetOrAddPart(Part partToAdd)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var part = await context.Parts.FirstOrDefaultAsync(p =>
                p.PartName == partToAdd.PartName &&
                p.PartNumber == partToAdd.PartNumber);

            // If a part does not exist in the database create it here
            // and return with database generated ID
            if (part != null)
            {
                return part;
            }
            
            await context.Parts.AddAsync(partToAdd);
            await context.SaveChangesAsync();
            return partToAdd;
        }
        catch (Exception e)
        {
            Log.Warning("Exception when adding part: {Message}", e.Message);
            return null;
        }
    }

    /// <summary>
    /// Asynchronously retrieves all work instructions, using caching for performance.
    /// </summary>
    /// <returns>List of work instructions.</returns>
    /// <remarks>
    /// Results are cached for 15 minutes to improve performance.
    /// </remarks>
    public async Task<List<WorkInstruction>> GetAllAsync()
    {
        if (_cache.TryGetValue(WORK_INSTRUCTION_CACHE_KEY, out List<WorkInstruction>? cachedWorkInstructionList) &&
            cachedWorkInstructionList != null)
        {
            return cachedWorkInstructionList;
        }
        
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstructions = await context.WorkInstructions
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .ToListAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_CACHE_KEY, workInstructions, TimeSpan.FromMinutes(15));

            return workInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetAllAsync Work Instructions, in WorkInstructionService", e.GetBaseException().ToString());
            return [];
        }
    }
    
    /// <summary>
    /// Retrieves a work instruction by its title.
    /// </summary>
    /// <param name="title">The title of the work instruction to retrieve.</param>
    /// <returns>The work instruction if found, null otherwise.</returns>

    public WorkInstruction? GetByTitle(string title)
    {
        try
        {
            using var context =  _contextFactory.CreateDbContext();
            var workInstruction = context.WorkInstructions
                .FirstOrDefault(w => w.Title == title);

            return workInstruction;
        }
        catch (Exception e)
        {
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByTitle with Title: {title}, in WorkInstructionService", e.GetBaseException().ToString(), title);
            return null;
        }
    }
    /// <summary>
    /// Asynchronously retrieves a work instruction by its ID, including related nodes and parts.
    /// </summary>
    /// <param name="id">The ID of the work instruction to retrieve.</param>
    /// <returns>The work instruction with its related data if found, null otherwise.</returns>

    public async Task<WorkInstruction?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return null;
            }
            
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByIdAsync with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new work instruction in the database.
    /// </summary>
    /// <param name="workInstruction">The work instruction to create.</param>
    /// <returns>True if creation was successful, false otherwise.</returns>
    /// <remarks>
    /// Validates the work instruction before saving and invalidates the cache after successful creation.
    /// </remarks>
    public async Task<bool> Create(WorkInstruction workInstruction)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Validate WorkInstruction
            var workInstructionValidator = new WorkInstructionValidator();
            var validationResult = await workInstructionValidator.ValidateAsync(workInstruction);

            if (!validationResult.IsValid)
            {
                return false;
            }

            workInstruction.IsActive = false;
            await context.WorkInstructions.AddAsync(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            
            Log.Information("Successfully created WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to Create a work instruction, in WorkInstructionService", e.GetBaseException().ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Deletes a Work Instruction from the database if there are no Production log relationships.
    /// </summary>
    /// <param name="id">The ID of the work instruction to delete.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    /// <remarks>
    /// The cache is invalidated after successful deletion.
    /// </remarks>
    public async Task<bool> DeleteByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (workInstruction == null)
            {
                return false;
            }

            if (!await IsEditable(workInstruction))
            {
                return false;
            }
            
            // Remove associated Work Instruction Nodes first
            context.WorkInstructionNodes.RemoveRange(workInstruction.Nodes);

            context.WorkInstructions.Remove(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            
            Log.Information("Successfully deleted WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to Delete a work instruction with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing work instruction in the database.
    /// </summary>
    /// <param name="workInstruction">The work instruction with updated values.</param>
    /// <returns>True if update was successful, false otherwise.</returns>
    /// <remarks>
    /// Verifies the work instruction exists before updating and invalidates the cache after successful update.
    /// </remarks>
    public async Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction)
    {
        // if ID is 0 that means it has NOT been saved to the database
        if (workInstruction.Id == 0)
        {
            return false;
        }
        

        await using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var existingWorkInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .ThenInclude(n => ((PartNode)n).Parts)
                .FirstOrDefaultAsync(w => w.Id == workInstruction.Id);

            if (existingWorkInstruction == null)
            {
                return false;
            }

            context.Entry(existingWorkInstruction).CurrentValues.SetValues(workInstruction);

            // Update any applicable nodes
            foreach (var newNode in workInstruction.Nodes)
            {
                var existingNode = existingWorkInstruction.Nodes.FirstOrDefault(n => n.Id == newNode.Id);

                if (existingNode != null)
                {
                    if (newNode is PartNode && existingNode is PartNode)
                    {
                        context.Entry(existingNode).CurrentValues.SetValues(newNode);
                    }
                    else if (newNode is Step && existingNode is Step)
                    {
                        context.Entry(existingNode).CurrentValues.SetValues(newNode);
                    }
                    else
                    {
                        context.Entry(existingNode).CurrentValues.SetValues(newNode);
                    }
                }
            }
            
            await context.SaveChangesAsync();

            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            Log.Information("Successfully updated WorkInstruction with ID: {workInstructionID}",
                workInstruction.Id);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to update work instruction {Id}", workInstruction.Id);
            return false;
        }
    }

    /// <summary>
    /// Regex pattern for parsing parts list strings in the format "(PART_NAME, PART_NUMBER)"
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"\(([^,)]+)(?:,\s*)?([^)]+)\)")]
    private static partial System.Text.RegularExpressions.Regex PartsListRegex();
    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex RemoveHTMLRegex();

}