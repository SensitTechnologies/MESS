using MESS.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MESS.Services.DTOs;

/// <summary>
/// Data transfer object representing a user and their associated roles
/// </summary>
public class UserRoleDTO
{
    /// <summary>
    /// The application user associated with these roles.
    /// </summary>
    public required ApplicationUser User { get; set; }
    /// <summary>
    /// List of role names assigned to the user
    /// </summary>
    public IList<string>? Roles { get; set; }
    /// <summary>
    /// Indicates whether the user account is locked out
    /// </summary>
    public bool IsLockedOut { get; set; }
    
    /// <summary>
    /// Indicates whether the user has been saved to the database
    /// </summary>
    public bool IsNew { get; set; }
}