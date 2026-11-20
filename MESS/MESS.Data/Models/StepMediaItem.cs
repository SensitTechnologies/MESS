using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// One photo attached to a work instruction <see cref="Step"/>: the stored file path
/// plus an optional caption shown below the photo in Production Log.
/// </summary>
public class StepMediaItem
{
    /// <summary>
    /// Maximum caption length accepted from the editor.
    /// </summary>
    public const int CaptionMaxLength = 200;

    /// <summary>
    /// Relative path under wwwroot where the image file is stored.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Optional caption displayed below the photo. Null/empty means no caption is shown.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Returns a copy of this item, so DTOs and entities never share a mutable instance.
    /// </summary>
    public StepMediaItem Clone() => new() { Path = Path, Caption = Caption };

    /// <summary>
    /// Normalizes user-entered caption text: trimmed, with blank input stored as <c>null</c>.
    /// </summary>
    public static string? NormalizeCaption(string? caption) =>
        string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
}

/// <summary>
/// Collection helpers for <see cref="StepMediaItem"/>.
/// </summary>
public static class StepMediaItemExtensions
{
    /// <summary>
    /// Copies each item into a new list; a null source yields an empty list.
    /// </summary>
    public static List<StepMediaItem> CloneItems(this IEnumerable<StepMediaItem>? items) =>
        items?.Select(i => i.Clone()).ToList() ?? [];

    /// <summary>
    /// Copies each item with its caption normalized (trimmed, blank → null) and capped at
    /// <see cref="StepMediaItem.CaptionMaxLength"/>. Used wherever editor input is persisted.
    /// </summary>
    public static List<StepMediaItem> ToPersistableItems(this IEnumerable<StepMediaItem>? items) =>
        items?.Select(i =>
        {
            var caption = StepMediaItem.NormalizeCaption(i.Caption);
            if (caption is { Length: > StepMediaItem.CaptionMaxLength })
                caption = caption[..StepMediaItem.CaptionMaxLength].TrimEnd();
            return new StepMediaItem { Path = i.Path, Caption = caption };
        }).ToList() ?? [];
}

/// <summary>
/// Validator for <see cref="StepMediaItem"/>: the caption is optional but limited in length.
/// </summary>
public class StepMediaItemValidator : AbstractValidator<StepMediaItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepMediaItemValidator"/> class.
    /// </summary>
    public StepMediaItemValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Image path is required.");

        RuleFor(x => x.Caption)
            .MaximumLength(StepMediaItem.CaptionMaxLength)
            .WithMessage($"Photo caption must be {StepMediaItem.CaptionMaxLength} characters or fewer.");
    }
}
