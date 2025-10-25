using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buildone.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class QRCodeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IQRCodeService _qrCodeService;
    private readonly ILogger<QRCodeController> _logger;

    public QRCodeController(
        ApplicationDbContext context,
        IQRCodeService qrCodeService,
        ILogger<QRCodeController> logger)
    {
        _context = context;
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    /// <summary>
    /// Generate QR code for an asset by Asset ID
    /// </summary>
    [HttpGet("asset/{assetId}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateAssetQRCode(int assetId, [FromQuery] int size = 300)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        try
        {
            var qrCodeBytes = _qrCodeService.GenerateAssetQRCode(asset.AssetTag, size);
            return File(qrCodeBytes, "image/png", $"QR-{asset.AssetTag}.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Failed to generate QR code" });
        }
    }

    /// <summary>
    /// Generate QR code with URL to asset details page
    /// </summary>
    [HttpGet("asset/{assetId}/generate-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateAssetURLQRCode(int assetId, [FromQuery] int size = 300)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var qrCodeBytes = _qrCodeService.GenerateAssetURLQRCode(assetId, baseUrl, size);
            return File(qrCodeBytes, "image/png", $"QR-URL-{asset.AssetTag}.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating URL QR code for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Failed to generate QR code" });
        }
    }

    /// <summary>
    /// Get QR code as base64 string for inline display
    /// </summary>
    [HttpGet("asset/{assetId}/base64")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetQRCodeBase64(int assetId, [FromQuery] int size = 300)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        try
        {
            var base64 = _qrCodeService.GenerateAssetQRCodeBase64(asset.AssetTag, size);
            return Ok(new
            {
                assetTag = asset.AssetTag,
                qrCode = $"data:image/png;base64,{base64}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating base64 QR code for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Failed to generate QR code" });
        }
    }

    /// <summary>
    /// Lookup asset by QR code scan (asset tag or URL)
    /// </summary>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupAssetByQRCode([FromBody] QRCodeLookupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ScannedData))
            return BadRequest(new { error = "Scanned data is required" });

        try
        {
            // Try to extract asset tag from scanned data
            string? assetTag = null;

            // Check if it's a URL
            if (Uri.TryCreate(request.ScannedData, UriKind.Absolute, out var uri))
            {
                // Extract asset ID from URL path like /Assets/Details/123
                var path = uri.AbsolutePath ?? string.Empty;
                // Try Assets URL first: /Assets/Details/{id}
                if (path.Contains("/Assets/Details/", StringComparison.OrdinalIgnoreCase))
                {
                    var idStr = path.Substring(path.IndexOf("/Assets/Details/", StringComparison.OrdinalIgnoreCase) + "/Assets/Details/".Length).Trim('/');
                    if (int.TryParse(idStr, out var assetId))
                    {
                        var assetById = await _context.Assets
                            .Include(a => a.AssignedEmployee)
                            .Include(a => a.Department)
                            .FirstOrDefaultAsync(a => a.Id == assetId);

                        if (assetById != null)
                        {
                            return Ok(new
                            {
                                asset = MapAssetToResponse(assetById),
                                lookupMethod = "url"
                            });
                        }
                    }
                }

                // Try Inventory URL: /Inventory/Details/{id}
                if (path.Contains("/Inventory/Details/", StringComparison.OrdinalIgnoreCase))
                {
                    var idStr = path.Substring(path.IndexOf("/Inventory/Details/", StringComparison.OrdinalIgnoreCase) + "/Inventory/Details/".Length).Trim('/');
                    if (int.TryParse(idStr, out var invId))
                    {
                        var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == invId);
                        if (inv != null)
                        {
                            return Ok(new
                            {
                                asset = MapInventoryToResponse(inv),
                                lookupMethod = "inventory_url"
                            });
                        }
                    }
                }
            }

            // Treat as asset tag
            assetTag = request.ScannedData.Trim();
            var asset = await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.AssetTag == assetTag);

            if (asset == null)
            {
                // Try inventory lookup by SKU or ItemName
                var inv = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.SKU == assetTag || i.ItemName == assetTag);

                if (inv != null)
                {
                    return Ok(new
                    {
                        asset = MapInventoryToResponse(inv),
                        lookupMethod = "inventory"
                    });
                }

                return NotFound(new { error = "Asset not found", scannedData = request.ScannedData });
            }

            return Ok(new
            {
                asset = MapAssetToResponse(asset),
                lookupMethod = "assetTag"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up asset by QR code: {ScannedData}", request.ScannedData);
            return StatusCode(500, new { error = "Failed to lookup asset" });
        }
    }

    private object MapInventoryToResponse(Inventory inv)
    {
        // Normalize inventory response to match asset response shape used by the scanner UI
        return new
        {
            Id = inv.Id,
            AssetTag = inv.SKU ?? $"INV-{inv.Id}",
            PcId = (string?)null,
            Brand = (string?)null,
            Model = inv.ItemName,
            SerialNumber = (string?)null,
            Type = "Inventory",
            Status = inv.StockStatus,
            WarrantyExpiry = inv.WarrantyEndDate,
            AssignedTo = (object?)null,
            Department = (object?)null,
            DetailsUrl = $"/Inventory/Details/{inv.Id}"
        };
    }

    private object MapAssetToResponse(Asset asset)
    {
        return new
        {
            asset.Id,
            asset.AssetTag,
            asset.PcId,
            asset.Brand,
            asset.Model,
            asset.SerialNumber,
            asset.Type,
            asset.Status,
            asset.WarrantyExpiry,
            AssignedTo = asset.AssignedEmployee != null ? new
            {
                asset.AssignedEmployee.Id,
                asset.AssignedEmployee.FullName,
                asset.AssignedEmployee.Email
            } : null,
            Department = asset.Department != null ? new
            {
                asset.Department.Id,
                asset.Department.Name,
                asset.Department.Code
            } : null,
            DetailsUrl = $"/Assets/Details/{asset.Id}"
        };
    }
}

public class QRCodeLookupRequest
{
    public string ScannedData { get; set; } = string.Empty;
}
