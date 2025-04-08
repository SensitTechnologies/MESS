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

namespace MESS.Services.WorkInstruction;
using MESS.Data.Models;
public partial class WorkInstructionService : IWorkInstructionService
{
    private readonly ApplicationContext _context;
    private readonly IProductService _productService;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // The following attributes define the current expected XLSX structure as of 3/28/2025
    private const string INSTRUCTION_TITLE_CELL = "B1";
    private const string VERSION_CELL = "D1";
    private const string PRODUCT_NAME_CELL = "B2";
    private const string QR_CODE_REQUIRED_CELL = "B1";
    private const string STEPS_PARTS_LIST_CELL = "B3";
    
    
    // Using int values here since there is an indeterminate amount of steps per Work Instruction
    private const int STEP_START_ROW = 7;
    private const int STEP_START_COLUMN = 1;
    private const int STEP_TITLE_COLUMN = 2;
    private const int STEP_DESCRIPTION_COLUMN = 3;
    private const int STEP_PARTS_LIST_COLUMN = 4;
    private const int STEP_MEDIA_COLUMN = 5;

    private const string WORK_INSTRUCTION_IMAGES_DIRECTORY = "WorkInstructionImages";
    const string WORK_INSTRUCTION_CACHE_KEY = "AllWorkInstructions";

