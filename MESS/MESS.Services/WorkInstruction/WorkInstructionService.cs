using ClosedXML.Excel;
using MESS.Data.Context;
using MESS.Data.DTO;
using MESS.Services.Product;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Net;
using System.Text;
using MESS.Services.ProductionLogServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace MESS.Services.WorkInstruction;
using MESS.Data.Models;
/// <inheritdoc />
public partial class WorkInstructionService : IWorkInstructionService
{
    private readonly IProductService _productService;
    private readonly IProductionLogService _productionLogService;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    // The following attributes define the current expected XLSX structure as of 7/1/2025
    private const string PRODUCT_NAME_CELL = "B1";
    private const string INSTRUCTION_TITLE_CELL = "B2";
    private const string VERSION_CELL = "B3";
    private const string SHOULD_GENERATE_QR_CODE_CELL = "B4";
    private const string COLLECTS_PRODUCT_SERIAL_NUMBER_CELL = "B5";
    private const string ORIGINAL_ID_CELL = "B6";
    
    // Using int values here since there is an indeterminate amount of steps per Work Instruction
    private const int STEP_START_ROW = 9;
    private const int PART_COLUMN = 1;
    private const int STEP_NAME_COLUMN = 2;
    private const int STEP_BODY_COLUMN = 3;
    private const int STEP_PRIMARY_MEDIA_COLUMN = 4;
    private const int STEP_DETAILED_BODY_COLUMN = 5;
    private const int STEP_SECONDARY_MEDIA_COLUMN = 6;

    private const string WORK_INSTRUCTION_IMAGES_DIRECTORY = "WorkInstructionImages";
    const string WORK_INSTRUCTION_CACHE_KEY = "AllWorkInstructions";

    // Currently, this is set to 1GB
    private const int SPREADSHEET_SIZE_LIMIT = 1024 * 1024 * 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionService"/> class.
    /// </summary>
    /// <param name="productService">The service for managing product-related operations.</param>
    /// <param name="productionLogService">The service for managing product-related operations.</param>
    /// <param name="cache">The memory cache for caching work instructions.</param>
    /// <param name="webHostEnvironment">The hosting environment for accessing web root paths.</param>
    /// <param name="contextFactory">The factory for creating database contexts.</param>
    public WorkInstructionService(IProductService productService, IProductionLogService productionLogService, IMemoryCache cache, IWebHostEnvironment webHostEnvironment, IDbContextFactory<ApplicationContext> contextFactory)
    {
        _productService = productService;
        _productionLogService = productionLogService;
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
            worksheet.Cell(SHOULD_GENERATE_QR_CODE_CELL).Value = workInstructionToExport.ShouldGenerateQrCode;
            worksheet.Cell(COLLECTS_PRODUCT_SERIAL_NUMBER_CELL).Value = workInstructionToExport.CollectsProductSerialNumber;
            worksheet.Cell(ORIGINAL_ID_CELL).Value = workInstructionToExport.OriginalId ?? workInstructionToExport.Id;
            
            // Since a work instruction can be associated with multiple Products it is stored as a comma seperated list
            if (workInstructionToExport.Products.Count > 0)
            {
                var productNames = string.Join(", ", workInstructionToExport.Products.Select(p => p.Name));
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
        
            foreach (var node in orderedNodes)
            {
                if (node is PartNode partNode)
                {
                    // Write parts string to column A (PART_COLUMN)
                    var partStrings = partNode.Parts.Select(p => $"({p.PartNumber}, {p.PartName})");
                    worksheet.Cell(currentRow, PART_COLUMN).Value = string.Join(", ", partStrings);

                    // Leave other columns blank automatically
                }
                else if (node is Step step)
                {
                    worksheet.Cell(currentRow, PART_COLUMN).Value = ""; // no part
                    
                    worksheet.Cell(currentRow, STEP_NAME_COLUMN).Value = step.Name;

                    //Applying Text
                    ApplyFormattingToCells(worksheet.Cell(currentRow, STEP_BODY_COLUMN), step.Body);
                    ApplyFormattingToCells(worksheet.Cell(currentRow, STEP_DETAILED_BODY_COLUMN), step.DetailedBody);
                    
                    //Inserting Images
                    InsertImagesIntoCell(
                        worksheet,
                        currentRow,
                        STEP_PRIMARY_MEDIA_COLUMN,
                        step.PrimaryMedia
                    );

                    InsertImagesIntoCell(
                        worksheet,
                        currentRow,
                        STEP_SECONDARY_MEDIA_COLUMN,
                        step.SecondaryMedia
                    );
                }

                currentRow++;
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
            Log.Warning("Unable to export work instruction: {workInstructionTitle} to xlsx. Exception type: {exceptionType}", workInstructionToExport.Title, e.GetType());
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
        htmlContent = System.Net.WebUtility.HtmlDecode(htmlContent);

        var tokens = TokenizeSimpleHtml(htmlContent);

        var richText = cell.GetRichText();

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
                catch { }
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
        if (mediaPaths == null || mediaPaths.Count == 0)
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

            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            using (var image = Image.Load(stream))
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                // Scale to target height
                double scale = (double)targetDisplayHeightPx / originalHeight;
                int scaledWidth = (int)(originalWidth * scale);
                int scaledHeight = targetDisplayHeightPx;

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
                    .WithSize(scaledWidth, scaledHeight);

                offsetX += scaledWidth + imageSpacingPx;
                lineMaxHeight = Math.Max(lineMaxHeight, scaledHeight);
                anyImageInserted = true;
            }
        }

        // Adjust row height
        if (anyImageInserted)
        {
            double totalHeightPx = offsetY + lineMaxHeight + cellEdgePaddingPx;
            worksheet.Row(row).Height = totalHeightPx * 0.75; // Excel points ≈ 0.75 px
        }
    }
    
