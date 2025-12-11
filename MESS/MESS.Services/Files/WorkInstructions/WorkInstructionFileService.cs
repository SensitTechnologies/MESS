using System.Net;
using System.Text;
using ClosedXML.Excel;
using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.CRUD.PartDefinitions;
using MESS.Services.CRUD.Products;
using MESS.Services.CRUD.WorkInstructions;
using MESS.Services.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace MESS.Services.Files.WorkInstructions;

/// <inheritdoc />
public partial class WorkInstructionFileService : IWorkInstructionFileService
{
    // The following attributes define the current expected XLSX structure as of 7/1/2025
    private const string PRODUCT_NAME_CELL = "B1";
    private const string INSTRUCTION_TITLE_CELL = "B2";
    private const string VERSION_CELL = "B3";
    private const string SHOULD_GENERATE_QR_CODE_CELL = "B4";
    private const string PART_PRODUCED_IS_SERIALIZED = "B5";
    private const string ORIGINAL_ID_CELL = "B6";
    
    // Using int values here since there is an indeterminate amount of steps per Work Instruction
    private const int STEP_START_ROW = 9;
    private const int PART_COLUMN = 1;
    private const int STEP_NAME_COLUMN = 2;
    private const int STEP_BODY_COLUMN = 3;
    private const int STEP_PRIMARY_MEDIA_COLUMN = 4;
    private const int STEP_DETAILED_BODY_COLUMN = 5;
    private const int STEP_SECONDARY_MEDIA_COLUMN = 6;
    
    // Currently, this is set to 1GB
    private const int SPREADSHEET_SIZE_LIMIT = 1024 * 1024 * 1024;
    
