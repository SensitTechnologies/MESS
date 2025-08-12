using MESS.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MESS.Data.DTO;

/// <summary>
/// Data transfer object representing a user and their associated roles
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// The application user associated with these roles.
    /// </summary>
    public required ApplicationUser User { get; set; }
    
    /// <summary>
    /// Gets or sets the password for the user.  
    /// This is required only when creating a new user.
    /// </summary>
    public string Password { get; set; } = "";
    
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
    
    /// <summary>
    /// Represents the data required to register a new user.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// This field is optional and can be null.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the username for the new user.
        /// This field is required.
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// Gets or sets the password for the new user.
        /// This field is required.
        /// </summary>
        public string Password { get; set; } = "";
    }
}