    /// <summary>
    /// Removes HTML tags from a string.
    /// </summary>
    private string StripHtmlTags(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        return RemoveHtmlRegex().Replace(input, string.Empty);
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
            Log.Warning("Unable to determine if WorkInstruction: {WorkInstructionTitle} is editable. Exception Type: {ExceptionType}", workInstruction.Title, e.GetType());
            return false;
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
            var collectsProductSerialNumber = worksheet.Cell(COLLECTS_PRODUCT_SERIAL_NUMBER_CELL).GetValue<bool>();
            
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
                CollectsProductSerialNumber = collectsProductSerialNumber,
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
                    // Create a PartNode from the part string in column A
                    if (!string.IsNullOrWhiteSpace(partCell))
                    {
                        var parts = await GetPartsListFromString(partCell);

                        if (parts != null && parts.Any())
                        {
                            var partNode = new PartNode
                            {
                                NodeType = WorkInstructionNodeType.Part,
                                Position = position
                            };

                            partNode.Parts.AddRange(parts);
                            workInstruction.Nodes.Add(partNode);

                            Log.Information("Imported PartNode at position {Position} with parts count {Count}", position, parts.Count);
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

            if (await Create(workInstruction))
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

    /// <inheritdoc />
    public async Task<bool> IsUnique(WorkInstruction workInstruction)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var title = workInstruction.Title;
            var version = workInstruction.Version;

            var isUniqueCount = await context.WorkInstructions
                .CountAsync(w => w.Title == title && w.Version == version);

            if (isUniqueCount >= 1)
            {
                return false;
            }
            
            if (isUniqueCount is 0)
            {
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to determine if WorkInstruction with Title: {Title}, and Version: {Version} is unique. Exception thrown: {Exception}", workInstruction.Title, workInstruction.Version, e.ToString());
            return false;
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

                var cellColor = "";
                
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
    /// Extracts a list of parts from a formatted string.
    /// </summary>
    /// <param name="partsListString">String in format: "(PART_SERIAL_NUMBER, PART_NAME), (PART_SERIAL_NUMBER, PART_NAME), ..."</param>
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
                        PartNumber = match.Groups[1].Value.Trim(),
                        PartName = match.Groups[2].Value.Trim()
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
            Log.Warning("Exception Caught when attempting to use a Regex pattern on a Parts List on Work Instruction Import. Exception: {Exception}", e.ToString());
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
            var existingPart = await context.Parts.FirstOrDefaultAsync(p =>
                p.PartName == partToAdd.PartName &&
                p.PartNumber == partToAdd.PartNumber);


            if (existingPart != null)
            {
                // Detach from context so that EF Core does not attempt to re-add it to the database
                context.Entry(existingPart).State = EntityState.Unchanged;
                Log.Information("GetOrAddPart: Successfully found pre-existing Part with ID: {ExistingPartID}", existingPart.Id);
                return existingPart;
            }
            
            // If a part does not exist in the database create it here
            // and return with database generated ID
            await context.Parts.AddAsync(partToAdd);
            await context.SaveChangesAsync();
            Log.Information("Successfully created a new Part with Name: {PartName}, and Number: {PartNumber}", partToAdd.PartName, partToAdd.PartNumber);
            return partToAdd;
        }
        catch (Exception e)
        {
            Log.Warning("Exception when adding part: {Exception}", e.ToString());
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
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .ToListAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_CACHE_KEY, workInstructions, TimeSpan.FromMinutes(15));

            Log.Information("GetAllAsync Successfully retrieved a List of {WorkInstructionCount} WorkInstructions", workInstructions.Count);
            return workInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetAllAsync Work Instructions, in WorkInstructionService. Exception: {Exception}", e.ToString());
            return [];
        }
    }

    /// <summary>
    /// Asynchronously retrieves only the latest versions of all work instructions,
    /// using caching for performance.
    /// </summary>
    /// <returns>List of work instructions where IsLatest is true.</returns>
    /// <remarks>
    /// Results are cached for 15 minutes to improve performance.
    /// </remarks>
    public async Task<List<WorkInstruction>> GetAllLatestAsync()
    {
        const string WORK_INSTRUCTION_LATEST_CACHE_KEY = WORK_INSTRUCTION_CACHE_KEY + "_Latest";

        if (_cache.TryGetValue(WORK_INSTRUCTION_LATEST_CACHE_KEY, out List<WorkInstruction>? cachedLatestList) &&
            cachedLatestList != null)
        {
            return cachedLatestList;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var latestWorkInstructions = await context.WorkInstructions
                .Where(w => w.IsLatest)
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .ToListAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_LATEST_CACHE_KEY, latestWorkInstructions, TimeSpan.FromMinutes(15));

            Log.Information("GetAllLatestAsync successfully retrieved {WorkInstructionCount} latest WorkInstructions", latestWorkInstructions.Count);
            return latestWorkInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown in GetAllLatestAsync in WorkInstructionService. Exception: {Exception}", e.ToString());
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
                .FirstOrDefault(w => w.Title.ToUpper() == title.ToUpper());

            Log.Information("Successfully retrieved a WorkInstruction by title: {Title}", title);
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
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            return workInstruction;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetByIdAsync with ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves the full version history for a given work instruction lineage,
    /// identified by its OriginalId. Results are ordered by LastModifiedOn descending (most recent edits first).
    /// </summary>
    /// <param name="originalId">The OriginalId of the work instruction lineage to retrieve.</param>
    /// <returns>
    /// List of all versions in the lineage, including the original itself,
    /// ordered by LastModifiedOn descending.
    /// </returns>
    public async Task<List<WorkInstruction>> GetVersionHistoryAsync(int originalId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var versionHistory = await context.WorkInstructions
                .Where(w => w.OriginalId == originalId || w.Id == originalId)
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .OrderByDescending(w => w.LastModifiedOn)
                .ToListAsync();

            Log.Information(
                "GetVersionHistoryAsync successfully retrieved {Count} versions for OriginalId {OriginalId}",
                versionHistory.Count,
                originalId
            );

            return versionHistory;
        }
        catch (Exception e)
        {
            Log.Warning(
                "Exception thrown in GetVersionHistoryAsync in WorkInstructionService. Exception: {Exception}",
                e.ToString()
            );
            return [];
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> MarkAllVersionsNotLatestAsync(int originalId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var versions = await context.WorkInstructions
            .Where(w => w.OriginalId == originalId || w.Id == originalId)
            .ToListAsync();

        foreach (var wi in versions)
        {
            wi.IsLatest = false;
        }

        await context.SaveChangesAsync();
        return true;
    }
    
    /// <inheritdoc />
    public async Task MarkOtherVersionsInactiveAsync(int workInstructionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var target = await context.WorkInstructions.FindAsync(workInstructionId);
        if (target == null) return;

        int rootId = target.OriginalId ?? target.Id;

        var chain = await context.WorkInstructions
            .Where(w => (w.Id == rootId || w.OriginalId == rootId) && w.Id != workInstructionId)
            .ToListAsync();

        foreach (var wi in chain)
        {
            wi.IsActive = false;
        }

        await context.SaveChangesAsync();
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

            // Having to refetch data since we are using DbContextFactory and the change-tracker is reset on each instantiation
            foreach (var product in workInstruction.Products.Where(product => product.Id > 0))
            {
                context.Attach(product);
                context.Entry(product).State = EntityState.Unchanged;
            }
            
            foreach (var node in workInstruction.Nodes)
            {
                if (node is not PartNode partNode)
                {
                    continue;
                }
                
                foreach (var part in partNode.Parts.Where(p => p.Id > 0).ToList())
                {
                    // Attach existing part to the context
                    context.Attach(part);
                    context.Entry(part).State = EntityState.Unchanged;
                }
            }
            
            workInstruction.IsActive = false;
            await context.WorkInstructions.AddAsync(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            
            Log.Information("Successfully created WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Create a work instruction, in WorkInstructionService. Exception: {Exception}", e.ToString());
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
            
            // Remove associated Work Instruction Node Images first
            await DeleteImagesByWorkInstructionAsync(workInstruction);
            
            // Remove associated Work Instruction Nodes first
            context.WorkInstructionNodes.RemoveRange(workInstruction.Nodes);

            context.WorkInstructions.Remove(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            
            Log.Information("Successfully deleted WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Delete a work instruction with ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteNodesAsync(IEnumerable<WorkInstructionNode> nodes)
    {
        if (nodes == null || !nodes.Any())
            return true; // Nothing to delete, treat as success

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var nodeIds = nodes.Select(n => n.Id).ToList();

            var nodesToDelete = await context.WorkInstructionNodes
                .Where(n => nodeIds.Contains(n.Id))
                .ToListAsync();

            if (nodesToDelete.Count == 0)
                return true; // No matching nodes found

            // Delete associated images/media files first
            await DeleteImagesByNodesAsync(nodesToDelete);

            // Remove the nodes themselves
            context.WorkInstructionNodes.RemoveRange(nodesToDelete);

            await context.SaveChangesAsync();

            // Invalidate cache if applicable
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Deleted {Count} WorkInstructionNodes and their images successfully.", nodesToDelete.Count);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to delete WorkInstructionNodes and images. Exception: {Exception}", e.ToString());
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAllVersionsByIdAsync(int id)
    {
        try
        {
            // assuming the input work instruction is the original
            await using var context = await _contextFactory.CreateDbContextAsync();
            var originalWorkInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (originalWorkInstruction == null)
            {
                return false;
            }


            // if input work instruction is not the original, find it
            if (originalWorkInstruction.OriginalId != null)
            {
                var ogId = originalWorkInstruction.OriginalId;

                originalWorkInstruction = await context.WorkInstructions
                    .Include(w => w.Nodes)
                    .FirstOrDefaultAsync(w => w.Id == ogId);

                if (originalWorkInstruction == null)
                {
                    return false;
                }
            }
            
            // record original id to get the rest or the versions but delete this one now
            var originalId =  originalWorkInstruction.Id;
            
            // query for all work instructions associated with the original
            var otherVersions = await context.WorkInstructions
                .Where(w => w.OriginalId == originalId)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).Parts)
                .ToListAsync();
            
            otherVersions.Add(originalWorkInstruction);
            
            // delete each one
            foreach (var version in otherVersions)
            {
                // Remove all production logs associated with a work instruction
                await  _productionLogService.DeleteProductionLogByWorkInstructionAsync(version);
                await DeleteImagesByWorkInstructionAsync(version);
               
                // Remove associated Work Instruction Nodes first
                context.WorkInstructionNodes.RemoveRange(version.Nodes);
                context.WorkInstructions.Remove(version);
            }

            //save
            await context.SaveChangesAsync();

            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Successfully deleted all versions associated with WorkInstruction ID: {workInstructionID}", originalWorkInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Delete all versions associated with WorkInstruction ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
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
        Log.Information("Beginning to update Work Instruction: {Id}", workInstruction.Id);
        if (workInstruction.Id == 0)
            return false;

        await using ApplicationContext context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var existing = await context.WorkInstructions
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .FirstOrDefaultAsync(w => w.Id == workInstruction.Id);

            if (existing == null)
                return false;

            // Load parts for PartNodes
            foreach (var partNode in existing.Nodes.OfType<PartNode>())
                await context.Entry(partNode).Collection(p => p.Parts).LoadAsync();

            // Enforce uniqueness
            if (!string.Equals(existing.Title, workInstruction.Title, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.Version, workInstruction.Version, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsUnique(workInstruction))
                    return false;
            }

            // Update basic fields
            context.Entry(existing).CurrentValues.SetValues(workInstruction);

            await UpdateProducts(context, existing, workInstruction);
            await UpdateNodes(context, existing, workInstruction);

            await context.SaveChangesAsync();
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Successfully updated WorkInstruction with ID: {Id}", workInstruction.Id);
            return true;
        }
        catch (Exception e)
        {
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            Log.Warning("Error updating WorkInstruction {Id}: {Exception}", workInstruction.Id, e);
            return false;
        }
    }

    private static async Task UpdateProducts(DbContext context, WorkInstruction existing, WorkInstruction updated)
    {
        var productIds = updated.Products.Select(p => p.Id).ToHashSet();

        var attachedProducts = await context.Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        existing.Products = attachedProducts;
    }
    
    private async Task UpdateNodes(ApplicationContext context, WorkInstruction existing, WorkInstruction updated)
    {
        var newNodeIds = updated.Nodes.Select(n => n.Id).ToHashSet();

        // Remove deleted nodes
        existing.Nodes.RemoveAll(n => n.Id != 0 && !newNodeIds.Contains(n.Id));

        // Add new nodes
        foreach (var newNode in updated.Nodes.Where(n => n.Id == 0))
            existing.Nodes.Add(newNode);

        // Update existing nodes
        foreach (var newNode in updated.Nodes.Where(n => n.Id != 0))
        {
            var existingNode = existing.Nodes.FirstOrDefault(n => n.Id == newNode.Id);
            if (existingNode == null) continue;

            context.Entry(existingNode).CurrentValues.SetValues(newNode);

            if (existingNode is Step step && newNode is Step newStep)
                UpdateStep(context, step, newStep);

            else if (existingNode is PartNode partNode && newNode is PartNode newPartNode)
                await UpdatePartNode(context, partNode, newPartNode);
        }
    }
    
    private static void UpdateStep(DbContext context, Step existing, Step updated)
    {
        // Merge PrimaryMedia
        foreach (var item in updated.PrimaryMedia)
        {
            if (!existing.PrimaryMedia.Contains(item))
                existing.PrimaryMedia.Add(item);
        }

        // Merge SecondaryMedia
        foreach (var item in updated.SecondaryMedia)
        {
            if (!existing.SecondaryMedia.Contains(item))
                existing.SecondaryMedia.Add(item);
        }

        //remove any images no longer present in the update
        existing.PrimaryMedia = existing.PrimaryMedia
            .Where(m => updated.PrimaryMedia.Contains(m))
            .ToList();
        existing.SecondaryMedia = existing.SecondaryMedia
            .Where(m => updated.SecondaryMedia.Contains(m))
            .ToList();

        // EF will track the changes automatically since these are scalar properties (strings).
        context.Entry(existing).State = EntityState.Modified;
    }
    
    private async Task UpdatePartNode(ApplicationContext context, PartNode existing, PartNode updated)
    {
        Log.Information("Updating PartNode ID {Id}: Incoming parts = {Count}", updated.Id, updated.Parts?.Count ?? 0);

        context.Entry(existing).CurrentValues.SetValues(updated);

        var newParts = new List<Part>();

        if (updated.Parts != null)
        {
            foreach (var incoming in updated.Parts)
            {
                if (incoming.Id == 0)
                {
                    context.Parts.Add(incoming);
                    newParts.Add(incoming);
                }
                else
                {
                    var existingPart = await context.Parts.FindAsync(incoming.Id);
                    if (existingPart != null)
                    {
                        context.Entry(existingPart).CurrentValues.SetValues(incoming);
                        newParts.Add(existingPart);
                    }
                    else
                    {
                        Log.Warning("Part ID {Id} not found for PartNode {NodeId}", incoming.Id, updated.Id);
                    }
                }
            }
        }

        // Remove old parts
        var toRemove = existing.Parts.Where(p => newParts.All(np => np.Id != p.Id)).ToList();
        foreach (var part in toRemove)
            existing.Parts.Remove(part);

        // Add new parts
        var toAdd = newParts.Where(np => existing.Parts.All(p => p.Id != np.Id)).ToList();
        foreach (var part in toAdd)
            existing.Parts.Add(part);

        Log.Information("PartNode ID {Id} now has {Count} parts", existing.Id, existing.Parts.Count);

        context.Entry(existing).State = EntityState.Modified;
    }
    
    /// <summary>
    /// Saves an uploaded image file to the work instruction images directory and returns its relative path.
    /// </summary>
    /// <param name="file">The uploaded browser file.</param>
    /// <returns>The relative path to the saved image (for database storage).</returns>
    public async Task<string> SaveImageFileAsync(IBrowserFile file)
    {
        try
        {
            // Decide where on disk to save
            var imageDir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);

            if (!Directory.Exists(imageDir))
            {
                Directory.CreateDirectory(imageDir);
            }

            // Create a unique filename with original extension preserved
            var extension = Path.GetExtension(file.Name);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";  // Default to png if browser doesn't send extension
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(imageDir, fileName);

            // Save file contents
            await using (var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {                
                await stream.CopyToAsync(fileStream);
            }

            // Return the relative path for storing in the DB
            var relativePath = Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName);
            Log.Information("Saved image file: {RelativePath}", relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving uploaded image file: {FileName}", file?.Name ?? "unknown");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> SaveImageFileAsync(string file)
    {
        try
        {
            // Decide where on disk to save
            var imageDir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);

            if (!Directory.Exists(imageDir))
            {
                Directory.CreateDirectory(imageDir);
            }

            var extension = Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";  // Default to png if browser doesn't send extension
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(imageDir, fileName);

            // Save file contents
            await using (var stream = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, file), FileMode.Open, FileAccess.Read))
            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Return the relative path for storing in the DB
            var relativePath = Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName);
            Log.Information("Saved image file: {RelativePath}", relativePath);

            return relativePath;
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Error saving uploaded image file: {FileName}", file ?? "unknown");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteImagesByWorkInstructionAsync(WorkInstruction instruction)
    {
        foreach (var node in instruction.Nodes)
        {
            if (node is Step stepNode)
            {
                List<string> allImages = [..stepNode.PrimaryMedia, ..stepNode.SecondaryMedia];
                foreach (var im in allImages)
                {
                    await DeleteImageFile(im);
                }
            }
        }
    }
    
    /// <summary>
    /// Deletes images associated with the specified <paramref name="nodes"/>.
    /// </summary>
    /// <param name="nodes">The collection of <see cref="WorkInstructionNode"/> entities whose images should be deleted.</param>
    private async Task DeleteImagesByNodesAsync(IEnumerable<WorkInstructionNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is Step stepNode)
            {
                // Concatenate all images from primary and secondary media
                var allImages = stepNode.PrimaryMedia.Concat(stepNode.SecondaryMedia).ToList();

                foreach (var image in allImages)
                {
                    await DeleteImageFile(image);
                }
            }
        }
    }

    /// <inheritdoc/>
    public Task DeleteImageFile(string FileName)
    {
        try
        {
            // find where on disk is saved
            var imageDir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);

            if (Directory.Exists(imageDir))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, FileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Log.Information("Deleted image file: {FileName}", FileName);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error Deleting stored image file: {FileName}", FileName);
            throw;
        }

        return Task.CompletedTask;
    }


    /// <summary>
    /// Regex pattern for parsing parts list strings in the format "(PART_NUMBER, PART_NAME)"
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"\(([^,]+),\s*([^)]+)\)")]
    private static partial System.Text.RegularExpressions.Regex PartsListRegex();
    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex RemoveHtmlRegex();
}