    public WorkInstructionService(ApplicationContext context, IProductService productService, IMemoryCache cache, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _productService = productService;
        _cache = cache;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<WorkInstructionImportResult> ImportFromXlsx(List<IBrowserFile> files)
    {
        try
        {
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
            
            var workInstruction = new WorkInstruction
            {
                Title = worksheet.Cell(INSTRUCTION_TITLE_CELL).GetString(),
                Version = versionString,
                Products = [],
                Nodes = []
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

            // Add any required parts to the work instruction
            var partsNode = new PartNode();
            var partsList = await GetPartsListFromString(worksheet.Cell(STEPS_PARTS_LIST_CELL).GetString());

            if (partsList != null)
            {
                partsNode.Parts.AddRange(partsList);
                workInstruction.Nodes.Add(partsNode);
            }

            // Start from row 7 (assuming header row is 6)
            var stepStartRow = STEP_START_ROW;
            while (!worksheet.Cell(stepStartRow, STEP_START_COLUMN).IsEmpty())
            {
                var step = new Step
                {
                    Name = GetRichTextFromCell(worksheet.Cell(stepStartRow, STEP_TITLE_COLUMN)),
                    Content = new List<string>(),
                    Body = GetRichTextFromCell(worksheet.Cell(stepStartRow, STEP_DESCRIPTION_COLUMN)),
                    SubmitTime = DateTimeOffset.UtcNow,
                };
                
                var pictures = worksheet.Pictures
                    .Where(p => p.TopLeftCell.Address.RowNumber == stepStartRow 
                                && p.TopLeftCell.Address.ColumnNumber == STEP_MEDIA_COLUMN)
                    .ToList();
                
                foreach (var picture in pictures)
                {
                    var fileName = $"{Guid.NewGuid()}.{picture.Format.ToString()}";

                    var imageDir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);

                    var imagePath = Path.Combine(imageDir, fileName);
                    
                    // Verify that directory exists. If not create it.
                    if (!Directory.Exists(imageDir))
                    {
                        Directory.CreateDirectory(imageDir);
                    }
                    
                    using (var ms = new MemoryStream())
                    {
                        await picture.ImageStream.CopyToAsync(ms);
                        var imageBytes = ms.ToArray();
                        await File.WriteAllBytesAsync(imagePath, imageBytes);
                    }
                    
                    step.Content.Add(Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName));
                }
            
                // Step extends WorkInstructionNode for ordering purposes
                // Calculation 0 Indexes the step order. 1st step has position 0.
                step.Position = stepStartRow - STEP_START_ROW;
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

    private string GetRichTextFromCell(IXLCell cell)
    {
        if (!cell.HasRichText)
        {
            return cell.GetString();
        }

        var sb = new StringBuilder();
        sb.Append("<div>");

        foreach (var richText in cell.GetRichText())
        {
            var styles = new List<string>();

            if (richText.Bold) styles.Add("font-weight: bold");
            if (richText.Italic) styles.Add("font-style: italic");
            // if (richText.Underlined) styles.Add("text-decoration: underline");
            // if (richText.Strike) styles.Add("text-decoration: line-through");
            if (richText.FontSize > 0) styles.Add($"font-size: {richText.FontSize}pt");
            if (richText.FontColor != XLColor.NoColor) styles.Add($"color: {richText.FontColor}");

            var text = WebUtility.HtmlEncode(richText.Text);
        
            if (styles.Any())
            {
                sb.Append($"<span style=\"{string.Join(";", styles)}\">{text}</span>");
            }
            else
            {
                sb.Append(text);
            }
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    // Expected string format is as follows:
    // (PART_NAME, PART_SERIAL_NUMBER), (PART_NAME, PART_SERIAL_NUMBER), (PART_NAME, PART_SERIAL_NUMBER), ...
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

    private async Task<Part?> GetOrAddPart(Part partToAdd)
    {
        try
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p =>
                p.PartName == partToAdd.PartName &&
                p.PartNumber == partToAdd.PartNumber);

            // If a part does not exist in the database create it here
            // and return with database generated ID
            if (part != null)
            {
                return part;
            }
            
            await _context.Parts.AddAsync(partToAdd);
            return await _context.Parts.FindAsync(partToAdd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public List<WorkInstruction> GetAll()
    {
        try
        {
            var workInstructions = _context.WorkInstructions
                .Include(w => w.Nodes)
                .ToList();

            return workInstructions;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetAll Work Instructions, in WorkInstructionService", e.GetBaseException().ToString());
            return [];
        }
    }

    public async Task<List<WorkInstruction>> GetAllAsync()
    {
        if (_cache.TryGetValue(WORK_INSTRUCTION_CACHE_KEY, out List<WorkInstruction>? cachedWorkInstructionList) &&
            cachedWorkInstructionList != null)
        {
            return cachedWorkInstructionList;
        }
        
        try
        {
            var workInstructions = await _context.WorkInstructions
                .Include(w => w.Nodes)
                .ToListAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_CACHE_KEY, workInstructions, TimeSpan.FromMinutes(15));

            return workInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetAllAsync Work Instructions, in WorkInstructionService", e.GetBaseException().ToString());
            throw;
        }
    }
    

    public WorkInstruction? GetByTitle(string title)
    {
        try
        {
            var workInstruction = _context.WorkInstructions
                .FirstOrDefault(w => w.Title == title);

            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByTitle with Title: {title}, in WorkInstructionService", e.GetBaseException().ToString(), title);
            return null;
        }
    }

    public WorkInstruction? GetById(int id)
    {
        try
        {
            var workInstruction = _context.WorkInstructions.Find(id);

            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetById with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return null;
        }
    }

    public async Task<WorkInstruction?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return null;
            }
            
            var workInstruction = await _context.WorkInstructions
                .Include(w => w.Nodes)
                .FirstAsync(w => w.Id == id);
            
            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByIdAsync with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return null;
        }
    }

    public async Task<bool> Create(WorkInstruction workInstruction)
    {
        try
        {
            // Validate WorkInstruction
            var workInstructionValidator = new WorkInstructionValidator();
            var validationResult = workInstructionValidator.Validate(workInstruction);

            if (!validationResult.IsValid)
            {
                return false;
            }

            workInstruction.IsActive = true;
            await _context.WorkInstructions.AddAsync(workInstruction);
            await _context.SaveChangesAsync();
            
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

    public async Task<bool> DeleteByIdAsync(int id)
    {
        try
        {
            var workInstruction = await _context.WorkInstructions.FindAsync(id);
            
            if (workInstruction == null)
            {
                return false;
            }

            workInstruction.IsActive = false;
            await _context.SaveChangesAsync();
            
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

    public async Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction)
    {
        // if ID is 0 that means it has NOT been saved to the database
        if (workInstruction.Id == 0)
        {
            return false;
        }
        try
        {
            // verify that the WorkInstruction already exists in the database
            var existingWorkInstruction = await _context.WorkInstructions.FindAsync(workInstruction.Id);

            if (existingWorkInstruction == null)
            {
                return false;
            }
            
            Log.Information("Successfully updated WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            _context.WorkInstructions.Update(workInstruction);
            await _context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to UpdateWorkInstructionAsync with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), workInstruction.Id);
            return false;
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\(([^,)]+)(?:,\s*)?([^)]+)\)")]
    private static partial System.Text.RegularExpressions.Regex PartsListRegex();
}