using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using buildone.Data;
using buildone.Data.Enums;
using buildone.DTOs;

namespace buildone.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetInventories(
        [FromQuery] string? searchTerm,
        [FromQuery] string? category,
        [FromQuery] string? stockStatus)
    {
        try
        {
            var query = _context.Inventories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(i => i.ItemName.Contains(searchTerm) ||
                                        i.SKU!.Contains(searchTerm) ||
                                        i.Description!.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                if (Enum.TryParse<StockStatus>(stockStatus, out var status))
                {
                    query = query.Where(i => i.StockStatus == status);
                }
            }

            var inventories = await query
                .OrderBy(i => i.ItemName)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    SKU = i.SKU,
                    Description = i.Description,
                    Category = i.Category,
                    CurrentQuantity = i.CurrentQuantity,
                    MinimumQuantity = i.MinimumQuantity,
                    MaximumQuantity = i.MaximumQuantity,
                    UnitPrice = i.UnitPrice,
                    Unit = i.Unit,
                    StockStatus = i.StockStatus.ToString(),
                    StorageLocation = i.StorageLocation,
                    Supplier = i.Supplier,
                    LastRestocked = i.LastRestocked,
                    IsLowStock = i.IsLowStock,
                    IsOutOfStock = i.IsOutOfStock,
                    IsOverstocked = i.IsOverstocked
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<InventoryDto>>
            {
                Success = true,
                Data = inventories,
                Message = $"Retrieved {inventories.Count} inventory items"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventories");
            return StatusCode(500, new ApiResponse<IEnumerable<InventoryDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving inventories"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetInventory(int id)
    {
        try
        {
            var inventory = await _context.Inventories
                .Where(i => i.Id == id)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    SKU = i.SKU,
                    Description = i.Description,
                    Category = i.Category,
                    CurrentQuantity = i.CurrentQuantity,
                    MinimumQuantity = i.MinimumQuantity,
                    MaximumQuantity = i.MaximumQuantity,
                    UnitPrice = i.UnitPrice,
                    Unit = i.Unit,
                    StockStatus = i.StockStatus.ToString(),
                    StorageLocation = i.StorageLocation,
                    Supplier = i.Supplier,
                    LastRestocked = i.LastRestocked,
                    IsLowStock = i.IsLowStock,
                    IsOutOfStock = i.IsOutOfStock,
                    IsOverstocked = i.IsOverstocked
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Inventory item not found"
                });
            }

            return Ok(new ApiResponse<InventoryDto>
            {
                Success = true,
                Data = inventory,
                Message = "Inventory item retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory {Id}", id);
            return StatusCode(500, new ApiResponse<InventoryDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the inventory item"
            });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<InventoryStatisticsDto>>> GetStatistics()
    {
        try
        {
            var totalItems = await _context.Inventories.CountAsync();
            var outOfStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity <= 0);
            var lowStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity > 0 && i.CurrentQuantity <= i.MinimumQuantity);
            var inStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity > i.MinimumQuantity && i.CurrentQuantity < i.MaximumQuantity);
            var fullyStocked = await _context.Inventories.CountAsync(i => i.CurrentQuantity >= i.MaximumQuantity);
            var totalValue = await _context.Inventories
                .Where(i => i.UnitPrice.HasValue)
                .SumAsync(i => i.CurrentQuantity * i.UnitPrice!.Value);

            var statistics = new InventoryStatisticsDto
            {
                TotalItems = totalItems,
                OutOfStockItems = outOfStock,
                LowStockItems = lowStock,
                InStockItems = inStock,
                FullyStockedItems = fullyStocked,
                TotalValue = totalValue
            };

            return Ok(new ApiResponse<InventoryStatisticsDto>
            {
                Success = true,
                Data = statistics,
                Message = "Statistics retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory statistics");
            return StatusCode(500, new ApiResponse<InventoryStatisticsDto>
            {
                Success = false,
                Message = "An error occurred while retrieving statistics"
            });
        }
    }

    [HttpPost("restock/{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> RestockInventory(int id, [FromBody] RestockInventoryDto dto)
    {
        try
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Inventory item not found"
                });
            }

            var previousQuantity = inventory.CurrentQuantity;
            inventory.CurrentQuantity += dto.Quantity;
            inventory.LastRestocked = DateTime.UtcNow;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.UpdatedBy = User.Identity?.Name;

            // Update stock status
            inventory.StockStatus = DetermineStockStatus(inventory);

            // Record transaction
            var transaction = new InventoryTransaction
            {
                InventoryId = id,
                TransactionType = "Restock",
                Quantity = dto.Quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = inventory.CurrentQuantity,
                Remarks = dto.Remarks,
                Reference = dto.Reference,
                PerformedBy = User.Identity?.Name,
                TransactionDate = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory {Id} restocked by {Quantity}. Previous: {Previous}, New: {New}",
                id, dto.Quantity, previousQuantity, inventory.CurrentQuantity);

            var result = new InventoryDto
            {
                Id = inventory.Id,
                ItemName = inventory.ItemName,
                SKU = inventory.SKU,
                CurrentQuantity = inventory.CurrentQuantity,
                StockStatus = inventory.StockStatus.ToString(),
                IsLowStock = inventory.IsLowStock,
                IsOutOfStock = inventory.IsOutOfStock
            };

            return Ok(new ApiResponse<InventoryDto>
            {
                Success = true,
                Data = result,
                Message = $"Inventory restocked successfully. Added {dto.Quantity} {inventory.Unit}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking inventory {Id}", id);
            return StatusCode(500, new ApiResponse<InventoryDto>
            {
                Success = false,
                Message = "An error occurred while restocking inventory"
            });
        }
    }

    [HttpPost("adjust/{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> AdjustInventory(int id, [FromBody] AdjustInventoryDto dto)
    {
        try
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Inventory item not found"
                });
            }

            var previousQuantity = inventory.CurrentQuantity;
            var difference = dto.NewQuantity - previousQuantity;
            
            inventory.CurrentQuantity = dto.NewQuantity;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.UpdatedBy = User.Identity?.Name;

            // Update stock status
            inventory.StockStatus = DetermineStockStatus(inventory);

            // Record transaction
            var transaction = new InventoryTransaction
            {
                InventoryId = id,
                TransactionType = "Adjustment",
                Quantity = difference,
                PreviousQuantity = previousQuantity,
                NewQuantity = inventory.CurrentQuantity,
                Remarks = dto.Remarks,
                PerformedBy = User.Identity?.Name,
                TransactionDate = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory {Id} adjusted. Previous: {Previous}, New: {New}, Difference: {Diff}",
                id, previousQuantity, inventory.CurrentQuantity, difference);

            var result = new InventoryDto
            {
                Id = inventory.Id,
                ItemName = inventory.ItemName,
                SKU = inventory.SKU,
                CurrentQuantity = inventory.CurrentQuantity,
                StockStatus = inventory.StockStatus.ToString(),
                IsLowStock = inventory.IsLowStock,
                IsOutOfStock = inventory.IsOutOfStock
            };

            return Ok(new ApiResponse<InventoryDto>
            {
                Success = true,
                Data = result,
                Message = "Inventory adjusted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory {Id}", id);
            return StatusCode(500, new ApiResponse<InventoryDto>
            {
                Success = false,
                Message = "An error occurred while adjusting inventory"
            });
        }
    }

    [HttpPost("withdraw/{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> WithdrawInventory(int id, [FromBody] WithdrawInventoryDto dto)
    {
        try
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Inventory item not found"
                });
            }

            // Validate quantity
            if (dto.Quantity <= 0)
            {
                return BadRequest(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Withdrawal quantity must be greater than zero"
                });
            }

            if (dto.Quantity > inventory.CurrentQuantity)
            {
                return BadRequest(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = $"Cannot withdraw {dto.Quantity} {inventory.Unit}. Only {inventory.CurrentQuantity} {inventory.Unit} available in stock"
                });
            }

            // Validate remarks (required for audit)
            if (string.IsNullOrWhiteSpace(dto.Remarks))
            {
                return BadRequest(new ApiResponse<InventoryDto>
                {
                    Success = false,
                    Message = "Description/Note is required for withdrawal audit trail"
                });
            }

            var previousQuantity = inventory.CurrentQuantity;
            inventory.CurrentQuantity -= dto.Quantity;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.UpdatedBy = User.Identity?.Name;

            // Update stock status
            inventory.StockStatus = DetermineStockStatus(inventory);

            // Record transaction with negative quantity to indicate withdrawal
            var transaction = new InventoryTransaction
            {
                InventoryId = id,
                TransactionType = "Withdrawal",
                Quantity = -dto.Quantity, // Negative to indicate withdrawal
                PreviousQuantity = previousQuantity,
                NewQuantity = inventory.CurrentQuantity,
                Remarks = dto.Remarks,
                PerformedBy = User.Identity?.Name,
                TransactionDate = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory {Id} withdrawal. User: {User}, Quantity: {Qty}, Remaining: {Remaining}",
                id, User.Identity?.Name, dto.Quantity, inventory.CurrentQuantity);

            var result = new InventoryDto
            {
                Id = inventory.Id,
                ItemName = inventory.ItemName,
                SKU = inventory.SKU,
                CurrentQuantity = inventory.CurrentQuantity,
                StockStatus = inventory.StockStatus.ToString(),
                IsLowStock = inventory.IsLowStock,
                IsOutOfStock = inventory.IsOutOfStock
            };

            return Ok(new ApiResponse<InventoryDto>
            {
                Success = true,
                Data = result,
                Message = $"Successfully withdrew {dto.Quantity} {inventory.Unit}. Remaining stock: {inventory.CurrentQuantity} {inventory.Unit}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing from inventory {Id}", id);
            return StatusCode(500, new ApiResponse<InventoryDto>
            {
                Success = false,
                Message = "An error occurred while withdrawing from inventory"
            });
        }
    }

    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetTransactions(int id)
    {
        try
        {
            var transactions = await _context.InventoryTransactions
                .Where(t => t.InventoryId == id)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new
                {
                    t.Id,
                    t.TransactionType,
                    t.Quantity,
                    t.PreviousQuantity,
                    t.NewQuantity,
                    t.Remarks,
                    t.Reference,
                    t.PerformedBy,
                    t.TransactionDate
                })
                .Take(50)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Data = transactions,
                Message = $"Retrieved {transactions.Count} transactions"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for inventory {Id}", id);
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "An error occurred while retrieving transactions"
            });
        }
    }

    private static StockStatus DetermineStockStatus(Inventory inventory)
    {
        if (inventory.CurrentQuantity <= 0)
            return StockStatus.OutOfStock;
        
        if (inventory.CurrentQuantity <= inventory.MinimumQuantity)
            return StockStatus.LowStock;
        
        if (inventory.CurrentQuantity >= inventory.MaximumQuantity)
            return StockStatus.Overstocked;
        
        if (inventory.CurrentQuantity >= (inventory.MaximumQuantity * 0.8))
            return StockStatus.FullyStocked;
        
        return StockStatus.InStock;
    }
}
