using MESS.Data.Models;

namespace MESS.Services.Files.ApplicationUsers;

/// <summary>
/// Provides functionality to import and export application users from/to CSV files.
/// IDs are excluded to allow importing into different databases.
/// </summary>
public interface IApplicationUserFileService
{
    /// <summary>
    /// Exports a collection of <see cref="ApplicationUser"/> instances to a CSV-formatted string.
    /// </summary>
    /// <param name="users">The collection of users to export. Each user will be represented as a CSV row.</param>
    /// <param name="userRoles">
    /// Roles for each user will be included as a semicolon-separated list in the CSV. 
    /// If null, the Roles column will be empty for all users.
    /// </param>
    /// <returns>A CSV-formatted string representing the users, including a header row. 
    /// The CSV includes the columns: UserName, Email, FirstName, LastName, Roles.
    /// User IDs are excluded to allow importing into different databases.</returns>
    string ExportToCsv(IEnumerable<ApplicationUser> users, IDictionary<string, IEnumerable<string>>? userRoles = null);

    /// <summary>
    /// Imports application users from a CSV-formatted string.
    /// </summary>
    /// <param name="csvData">The CSV data containing user information. 
    /// The CSV must include the columns: UserName, Email, FirstName, LastName, Roles.
    /// The first row is expected to be a header row. User IDs in the CSV are ignored.</param>
    /// <param name="userRoles">
    /// An output dictionary that will be populated with the roles for each user. 
    /// The key is username, and the value is a list of role names parsed from the Roles column. 
    /// Roles are expected to be semicolon-separated in the CSV. This dictionary can be used to assign roles after importing users.
    /// </param>
    /// <returns>
    /// A collection of <see cref="ApplicationUser"/> instances created from the CSV data, 
    /// with properties populated but without IDs. The returned users are ready to be saved to a database using your user service.
    /// </returns>
    /// <remarks>
    /// Each CSV row must have exactly six columns. Empty or malformed rows will cause parsing errors. 
    /// Boolean values in the IsActive column must be 'true' or 'false' (case-insensitive). 
    /// Roles are optional and can be empty. Duplicate UserNames or Emails within the CSV are not checked here and should be validated separately using <see cref="ValidateCsv"/>.
    /// </remarks>
    IEnumerable<ApplicationUser> ImportFromCsv(string csvData, out Dictionary<string, List<string>> userRoles);

    /// <summary>
    /// Validates the CSV data before import.
    /// Returns a list of errors, or an empty list if valid.
    /// </summary>
    /// <param name="csvData">The CSV data to validate.</param>
    /// <returns>A list of validation error messages.</returns>
    IEnumerable<string> ValidateCsv(string csvData);
}