using System.Text;
using MESS.Data.Models;

namespace MESS.Services.Files.ApplicationUsers;

/// <summary>
/// Implementation of <see cref="IApplicationUserFileService"/> for CSV operations.
/// </summary>
public class ApplicationUserFileService : IApplicationUserFileService
{
    /// <inheritdoc/>
    public string ExportToCsv(IEnumerable<ApplicationUser> users, IDictionary<string, IEnumerable<string>>? userRoles = null)
    {
        if (users == null) throw new ArgumentNullException(nameof(users));

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("UserName,Email,FirstName,LastName,Roles");

        foreach (var user in users)
        {
            string Escape(string? value)
            {
                if (string.IsNullOrEmpty(value)) return "";
                var escaped = value.Replace("\"", "\"\"");
                if (escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n'))
                    escaped = $"\"{escaped}\"";
                return escaped;
            }

            // Get roles for user if provided
            var rolesList = userRoles != null && userRoles.TryGetValue(user.UserName ?? "", out var roles)
                ? string.Join(";", roles)
                : "";

            var line = string.Join(",",
                Escape(user.UserName),
                Escape(user.Email),
                Escape(user.FirstName),
                Escape(user.LastName),
                Escape(rolesList)
            );

            sb.AppendLine(line);
        }

        return sb.ToString();
    }
    
    /// <inheritdoc/>
    public IEnumerable<ApplicationUser> ImportFromCsv(string csvData, out Dictionary<string, List<string>> userRoles)
    {
        if (string.IsNullOrWhiteSpace(csvData))
            throw new ArgumentException("CSV data cannot be null or empty.", nameof(csvData));

        var users = new List<ApplicationUser>();
        userRoles = new Dictionary<string, List<string>>();

        using var reader = new StringReader(csvData);
        string? line;

        // Read header
        line = reader.ReadLine();
        if (line == null) throw new FormatException("CSV data is empty, no header found.");

        var headers = ParseCsvLine(line);
        if (headers.Count != 5)
            throw new FormatException("CSV must have 5 columns: UserName, Email, FirstName, LastName, Roles");

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Count != 5)
                throw new FormatException($"CSV row does not have 5 columns: {line}");

            var user = new ApplicationUser
            {
                UserName = fields[0],
                Email = fields[1],
                FirstName = fields[2],
                LastName = fields[3],
            };

            users.Add(user);

            // Parse roles into dictionary
            var roles = string.IsNullOrWhiteSpace(fields[4])
                ? new List<string>()
                : fields[4].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            userRoles[user.UserName ?? ""] = roles;
        }

        return users;
    }

    /// <summary>
    /// Parses a single CSV line into a list of fields, handling quoted fields and escaped quotes.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Handle double quote escape
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // skip next quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        result.Add(current.ToString());
        return result;
    }

    /// <inheritdoc/>
    public IEnumerable<string> ValidateCsv(string csvData)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(csvData))
        {
            errors.Add("CSV data is empty.");
            return errors;
        }

        using var reader = new StringReader(csvData);
        string? line;
        int rowIndex = 0;

        // Read header
        line = reader.ReadLine();
        rowIndex++;
        if (line == null)
        {
            errors.Add("CSV has no header row.");
            return errors;
        }

        var headers = ParseCsvLine(line);
        if (headers.Count != 5)
        {
            errors.Add("CSV header must have 5 columns: UserName,Email,FirstName,LastName,Roles");
        }

        var userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while ((line = reader.ReadLine()) != null)
        {
            rowIndex++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Count != 5)
            {
                errors.Add($"Row {rowIndex}: Expected 5 columns but found {fields.Count}.");
                continue;
            }

            var userName = fields[0];
            var email = fields[1];
            var firstName = fields[2];
            var lastName = fields[3];
            var roles = fields[4];

            // Required field checks
            if (string.IsNullOrWhiteSpace(userName)) errors.Add($"Row {rowIndex}: UserName is required.");
            if (string.IsNullOrWhiteSpace(email)) errors.Add($"Row {rowIndex}: Email is required.");
            if (string.IsNullOrWhiteSpace(firstName)) errors.Add($"Row {rowIndex}: FirstName is required.");
            if (string.IsNullOrWhiteSpace(lastName)) errors.Add($"Row {rowIndex}: LastName is required.");

            // Length checks (from your FluentValidation)
            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 1024)
                errors.Add($"Row {rowIndex}: FirstName exceeds max length of 1024 characters.");
            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 1024)
                errors.Add($"Row {rowIndex}: LastName exceeds max length of 1024 characters.");

            // Duplicate check within CSV
            if (!string.IsNullOrWhiteSpace(userName))
            {
                if (!userNames.Add(userName))
                    errors.Add($"Row {rowIndex}: Duplicate UserName '{userName}' in CSV.");
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!emails.Add(email))
                    errors.Add($"Row {rowIndex}: Duplicate Email '{email}' in CSV.");
            }

            // Optional: roles parsing validation
            if (!string.IsNullOrWhiteSpace(roles))
            {
                var invalidRoles = roles.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(r => string.IsNullOrWhiteSpace(r));
                if (invalidRoles.Any())
                    errors.Add($"Row {rowIndex}: Roles contain empty entries.");
            }
        }

        return errors;
    }
}