    private const string WORK_INSTRUCTION_IMAGES_DIRECTORY = "WorkInstructionImages";
    
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    private readonly IWorkInstructionService _workInstructionService;
    private readonly IProductService _productService;
    private readonly IPartDefinitionService _partDefinitionService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionFileService"/> class.
    /// </summary>
    /// <param name="contextFactory">
    /// Factory used to create <see cref="ApplicationContext"/> instances for database operations.
    /// </param>
    /// <param name="webHostEnvironment">
    /// Provides access to the hosting environment, including the web root path for reading and writing media files.
    /// </param>
    /// <param name="workInstructionService">
    /// Service responsible for creating and managing <see cref="WorkInstruction"/> entities.
    /// </param>
    /// <param name="productService">
    /// Service used to retrieve <see cref="Product"/> entities for associating with imported work instructions.
    /// </param>
    /// <param name="partDefinitionService">
    /// Service for resolving <see cref="PartDefinition"/> entities referenced by part nodes in a work instruction.
    /// </param>
    /// <remarks>
    /// This constructor injects all dependencies required for reading, writing, and mapping
    /// work instruction data between Excel and the database.
    /// </remarks>
    public WorkInstructionFileService(IDbContextFactory<ApplicationContext> contextFactory, IWebHostEnvironment webHostEnvironment, 
        IWorkInstructionService workInstructionService, IProductService productService, IPartDefinitionService partDefinitionService)
    {
        _contextFactory = contextFactory;
        _webHostEnvironment = webHostEnvironment;
        _workInstructionService = workInstructionService;
        _productService = productService;
        _partDefinitionService = partDefinitionService;
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
            worksheet.Cell(SHOULD_GENERATE_QR_CODE_CELL).Value = workInstructionToExport.ShouldGenerateQrCode;
            worksheet.Cell(PART_PRODUCED_IS_SERIALIZED).Value = workInstructionToExport.PartProducedIsSerialized;
            worksheet.Cell(ORIGINAL_ID_CELL).Value = workInstructionToExport.OriginalId ?? workInstructionToExport.Id;
            
            // Since a work instruction can be associated with multiple Products it is stored as a comma seperated list
            if (workInstructionToExport.Products.Count > 0)
            {
                var productNames = string.Join(", ", workInstructionToExport.Products.Select(p => p.PartDefinition.Name));
                worksheet.Cell(PRODUCT_NAME_CELL).Value = productNames;
            }
            
            // Setup headers
            worksheet.Cell("A1").Value = "Product";
            worksheet.Cell("A2").Value = "Instruction";
            worksheet.Cell("A3").Value = "Version";
            worksheet.Cell("A4").Value = "QR Code";
            worksheet.Cell("A5").Value = "Serial Number";
            worksheet.Cell("A6").Value = "Version History ID";
            worksheet.Range("A1:A6").Style.Font.Bold = true;
            
            // Setup Header Comments
            worksheet.Cell("C1").Value = "Name of the product that this work instruction contributes to.";
            worksheet.Cell("C2").Value = "Title of this set of steps to produce the product.";
            worksheet.Cell("C3").Value = "Version code for this document.";
            worksheet.Cell("C4").Value = "True if the work instruction produces a printed QR code on completion. " +
                                         "False otherwise.";
            worksheet.Cell("C5").Value = "True if the work instruction allows the operator to input a product serial " +
                                         "number after the last step.  False otherwise.";
            worksheet.Cell("C6").Value = "Optional integer field that can be used to link this work instruction to an " +
                                         "already existing version history chain. Leave blank if unsure.";
            
            //Styling the comments
            var commentRange = worksheet.Range("C1:C6");
            commentRange.Style.Font.FontColor = XLColor.Gray;
            commentRange.Style.Font.Italic = true;
            
            // Step Header row. With bold.
            var currentRow = STEP_START_ROW;
            worksheet.Cell(currentRow, PART_COLUMN).Value = "Parts";
            worksheet.Cell(currentRow, STEP_NAME_COLUMN).Value = "Name";
            worksheet.Cell(currentRow, STEP_BODY_COLUMN).Value = "Body";
            worksheet.Cell(currentRow, STEP_PRIMARY_MEDIA_COLUMN).Value = "Primary Media";
            worksheet.Cell(currentRow, STEP_DETAILED_BODY_COLUMN).Value = "Detailed Body";
            worksheet.Cell(currentRow, STEP_SECONDARY_MEDIA_COLUMN).Value = "Secondary Media";

            // Make them bold
            worksheet.Range(currentRow, PART_COLUMN, currentRow, STEP_SECONDARY_MEDIA_COLUMN)
                .Style.Font.Bold = true;
            
            var orderedNodes = workInstructionToExport.Nodes
                .OrderBy(n => n.Position)
                .ToList();

            currentRow++;
            
            var index = 0;

            while (index < orderedNodes.Count)
            {
                var node = orderedNodes[index];

                switch (node)
                {
                    case PartNode:
                    {
                        // Collect all consecutive part nodes
                        var partGroup = new List<PartNode>();
                        while (index < orderedNodes.Count && orderedNodes[index] is PartNode pn)
                        {
                            partGroup.Add(pn);
                            index++;
                        }

                        // Combine all part definitions into a single string
                        var partStrings = partGroup
                            .Select(pn => $"({pn.PartDefinition.Number}, {pn.PartDefinition.Name})");

                        worksheet.Cell(currentRow, PART_COLUMN).Value = string.Join(", ", partStrings);

                        // Leave other columns blank automatically
                        currentRow++;
                        break;
                    }
                    case Step step:
                        worksheet.Cell(currentRow, PART_COLUMN).Value = ""; // no part

                        worksheet.Cell(currentRow, STEP_NAME_COLUMN).Value = step.Name;

                        ApplyFormattingToCells(worksheet.Cell(currentRow, STEP_BODY_COLUMN), step.Body);
                        ApplyFormattingToCells(worksheet.Cell(currentRow, STEP_DETAILED_BODY_COLUMN), step.DetailedBody);

                        InsertImagesIntoCell(worksheet, currentRow, STEP_PRIMARY_MEDIA_COLUMN, step.PrimaryMedia);
                        InsertImagesIntoCell(worksheet, currentRow, STEP_SECONDARY_MEDIA_COLUMN, step.SecondaryMedia);

                        currentRow++;
                        index++;
                        break;
                    default:
                        // For any unrecognized node type
                        index++;
                        break;
                }
            }
            
            //Set Column Widths
            worksheet.Column(PART_COLUMN).Width = 30;
            worksheet.Column(STEP_NAME_COLUMN).Width = 30;
            worksheet.Column(STEP_BODY_COLUMN).Width = 40;
            worksheet.Column(STEP_PRIMARY_MEDIA_COLUMN).Width = 40;
            worksheet.Column(STEP_DETAILED_BODY_COLUMN).Width = 40;
            worksheet.Column(STEP_SECONDARY_MEDIA_COLUMN).Width = 40;
            
            // Enable wrapping on data rows starting from STEP_START_ROW downward
            for (int col = PART_COLUMN; col <= STEP_SECONDARY_MEDIA_COLUMN; col++)
            {
                worksheet.Column(col).Style.Alignment.WrapText = true;
            }
            
            // Disable wrapping on header rows (e.g., rows 1 to STEP_START_ROW - 1)
            for (int row = 1; row < STEP_START_ROW; row++)
            {
                for (int col = PART_COLUMN; col <= STEP_SECONDARY_MEDIA_COLUMN; col++)
                {
                    worksheet.Cell(row, col).Style.Alignment.WrapText = false;
                }
            }
            
            // Determine the data range starting from your header row (STEP_START_ROW)
            // and covering all columns used (PART_COLUMN to STEP_SECONDARY_MEDIA_COLUMN)
            var lastRowUsed = worksheet.LastRowUsed();
            if (lastRowUsed != null)
            {
                int lastRowNumber = lastRowUsed.RowNumber();

                // Define the range including headers and data rows
                var tableRange = worksheet.Range(
                    worksheet.Cell(STEP_START_ROW, PART_COLUMN),
                    worksheet.Cell(lastRowNumber, STEP_SECONDARY_MEDIA_COLUMN)
                );

                // Create a table on that range
                var table = tableRange.CreateTable();

                // Set a built-in table style with alternating row colors
                table.Theme = XLTableTheme.TableStyleMedium2;
            }
            
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
            Log.Warning(e, "Unable to export work instruction: {workInstructionTitle} to xlsx. Exception type: {exceptionType}", 
                workInstructionToExport.Title, 
                e.GetType());
            return null;
        }
    }
    
    private class TextRun
    {
        public string Text { get; set; } = "";
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string? FontColor { get; set; }
        public double? FontSize { get; set; }
        public bool IsLink { get; set; }
        public string? LinkHref { get; set; }
        public string? FontFamily { get; set; }
        public bool IsStrikethrough { get; set; }
        public string? BackgroundColor { get; set; }
    }
    
    /// <summary>
    /// Applies rich text formatting to an Excel cell based on the provided HTML content.
    /// This includes styles such as bold, italic, underline, strikethrough, font size, 
    /// font family, font color, background color, and link styling.
    /// </summary>
    /// <param name="cell">The target Excel cell to apply formatting to.</param>
    /// <param name="htmlContent">The HTML content containing the text and inline styles to parse and apply.</param>
    private void ApplyFormattingToCells(IXLCell cell, string? htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            return;
        }

        htmlContent = htmlContent.Trim();

        // Remove div wrapper
        if (htmlContent.StartsWith("<div>") && htmlContent.EndsWith("</div>"))
        {
            htmlContent = htmlContent.Substring(5, htmlContent.Length - 11);
        }

        // Decode HTML entities
        htmlContent = WebUtility.HtmlDecode(htmlContent);

        var tokens = TokenizeSimpleHtml(htmlContent);

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token.Text)) continue;

            var rt = cell.GetRichText().AddText(token.Text);
            rt.Bold = token.IsBold;
            rt.Italic = token.IsItalic;
            rt.Underline = token.IsUnderline ? XLFontUnderlineValues.Single : XLFontUnderlineValues.None;
            rt.Strikethrough = token.IsStrikethrough;

            if (!string.IsNullOrWhiteSpace(token.FontColor))
            {
                try { rt.FontColor = XLColor.FromHtml(token.FontColor); }
                catch
                {
                    // ignored
                }
            }

            if (!string.IsNullOrWhiteSpace(token.FontFamily))
            {
                rt.FontName = token.FontFamily;
            }

            if (token.FontSize.HasValue)
            {
                rt.FontSize = token.FontSize.Value;
            }

            if (token.IsLink)
            {
                rt.Underline = XLFontUnderlineValues.Single;
                rt.FontColor = XLColor.FromHtml("#0000EE"); // Typical link blue
            }

            // Note: Background color can't be set on rich text runs in ClosedXML.
            // You can collect it here for whole-cell fallback if you want:
            //   if (!string.IsNullOrWhiteSpace(token.BackgroundColor)) { ... }
        }

    }
    
    private void ApplyTagToRun(TextRun run, string tag, bool enable, Dictionary<string,string>? attributes = null)
    {
        switch (tag)
        {
            case "b":
                run.IsBold = enable;
                break;
            case "i":
                run.IsItalic = enable;
                break;
            case "u":
                run.IsUnderline = enable;
                break;
            case "a":
                run.IsLink = enable;
                if (enable && attributes != null && attributes.TryGetValue("href", out var href))
                {
                    run.LinkHref = href;
                }
                break;
            case "span":
                if (enable && attributes != null)
                {
                    if (attributes.TryGetValue("style", out var styleString))
                    {
                        ApplySpanStyles(run, styleString);
                    }
                }
                break;
        }
    }
    
    private void ApplySpanStyles(TextRun run, string styleString)
    {
        var styles = styleString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in styles)
        {
            var kvp = s.Split(':', 2);
            if (kvp.Length != 2) continue;
            var key = kvp[0].Trim().ToLower();
            var value = kvp[1].Trim();

            switch (key)
            {
                case "color":
                    run.FontColor = value;
                    break;
                case "background-color":
                    run.BackgroundColor = value;
                    break;
                case "font-size":
                    if (value.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
                    {
                        value = value[..^2].Trim();
                    }
                    if (double.TryParse(value, out var size))
                    {
                        run.FontSize = size;
                    }
                    break;
                case "font-family":
                    run.FontFamily = value;
                    break;
                case "text-decoration":
                    if (value.Contains("line-through", StringComparison.OrdinalIgnoreCase))
                    {
                        run.IsStrikethrough = true;
                    }
                    if (value.Contains("underline", StringComparison.OrdinalIgnoreCase))
                    {
                        run.IsUnderline = true;
                    }
                    break;
            }
        }
    }
    
    private List<TextRun> TokenizeSimpleHtml(string html)
    {
        var runs = new List<TextRun>();
        var current = new TextRun();
        var stack = new Stack<(string Tag, Dictionary<string, string>? Attributes)>();

        var i = 0;
        while (i < html.Length)
        {
            if (html[i] == '<')
            {
                var end = html.IndexOf('>', i);
                if (end == -1)
                {
                    current.Text += html.Substring(i);
                    break;
                }

                var tagContent = html.Substring(i + 1, end - i - 1).Trim();

                if (!tagContent.StartsWith("/"))
                {
                    // Opening tag
                    var (tag, attributes) = ParseTagAndAttributes(tagContent);
                    stack.Push((tag, attributes));
                    ApplyTagToRun(current, tag, true, attributes);
                }
                else
                {
                    // Closing tag
                    var closing = tagContent.Substring(1).Trim().ToLower();
                    if (stack.Count > 0 && stack.Peek().Tag == closing)
                    {
                        stack.Pop();
                        ApplyTagToRun(current, closing, false);
                    }
                }

                i = end + 1;
            }
            else
            {
                var nextTag = html.IndexOf('<', i);
                if (nextTag == -1) nextTag = html.Length;

                current.Text += html.Substring(i, nextTag - i);
                i = nextTag;
            }

            if (!string.IsNullOrEmpty(current.Text))
            {
                runs.Add(current);
                current = new TextRun
                {
                    IsBold = current.IsBold,
                    IsItalic = current.IsItalic,
                    IsUnderline = current.IsUnderline,
                    FontColor = current.FontColor,
                    FontSize = current.FontSize,
                    IsLink = current.IsLink,
                    LinkHref = current.LinkHref
                };
            }
        }

        return runs;
    }
    
    private (string Tag, Dictionary<string,string> Attributes) ParseTagAndAttributes(string tagContent)
    {
        var spaceIndex = tagContent.IndexOf(' ');
        if (spaceIndex == -1)
        {
            return (tagContent.ToLower(), new Dictionary<string, string>());
        }

        var tagName = tagContent.Substring(0, spaceIndex).ToLower();
        var attrString = tagContent.Substring(spaceIndex + 1);

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var regex = new System.Text.RegularExpressions.Regex(@"(\w+)\s*=\s*(""[^""]*""|'[^']*'|[^\s]*)");
        foreach (System.Text.RegularExpressions.Match m in regex.Matches(attrString))
        {
            var key = m.Groups[1].Value;
            var val = m.Groups[2].Value.Trim('"', '\'');
            attributes[key] = val;
        }

        return (tagName, attributes);
    }
    
    private void InsertImagesIntoCell(IXLWorksheet worksheet, int row, int column, List<string> mediaPaths)
    {
        if (mediaPaths.Count == 0)
            return;

        // Settings
        const int targetDisplayHeightPx = 100; // All images scaled to this height
        const int cellEdgePaddingPx = 5;       // Padding from cell edges
        const int imageSpacingPx = 5;          // Spacing between images horizontally and vertically

        // Determine wrap width *more conservatively*
        double excelColumnWidthUnits = worksheet.Column(column).Width;
        double approxWrapWidthPx = (excelColumnWidthUnits * 7);
        
        if (approxWrapWidthPx <= 0) approxWrapWidthPx = 500;

        int offsetX = cellEdgePaddingPx;
        int offsetY = cellEdgePaddingPx;
        int lineMaxHeight = 0;
        bool anyImageInserted = false;

        foreach (var media in mediaPaths)
        {
            var normalizedMediaPath = media.Replace('\\', Path.DirectorySeparatorChar);
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, normalizedMediaPath.TrimStart(Path.DirectorySeparatorChar));

            if (!File.Exists(imagePath))
                continue;

            using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var image = Image.Load(stream);
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Scale to target height
            var scale = (double)targetDisplayHeightPx / originalHeight;
            var scaledWidth = (int)(originalWidth * scale);

            // Check if image fits on current line
            if (offsetX + scaledWidth + cellEdgePaddingPx > approxWrapWidthPx)
            {
                // Wrap to next line
                offsetX = cellEdgePaddingPx;
                offsetY += lineMaxHeight + imageSpacingPx;
                lineMaxHeight = 0;
            }

            // Save image to memory
            using var rawStream = new MemoryStream();
            image.Save(rawStream, new PngEncoder());
            rawStream.Position = 0;

            // Add to Excel
            worksheet.AddPicture(rawStream)
                .MoveTo(worksheet.Cell(row, column), new System.Drawing.Point(offsetX, offsetY))
                .WithSize(scaledWidth, targetDisplayHeightPx);

            offsetX += scaledWidth + imageSpacingPx;
            lineMaxHeight = Math.Max(lineMaxHeight, targetDisplayHeightPx);
            anyImageInserted = true;
        }

        // Adjust row height
        if (anyImageInserted)
        {
            double totalHeightPx = offsetY + lineMaxHeight + cellEdgePaddingPx;
            worksheet.Row(row).Height = totalHeightPx * 0.75; // Excel points ≈ 0.75 px
        }
    }
    
    /// <inheritdoc />
    public async Task<WorkInstructionImportResult> ImportFromXlsx(IBrowserFile file)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            using var memoryStream = new MemoryStream();
            await file.OpenReadStream(maxAllowedSize: SPREADSHEET_SIZE_LIMIT).CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheet(1);
            
            var versionString = worksheet.Cell(VERSION_CELL).GetString();
            var workInstructionTitle = worksheet.Cell(INSTRUCTION_TITLE_CELL).GetString();
            
            var shouldGenerateQrCode = worksheet.Cell(SHOULD_GENERATE_QR_CODE_CELL).GetValue<bool>();
            var partProducedIsSerialized = worksheet.Cell(PART_PRODUCED_IS_SERIALIZED).GetValue<bool>();
            
            // Retrieve Product and assign relationship
            var productString = worksheet.Cell(PRODUCT_NAME_CELL).GetString();
            var product = await _productService.GetByTitleAsync(productString);

            if (product == null)
            {
                Log.Information("Product not found. Cannot create Work Instruction");
                return WorkInstructionImportResult.NoProductFound(file.Name, productString);
            }
            
            var originalIdString = worksheet.Cell(ORIGINAL_ID_CELL).GetString()?.Trim();
            int? originalId = null;

            if (int.TryParse(originalIdString, out var parsedId))
            {
                originalId = parsedId;
            }
            
            // Prepare to query for duplicates
            IQueryable<WorkInstruction> versionQuery;

            if (originalId.HasValue)
            {
                versionQuery = context.WorkInstructions
                    .Where(w => w.OriginalId == originalId.Value || w.Id == originalId.Value);
            }
            else
            {
                versionQuery = context.WorkInstructions
                    .Where(w => w.Title == workInstructionTitle && w.Products.Any(p => p.Id == product.Id));
            }

            // Duplicate check
            var duplicateExists = await versionQuery
                .Where(w => w.Version == versionString)
                .AnyAsync();

            if (duplicateExists)
            {
                Log.Information("Unable to import Work Instruction. Duplicate version found for Title: {Title}, Version: {Version}.", workInstructionTitle, versionString);
                return WorkInstructionImportResult.DuplicateWorkInstructionFound(file.Name, workInstructionTitle, versionString);
            }
            
            // Building the work instruction object with gathered data
            var workInstruction = new WorkInstruction
            {
                Title = workInstructionTitle,
                Version = versionString,
                Products = [],
                Nodes = [],
                ShouldGenerateQrCode = shouldGenerateQrCode,
                PartProducedIsSerialized = partProducedIsSerialized,
                IsLatest = true
            };
            
            // Handle lineage
            if (originalId.HasValue)
            {
                var existingChain = await context.WorkInstructions
                    .Where(w => w.OriginalId == originalId.Value || w.Id == originalId.Value)
                    .ToListAsync();

                if (existingChain.Any())
                {
                    Log.Information("Found {Count} existing work instructions with OriginalId {OriginalId} to mark as not latest", existingChain.Count, originalId.Value);

                    foreach (var old in existingChain)
                    {
                        old.IsLatest = false;
                    }
                    
                    Log.Information("Saving changes to demote old versions");
                    await context.SaveChangesAsync();
                    Log.Information("Demotion save completed");

                    workInstruction.OriginalId = originalId.Value;
                }
            }
            
            workInstruction.Products.Add(product);

            // Create all steps within instruction and add part nodes where logical
            // Start from row 9 (assuming header row is 8)
            var currentRow = STEP_START_ROW;
            var position = 0;

            while (true)
            {
                var partCell = worksheet.Cell(currentRow, PART_COLUMN).GetString()?.Trim();
                var stepBodyCell = worksheet.Cell(currentRow, STEP_BODY_COLUMN).GetString()?.Trim();

                var isPartRow = !string.IsNullOrWhiteSpace(partCell);
                var isStepRow = !string.IsNullOrWhiteSpace(stepBodyCell);

                if (!isPartRow && !isStepRow)
                {
                    // Stop on first fully empty row
                    break;
                }

                if (isPartRow)
                {
                    // Parse part list string, e.g. "(12345, Widget A), (67890, Widget B)"
                    var partDefinitions = await GetPartsListFromString(partCell ?? string.Empty);

                    if (partDefinitions != null && partDefinitions.Any())
                    {
                        // For each part definition, create a separate PartNode
                        foreach (var partDefinition in partDefinitions)
                        {
                            var partNode = new PartNode
                            {
                                NodeType = WorkInstructionNodeType.Part,
                                Position = position,
                                PartDefinition = partDefinition
                            };

                            workInstruction.Nodes.Add(partNode);

                            Log.Information(
                                "Imported PartNode at position {Position} with PartDefinition ID {Id}, Name {Name}, Number {Number}",
                                position,
                                partDefinition.Id,
                                partDefinition.Name,
                                partDefinition.Number
                            );
                        }
                    }
                }
                else if (isStepRow)
                {
                    var step = new Step
                    {
                        Name = System.Text.RegularExpressions.Regex.Replace(
                            string.Join(" ", worksheet.Cell(currentRow, STEP_NAME_COLUMN)
                                .GetRichText()
                                .Select(r => r.Text.Trim())),
                            @"\s+", " ").Trim(),
                        Body = ProcessCellText(worksheet.Cell(currentRow, STEP_BODY_COLUMN), workbook),
                        DetailedBody = ProcessCellText(worksheet.Cell(currentRow, STEP_DETAILED_BODY_COLUMN), workbook),
                        NodeType = WorkInstructionNodeType.Step,
                        Position = position
                    };

                    await ProcessStepMedia(worksheet, step, currentRow);

                    workInstruction.Nodes.Add(step);
                    Log.Information("Imported Step at position {Position} with title {Title}", position, step.Body);
                }

                position++;
                currentRow++;
            }
            
            var fileName = file.Name;

            if (await _workInstructionService.Create(workInstruction))
            {
                Log.Information("Successfully imported WorkInstruction from Excel: {title}", workInstruction.Title);
                // Not invalidating cache here, since it gets invalidated on successful creation
                return WorkInstructionImportResult.Success(fileName, workInstruction);
            }

            return WorkInstructionImportResult.Failure(fileName);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to import WorkInstruction from uploaded Excel file");
            var fileName = file.Name;
            var errorResult = WorkInstructionImportResult.Failure(fileName);

            var error = new ImportError
            {
                File = string.IsNullOrEmpty(fileName) ? fileName : "Unknown",
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
                default:
                    error.ErrorType = $"Exception Thrown: {e.GetType()}";
                    error.Message = "Please contact an admin.";
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
            Log.Warning("Exception thrown when attempting to process Step media for Workbook: {workbookName}. Exception: {ExceptionMessage}", worksheet.ToString(), e.ToString());
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
    private static string ProcessCellText(IXLCell cell, XLWorkbook workbook)
    {
        try
        {
            var sb = new StringBuilder();

            var hasHyperLink = cell.HasHyperlink;
            string? hyperLinkUri = null;
            
            
            if (hasHyperLink)
            {
                var hyperLink = cell.GetHyperlink();
                hyperLinkUri = hyperLink.ExternalAddress.AbsoluteUri;
            }

            if (!cell.HasRichText)
            {
                var content = cell.GetString();

                string cellColor;
                
                if (cell.Style.Font.FontColor.ColorType == XLColorType.Color)
                {
                    var color = cell.Style.Font.FontColor.Color;
                    var hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
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
                var richTextContent = new StringBuilder();
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
                            if (!string.Equals(color.Name, "Black"))
                            {
                                string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                                styles.Add($"color: {hexColor}");
                            }
                        }
                    }
                    
                    var text = WebUtility.HtmlEncode(richText.Text);
            
                    if (styles.Count > 0)
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
            
            return sb.ToString();
        }
        catch (Exception e)
        {
            Log.Warning("Unable to ProcessCellText while trying to process a work instruction. Cell Address: {cellAddress}. Exception: {Exception}", cell.Address.ToString(), e.ToString());
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Extracts a list of part definitions from a formatted string.
    /// </summary>
    /// <param name="partsListString">
    /// String in format: "(PART_NUMBER, PART_NAME), (PART_NUMBER, PART_NAME), ..."
    /// </param>
    /// <returns>
    /// List of <see cref="PartDefinition"/> objects if successful; null otherwise.
    /// </returns>
    /// <remarks>
    /// Uses regex to parse the parts list string and either retrieve existing part definitions
    /// from the database or create new ones as needed.
    /// </remarks>
    private async Task<List<PartDefinition>?> GetPartsListFromString(string partsListString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partsListString))
                return null;

            var partDefinitions = new List<PartDefinition>();
            var regex = PartsListRegex();
            var matches = regex.Matches(partsListString);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // Expecting format: (NUMBER, NAME)
                if (match.Groups.Count >= 3)
                {
                    var partToAdd = new PartDefinition
                    {
                        Number = match.Groups[1].Value.Trim(),
                        Name = match.Groups[2].Value.Trim()
                    };

                    // Persist or retrieve existing part definition
                    var persistedPart = await _partDefinitionService.GetOrAddPartAsync(partToAdd);

                    if (persistedPart != null)
                    {
                        partDefinitions.Add(persistedPart);
                    }
                    else
                    {
                        Log.Warning("Unable to find or create PartDefinition when importing XLSX. Parts List: {PartsList}", partsListString);
                    }
                }
            }

            return partDefinitions.Count > 0 ? partDefinitions : null;
        }
        catch (Exception e)
        {
            Log.Warning("Exception while parsing parts list string during Work Instruction import. Exception: {Exception}", e);
            return null;
        }
    }
    
    /// <summary>
    /// Regex pattern for parsing parts list strings in the format "(PART_NUMBER, PART_NAME)"
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"\(([^,]+),\s*([^)]+)\)")]
    private static partial System.Text.RegularExpressions.Regex PartsListRegex();
}