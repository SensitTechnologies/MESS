using MESS.Services.DTOs.Tags;

namespace MESS.Services.CRUD.Tags;

/// <summary>
/// Provides functionality to generate sequential tag codes
/// based on a specified numbering scheme.
/// </summary>
public static class TagCodeGenerator
{
    /// <summary>
    /// Generates a sequence of tag codes from a batch request.
    /// </summary>
    public static IEnumerable<string> Generate(TagBatchCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Generate(
            request.Scheme,
            request.Prefix,
            request.Start,
            request.Count,
            request.Padding);
    }

    /// <summary>
    /// Generates a sequence of tag codes.
    /// </summary>
    private static IEnumerable<string> Generate(
        TagNumberingScheme scheme,
        string? prefix,
        int start,
        int count,
        int padding = 0)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero.", nameof(count));

        if (start < 0)
            throw new ArgumentException("Start must be non-negative.", nameof(start));

        if (padding < 0)
            throw new ArgumentException("Padding must be non-negative.", nameof(padding));

        prefix ??= string.Empty;

        // Prevent overflow
        checked
        {
            var end = start + count;

            for (var i = start; i < end; i++)
            {
                yield return scheme switch
                {
                    TagNumberingScheme.Decimal =>
                        prefix + i.ToString($"D{padding}"),

                    TagNumberingScheme.Hexadecimal =>
                        prefix + i.ToString(padding > 0 ? $"X{padding}" : "X"),

                    TagNumberingScheme.Alphanumeric =>
                        prefix + ToAlpha(i),

                    _ => throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null)
                };
            }
        }
    }

    /// <summary>
    /// Converts a zero-based number into an alphabetical string (A, B, ..., Z, AA, AB, ...).
    /// </summary>
    private static string ToAlpha(int number)
    {
        if (number < 0)
            throw new ArgumentException("Value must be non-negative.", nameof(number));

        var result = string.Empty;
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