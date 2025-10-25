using QRCoder;

namespace buildone.Services;

public interface IQRCodeService
{
    /// <summary>
    /// Generates a QR code for an asset
    /// </summary>
    /// <param name="assetTag">Asset tag to encode</param>
    /// <param name="size">QR code size in pixels (default: 300)</param>
    /// <returns>PNG image as byte array</returns>
    byte[] GenerateAssetQRCode(string assetTag, int size = 300);

    /// <summary>
    /// Generates a QR code as base64 string for inline display
    /// </summary>
    /// <param name="assetTag">Asset tag to encode</param>
    /// <param name="size">QR code size in pixels (default: 300)</param>
    /// <returns>Base64 encoded image string</returns>
    string GenerateAssetQRCodeBase64(string assetTag, int size = 300);

    /// <summary>
    /// Generates QR code with URL to asset details page
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="baseUrl">Base URL of the application</param>
    /// <param name="size">QR code size in pixels</param>
    /// <returns>PNG image as byte array</returns>
    byte[] GenerateAssetURLQRCode(int assetId, string baseUrl, int size = 300);
}

public class QRCodeService : IQRCodeService
{
    private readonly ILogger<QRCodeService> _logger;

    public QRCodeService(ILogger<QRCodeService> logger)
    {
        _logger = logger;
    }

    public byte[] GenerateAssetQRCode(string assetTag, int size = 300)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(assetTag, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            var pixelsPerModule = Math.Max(1, size / 25); // Calculate pixels per module based on desired size
            return qrCode.GetGraphic(pixelsPerModule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for asset: {AssetTag}", assetTag);
            throw;
        }
    }

    public string GenerateAssetQRCodeBase64(string assetTag, int size = 300)
    {
        try
        {
            var qrCodeBytes = GenerateAssetQRCode(assetTag, size);
            return Convert.ToBase64String(qrCodeBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating base64 QR code for asset: {AssetTag}", assetTag);
            throw;
        }
    }

    public byte[] GenerateAssetURLQRCode(int assetId, string baseUrl, int size = 300)
    {
        try
        {
            var url = $"{baseUrl.TrimEnd('/')}/Assets/Details/{assetId}";
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            var pixelsPerModule = Math.Max(1, size / 25);
            return qrCode.GetGraphic(pixelsPerModule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating URL QR code for asset ID: {AssetId}", assetId);
            throw;
        }
    }
}
