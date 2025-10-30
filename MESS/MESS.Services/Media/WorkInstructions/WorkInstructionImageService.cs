using MESS.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace MESS.Services.Media.WorkInstructions;

/// <summary>
/// Provides functionality for managing images associated with work instructions,
/// including saving uploaded files, deleting images for specific nodes or instructions,
/// and ensuring the image storage directory exists.
/// </summary>
public class WorkInstructionImageService : IWorkInstructionImageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    private const string WORK_INSTRUCTION_IMAGES_DIRECTORY = "WorkInstructionImages";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionImageService"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The hosting environment for accessing web root paths.</param>
    public WorkInstructionImageService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }
    
    /// <summary>
    /// Ensures that the image directory exists and returns its full path.
    /// </summary>
    private string EnsureImageDirectory()
    {
        var dir = Path.Combine(_webHostEnvironment.WebRootPath, WORK_INSTRUCTION_IMAGES_DIRECTORY);
        Directory.CreateDirectory(dir); // Safe even if it already exists
        return dir;
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
            var imageDir = EnsureImageDirectory();

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
            var relativePath = Path.Combine(WORK_INSTRUCTION_IMAGES_DIRECTORY, fileName)
                .Replace("\\", "/");
            Log.Information("Saved image file: {RelativePath}", relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving uploaded image file: {FileName}", file.Name);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> SaveImageFileAsync(string file)
    {
        try
        {
            var imageDir = EnsureImageDirectory();

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
            Log.Error(ex, "Error saving uploaded image file: {FileName}", file);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task DeleteImagesByWorkInstructionAsync(WorkInstruction instruction)
    {
        await DeleteImagesByNodesAsync(instruction.Nodes);
    }
    
    /// <summary>
    /// Deletes images associated with the specified <paramref name="nodes"/>.
    /// </summary>
    /// <param name="nodes">The collection of <see cref="WorkInstructionNode"/> entities whose images should be deleted.</param>
    public async Task DeleteImagesByNodesAsync(IEnumerable<WorkInstructionNode> nodes)
    {
        // Flatten all image paths from all nodes
        var allImages = nodes
            .OfType<Step>() // Only steps have images
            .SelectMany(step => step.PrimaryMedia.Concat(step.SecondaryMedia))
            .ToList();

        if (!allImages.Any())
            return;

        // Delete images concurrently (but safely)
        await Parallel.ForEachAsync(allImages, async (image, _) =>
        {
            try
            {
                await DeleteImageFile(image);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to delete image: {Image}", image);
                // continue deleting other images
            }
        });
    }
    
    /// <inheritdoc/>
    public async Task DeleteImageFile(string fileName)
    {
        try
        {
            // Ensure the directory exists and get its path
            var imageDir = EnsureImageDirectory();

            // Combine directory with the relative file name
            var fullPath = Path.IsPathRooted(fileName)
                ? fileName
                : Path.Combine(imageDir, Path.GetFileName(fileName));

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                Log.Information("Deleted image file: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting stored image file: {FileName}", fileName);
            throw;
        }
    }
}