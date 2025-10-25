using System.Globalization;
using ClosedXML.Excel;
using buildone.Data;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class ExcelDataSeedingService : IExcelDataSeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExcelDataSeedingService> _logger;

    public ExcelDataSeedingService(ApplicationDbContext context, ILogger<ExcelDataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(int Departments, int Employees, int Assets, int Inventory)> SeedFromExcelAsync(System.IO.Stream stream, string? uploadedBy = null)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        using var workbook = new XLWorkbook(stream);

        int deptCount = 0, empCount = 0, assetCount = 0, inventoryCount = 0;

        // Departments sheet
        if (workbook.Worksheets.Any(ws => string.Equals(ws.Name, "Departments", StringComparison.OrdinalIgnoreCase)))
        {
            var ws = workbook.Worksheets.First(w => string.Equals(w.Name, "Departments", StringComparison.OrdinalIgnoreCase));
            deptCount = await ProcessDepartmentsWorksheetAsync(ws);
        }

        // Employees sheet
        if (workbook.Worksheets.Any(ws => string.Equals(ws.Name, "Employees", StringComparison.OrdinalIgnoreCase)))
        {
            var ws = workbook.Worksheets.First(w => string.Equals(w.Name, "Employees", StringComparison.OrdinalIgnoreCase));
            empCount = await ProcessEmployeesWorksheetAsync(ws);
        }

        // Assets sheet
        if (workbook.Worksheets.Any(ws => string.Equals(ws.Name, "Assets", StringComparison.OrdinalIgnoreCase)))
        {
            var ws = workbook.Worksheets.First(w => string.Equals(w.Name, "Assets", StringComparison.OrdinalIgnoreCase));
            assetCount = await ProcessAssetsWorksheetAsync(ws);
        }

        // Inventory sheet
        if (workbook.Worksheets.Any(ws => string.Equals(ws.Name, "Inventory", StringComparison.OrdinalIgnoreCase)))
        {
            var ws = workbook.Worksheets.First(w => string.Equals(w.Name, "Inventory", StringComparison.OrdinalIgnoreCase));
            inventoryCount = await ProcessInventoryWorksheetAsync(ws);
        }

        _logger.LogInformation("Excel seed completed by {User}: Depts={Depts}, Emps={Emps}, Assets={Assets}, Inventory={Inv}", uploadedBy ?? "System", deptCount, empCount, assetCount, inventoryCount);

        return (deptCount, empCount, assetCount, inventoryCount);
    }

    private async Task<int> ProcessDepartmentsWorksheetAsync(IXLWorksheet ws)
    {
        var headerMap = BuildHeaderMap(ws);
        if (!headerMap.Any()) return 0;

        var created = 0;
        var rows = ws.RowsUsed().Skip(1);
        foreach (var r in rows)
        {
            var name = GetCellString(r, headerMap, "Name") ?? GetCellString(r, headerMap, "Department");
            var code = GetCellString(r, headerMap, "Code");
            if (string.IsNullOrWhiteSpace(name)) continue;

            // Avoid duplicates by code or name
            var exists = await _context.Departments.AnyAsync(d => (!string.IsNullOrEmpty(code) && d.Code == code) || d.Name == name);
            if (exists) continue;

            var dept = new Department { Name = name.Trim(), Code = code?.Trim() };
            _context.Departments.Add(dept);
            created++;
        }

        if (created > 0) await _context.SaveChangesAsync();
        return created;
    }

    private async Task<int> ProcessEmployeesWorksheetAsync(IXLWorksheet ws)
    {
        var headerMap = BuildHeaderMap(ws);
        if (!headerMap.Any()) return 0;

        var created = 0;
        var rows = ws.RowsUsed().Skip(1);
        foreach (var r in rows)
        {
            var fullName = GetCellString(r, headerMap, "FullName") ?? GetCellString(r, headerMap, "Name");
            var username = GetCellString(r, headerMap, "Username");
            var email = GetCellString(r, headerMap, "Email");
            var phone = GetCellString(r, headerMap, "Phone");
            var deptCode = GetCellString(r, headerMap, "DepartmentCode") ?? GetCellString(r, headerMap, "Department");

            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email)) continue;

            // Avoid duplicates by username or email
            var exists = await _context.Employees.AnyAsync(e => (!string.IsNullOrEmpty(username) && e.Username == username) || (!string.IsNullOrEmpty(email) && e.Email == email));
            if (exists) continue;

            int? departmentId = null;
            if (!string.IsNullOrWhiteSpace(deptCode))
            {
                var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == deptCode || d.Name == deptCode);
                if (dept != null) departmentId = dept.Id;
            }

            var emp = new Employee
            {
                FullName = fullName?.Trim() ?? username,
                Username = username?.Trim() ?? email?.Split('@').First(),
                Email = email?.Trim(),
                Phone = phone?.Trim(),
                DepartmentId = departmentId
            };

            _context.Employees.Add(emp);
            created++;
        }

        if (created > 0) await _context.SaveChangesAsync();
        return created;
    }

    private async Task<int> ProcessAssetsWorksheetAsync(IXLWorksheet ws)
    {
        var headerMap = BuildHeaderMap(ws);
        if (!headerMap.Any()) return 0;

        var created = 0;
        var rows = ws.RowsUsed().Skip(1);
        foreach (var r in rows)
        {
            var assetTag = GetCellString(r, headerMap, "AssetTag");
            var pcId = GetCellString(r, headerMap, "PcId") ?? GetCellString(r, headerMap, "PCID");
            var brand = GetCellString(r, headerMap, "Brand");
            var model = GetCellString(r, headerMap, "Model");
            var serial = GetCellString(r, headerMap, "SerialNumber") ?? GetCellString(r, headerMap, "Serial");
            var type = GetCellString(r, headerMap, "Type") ?? GetCellString(r, headerMap, "Category");
            var assignedUsername = GetCellString(r, headerMap, "AssignedTo") ?? GetCellString(r, headerMap, "AssignedUsername");

            if (string.IsNullOrWhiteSpace(assetTag) && string.IsNullOrWhiteSpace(serial)) continue;

            var exists = await _context.Assets.AnyAsync(a => (!string.IsNullOrEmpty(assetTag) && a.AssetTag == assetTag) || (!string.IsNullOrEmpty(serial) && a.SerialNumber == serial));
            if (exists) continue;

            int? assignedEmployeeId = null;
            if (!string.IsNullOrWhiteSpace(assignedUsername))
            {
                var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Username == assignedUsername || e.Email == assignedUsername);
                if (emp != null) assignedEmployeeId = emp.Id;
            }

            var asset = new Asset
            {
                AssetTag = assetTag?.Trim(),
                PcId = pcId?.Trim(),
                Brand = brand?.Trim(),
                Model = model?.Trim(),
                SerialNumber = serial?.Trim(),
                Type = type?.Trim(),
                AssignedEmployeeId = assignedEmployeeId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            created++;
        }

        if (created > 0) await _context.SaveChangesAsync();
        return created;
    }

    private async Task<int> ProcessInventoryWorksheetAsync(IXLWorksheet ws)
    {
        var headerMap = BuildHeaderMap(ws);
        if (!headerMap.Any()) return 0;

        var created = 0;
        var rows = ws.RowsUsed().Skip(1);
        foreach (var r in rows)
        {
            var itemName = GetCellString(r, headerMap, "ItemName") ?? GetCellString(r, headerMap, "Name");
            if (string.IsNullOrWhiteSpace(itemName)) continue;

            var sku = GetCellString(r, headerMap, "SKU");
            var description = GetCellString(r, headerMap, "Description");
            var category = GetCellString(r, headerMap, "Category");
            var unit = GetCellString(r, headerMap, "Unit");
            var supplier = GetCellString(r, headerMap, "Supplier");

            var currentQty = ParseInt(GetCellString(r, headerMap, "CurrentQuantity")) ?? 0;
            var minQty = ParseInt(GetCellString(r, headerMap, "MinimumQuantity")) ?? 0;
            var maxQty = ParseInt(GetCellString(r, headerMap, "MaximumQuantity")) ?? 0;
            var warrantyMonths = ParseInt(GetCellString(r, headerMap, "WarrantyPeriodMonths"));
            var warrantyStart = ParseDate(GetCellString(r, headerMap, "WarrantyStartDate"));
            var warrantyEnd = ParseDate(GetCellString(r, headerMap, "WarrantyEndDate"));

            // Avoid duplicates by SKU or ItemName
            var exists = await _context.Inventories.AnyAsync(i => (!string.IsNullOrEmpty(sku) && i.SKU == sku) || i.ItemName == itemName);
            if (exists) continue;

            var inv = new Data.Inventory
            {
                ItemName = itemName.Trim(),
                SKU = sku?.Trim() ?? $"INV-{Guid.NewGuid().ToString().Substring(0,8).ToUpper()}",
                Description = description?.Trim(),
                Category = category?.Trim(),
                Unit = unit?.Trim(),
                Supplier = supplier?.Trim(),
                CurrentQuantity = currentQty,
                MinimumQuantity = minQty,
                MaximumQuantity = maxQty,
                WarrantyPeriodMonths = warrantyMonths,
                WarrantyStartDate = warrantyStart,
                WarrantyEndDate = warrantyEnd,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                UpdatedBy = "ExcelImport"
            };

            // Set StockStatus conservatively; it may be recalculated elsewhere
            inv.StockStatus = currentQty <= 0 ? Data.Enums.StockStatus.OutOfStock : Data.Enums.StockStatus.InStock;

            _context.Inventories.Add(inv);
            created++;
        }

        if (created > 0) await _context.SaveChangesAsync();
        return created;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var header = ws.FirstRowUsed();
        if (header == null) return map;

        int col = 1;
        foreach (var cell in header.Cells())
        {
            var text = cell.GetString();
            if (!string.IsNullOrWhiteSpace(text) && !map.ContainsKey(text.Trim()))
            {
                map[text.Trim()] = col;
            }
            col++;
        }

        return map;
    }

    private static string? GetCellString(IXLRow row, Dictionary<string,int> headerMap, string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            if (headerMap.TryGetValue(name, out var idx))
            {
                return row.Cell(idx).GetString();
            }
        }
        return null;
    }

    private static string? GetCellString(IXLRow row, Dictionary<string,int> headerMap, string name)
        => GetCellString(row, headerMap, new[] { name });

    private static int? ParseInt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
        return null;
    }

    private static DateTime? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s!.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d)) return d;
        // Try parsing as Excel date serial if numeric
        if (double.TryParse(s!.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var serial))
        {
            try
            {
                return DateTime.FromOADate(serial);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }
}
