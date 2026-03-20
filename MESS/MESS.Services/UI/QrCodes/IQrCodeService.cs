namespace MESS.Services.UI.QrCodes;

/// <summary>
/// Provides functionality to generate and print QR codes with an optional label.
/// This service can be used to print QR codes for production logs, tags, work instructions, or any other string content.
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Generates a QR code for the specified <paramref name="content"/> and sends it to the printer
    /// with an optional <paramref name="label"/> displayed below the QR code.
    /// </summary>
    /// <param name="content">The string value to encode in the QR code. Must not be <c>null</c> or empty.</param>
    /// <param name="label">The text to display below the QR code. Can be <c>null</c> if no label is needed.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous print operation.</returns>
    /// <remarks>
    /// This method relies on a JavaScript interop call to handle the printing of the QR code.
    /// Any UI components can invoke this service without depending on a specific Blazor component.
    /// </remarks>
    Task PrintAsync(string content, string label);
}