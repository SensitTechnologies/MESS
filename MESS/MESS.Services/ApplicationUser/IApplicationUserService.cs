namespace MESS.Services.ApplicationUser;
using Data.Models;

public interface IApplicationUserService
{
    /// <summary>
    /// Signs out the current user.
    /// </summary>
    public Task SignOutAsync();
    /// <summary>
    /// Signs in a user by email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>True if sign-in was successful, otherwise false.</returns>
    public Task<bool> SignInAsync(string email);
    /// <summary>
    /// Gets a list of users by role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>A list of users in the specified role.</returns>
    public Task<List<ApplicationUser>> GetUsersByRoleAsync(string roleName);
    //<summary>
    // Retrieves a list of all ApplicationUsers currently registered
    //</summary>
    //<returns>List of ApplicationUser objects</returns>
    public Task<List<ApplicationUser>> GetApplicationUsers();
    
    //<summary>
    // Retrieves a ApplicationUser by id number
    //</summary>
    //<returns>ApplicationUser object</returns>
    public ApplicationUser? GetApplicationUserById(string id);
    
    //<summary>
    // Retrieves a ApplicationUser by last name
    //</summary>
    //<returns>ApplicationUser object</returns>
    public ApplicationUser? GetApplicationUserByLastName(string lastName);
    
    //<summary>
    // Creates a ApplicationUser object and saves it to the database
    //</summary>
    //<returns>ApplicationUser object</returns>
    public Task<bool> AddApplicationUser(ApplicationUser ApplicationUser);
    
    //<summary>
    // Updates a ApplicationUser currently in the database
    //</summary>
    //<returns>Updated ApplicationUser object</returns>
    public Task<bool> UpdateApplicationUser(ApplicationUser ApplicationUser);
    
    //<summary>
    // Deletes a ApplicationUser currently in the database
    //</summary>
    //<returns>Deleted ApplicationUser boolean</returns>
    public Task<bool> DeleteApplicationUser(string id);
    
}