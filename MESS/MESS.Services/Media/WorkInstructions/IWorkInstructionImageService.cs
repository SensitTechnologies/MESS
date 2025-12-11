using MESS.Data.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace MESS.Services.Media.WorkInstructions;

/// <summary>
/// Defines operations for managing work instruction images,
/// including saving, deleting, and retrieving image files.
/// </summary>
public interface IWorkInstructionImageService
{
    /// <summary>
    /// Saves an uploaded image file for a work instruction and returns its relative path for database storage.
    /// </summary>
    /// <param name="file">The uploaded browser file.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is the relative path
    /// (e.g., "WorkInstructionImages/guid-filename.png") to the saved image.
    /// </returns>
    Task<string> SaveImageFileAsync(IBrowserFile file);

    /// <summary>
    /// Saves an uploaded image file for a work instruction and returns its relative path for database storage.
    /// </summary>
    /// <param name="file">The uploaded file string</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is the relative path
    /// (e.g., "WorkInstructionImages/guid-filename.png") to the saved image.
    /// </returns>
    Task<string> SaveImageFileAsync(string file);

    /// <summary>
    /// Removes an Image File from the server
    /// </summary>
    /// <param name="fileName">The File to be deleted</param>
    /// <returns>
    /// A task representing the synchronous operation.
    /// </returns>
    Task DeleteImageFile(string fileName);
    
    /// <summary>
    /// Removes all Image Files related to a Work Instruction from the server
    /// </summary>
    /// <param name="instruction">The Work Instruction to delete all images from</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task DeleteImagesByWorkInstructionAsync(WorkInstruction instruction);
    
    
    /// <summary>
    /// Deletes images associated with the specified work instruction nodes.
    /// </summary>
    /// <param name="nodes">The nodes whose images should be deleted.</param>
    Task DeleteImagesByNodesAsync(IEnumerable<WorkInstructionNode> nodes);
}