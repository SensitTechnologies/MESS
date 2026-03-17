using QRCoder;

namespace MESS.Services.UI.QrCodes;

using Microsoft.JSInterop;
using QRCoder;

/// <inheritdoc/>
public class QrCodeService : IQrCodeService
{
    private readonly IJSRuntime _js;

    /// <summary>
    /// Initializes a new instance of the <see cref="QrCodeService"/> class.
    /// </summary>
    /// <param name="js">The <see cref="IJSRuntime"/> instance used to invoke JavaScript functions for printing QR codes.</param>
    /// <remarks>
    /// This constructor injects the JavaScript runtime dependency, allowing the service to call
    /// the client-side <c>printQRCode</c> function to render and print QR codes from Blazor components or other services.
    /// </remarks>
    public QrCodeService(IJSRuntime js)
    {
        _js = js;
    }

    /// <inheritdoc/>
    public async Task PrintAsync(string content, string label)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        // Generate QR
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        var dataUrl = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

        // Call JS print
        await _js.InvokeVoidAsync("printQRCode", dataUrl, label);
    }
}