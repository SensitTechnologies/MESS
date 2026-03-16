namespace MESS.Services.CRUD.Tags;

/// <summary>
/// Provides functionality to generate sequential tag codes
/// based on a specified numbering scheme.
/// </summary>
public static class TagCodeGenerator
{
    /// <summary>
    /// Generates a sequence of tag codes.
    /// </summary>
    /// <param name="scheme">The numbering scheme used to generate tag codes.</param>
    /// <param name="prefix">Optional prefix applied to each tag code (e.g., "TAG-").</param>
    /// <param name="start">The starting index for generation. Zero-based.</param>
    /// <param name="count">Number of tag codes to generate.</param>
    /// <param name="padding">Optional numeric padding (for decimal scheme).</param>
    public static IEnumerable<string> Generate(
        TagNumberingScheme scheme,
        string? prefix,
        int start,
        int count,
        int padding = 0)
    {
        prefix ??= "";

        switch (scheme)
        {
            case TagNumberingScheme.Decimal:
                for (int i = start; i < start + count; i++)
                    yield return prefix + i.ToString($"D{padding}");
                break;

            case TagNumberingScheme.Hexadecimal:
                for (int i = start; i < start + count; i++)
                    yield return prefix + i.ToString("X");
                break;

            case TagNumberingScheme.Alphanumeric:
                for (int i = start; i < start + count; i++)
                    yield return prefix + ToAlpha(i);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null);
        }
    }

    /// <summary>
    /// Converts a zero-based number into an alphabetical string (A, B, ..., Z, AA, AB, ...).
    /// </summary>
    private static string ToAlpha(int number)
    {
        string result = "";
        number++;

        while (number > 0)
        {
            number--;
            result = (char)('A' + number % 26) + result;
            number /= 26;
        }

        return result;
    }
}