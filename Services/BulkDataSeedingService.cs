using buildone.Data;
using buildone.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public interface IBulkDataSeedingService
{
    Task SeedBulkDataAsync();
    Task<(int Departments, int Employees, int Assets, int ImagingJobs)> GetSeedingStatisticsAsync();
}

public class BulkDataSeedingService : IBulkDataSeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BulkDataSeedingService> _logger;
    private readonly Random _random = new();

    // Realistic data for generation
    private readonly string[] _firstNames = 
    {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Barbara", "David", "Elizabeth", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
        "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley",
        "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle",
        "Kenneth", "Dorothy", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa",
        "Edward", "Deborah", "Ronald", "Stephanie", "Timothy", "Rebecca", "Jason", "Sharon",
        "Jeffrey", "Laura", "Ryan", "Cynthia", "Jacob", "Kathleen", "Gary", "Amy"
    };

    private readonly string[] _lastNames = 
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
        "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell",
        "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker"
    };

    private readonly string[] _laptopBrands = { "Dell", "HP", "Lenovo", "Apple", "Microsoft", "Asus" };
    private readonly string[] _desktopBrands = { "Dell", "HP", "Lenovo", "Acer" };
    private readonly string[] _tabletBrands = { "Apple", "Samsung", "Microsoft", "Lenovo" };

    private readonly Dictionary<string, string[]> _brandModels = new()
    {
        ["Dell"] = new[] { "Latitude 5420", "Latitude 7420", "OptiPlex 7090", "OptiPlex 3080", "Precision 5560" },
        ["HP"] = new[] { "EliteBook 840", "ProBook 450", "EliteDesk 800", "ProDesk 400", "ZBook 15" },
        ["Lenovo"] = new[] { "ThinkPad X1 Carbon", "ThinkPad T14", "ThinkCentre M90", "IdeaPad Duet", "Tab P11" },
        ["Apple"] = new[] { "MacBook Pro 14", "MacBook Air M2", "iPad Pro 12.9", "iPad Air" },
        ["Microsoft"] = new[] { "Surface Laptop 5", "Surface Pro 9", "Surface Go 3" },
        ["Asus"] = new[] { "ZenBook 14", "VivoBook 15", "ROG Flow" },
        ["Acer"] = new[] { "Aspire 5", "Veriton Desktop" },
        ["Samsung"] = new[] { "Galaxy Tab S8", "Galaxy Tab A7" }
    };

    private readonly string[] _imageVersions = 
    {
        "WIN11-PRO-23H2-v1", "WIN11-PRO-23H2-v2", "WIN11-ENT-23H2-v1", 
        "WIN10-PRO-22H2-v3", "WIN11-PRO-22H2-v5", "WIN11-ENT-22H2-v4"
    };

    public BulkDataSeedingService(
        ApplicationDbContext context,
        ILogger<BulkDataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting bulk data seeding process...");

            // Seed in correct order due to foreign key constraints
            var departments = await SeedDepartmentsAsync();
            _logger.LogInformation("Seeded {Count} departments", departments.Count);

            var employees = await SeedEmployeesAsync(departments);
            _logger.LogInformation("Seeded {Count} employees", employees.Count);

            var assets = await SeedAssetsAsync(employees, departments);
            _logger.LogInformation("Seeded {Count} assets", assets.Count);

            var imagingJobs = await SeedImagingJobsAsync(assets, employees);
            _logger.LogInformation("Seeded {Count} imaging jobs", imagingJobs.Count);

            var inventory = await SeedInventoryAsync();
            _logger.LogInformation("Seeded {Count} inventory items", inventory.Count);

            _logger.LogInformation("Bulk data seeding completed successfully!");
            _logger.LogInformation("Summary: {Depts} departments, {Emps} employees, {Assets} assets, {Jobs} imaging jobs, {Inv} inventory items",
                departments.Count, employees.Count, assets.Count, imagingJobs.Count, inventory.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during bulk data seeding");
            throw;
        }
    }

    private async Task<List<Department>> SeedDepartmentsAsync()
    {
        var existingDepartments = await _context.Departments.ToListAsync();
        
        if (existingDepartments.Any())
        {
            _logger.LogInformation("Departments already exist, skipping department seeding");
            return existingDepartments;
        }

        var departments = new List<Department>
        {
            new() { Name = "Information Technology", Code = "IT" },
            new() { Name = "Human Resources", Code = "HR" },
            new() { Name = "Finance", Code = "FIN" },
            new() { Name = "Operations", Code = "OPS" },
            new() { Name = "Sales", Code = "SALES" },
            new() { Name = "Marketing", Code = "MKT" },
            new() { Name = "Customer Support", Code = "CS" },
            new() { Name = "Research & Development", Code = "RD" }
        };

        await _context.Departments.AddRangeAsync(departments);
        await _context.SaveChangesAsync();

        return departments;
    }

    private async Task<List<Employee>> SeedEmployeesAsync(List<Department> departments)
    {
        var existingEmployeesCount = await _context.Employees.CountAsync();
        
        if (existingEmployeesCount >= 10)
        {
            _logger.LogInformation("Sufficient employees already exist, returning existing employees");
            return await _context.Employees.Take(10).ToListAsync();
        }

        var employees = new List<Employee>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < 10; i++)
        {
            string fullName;
            string username;
            
            do
            {
                var firstName = _firstNames[_random.Next(_firstNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];
                fullName = $"{firstName} {lastName}";
                username = $"{firstName.ToLower()}.{lastName.ToLower()}";
            } while (usedNames.Contains(username));

            usedNames.Add(username);

            var employee = new Employee
            {
                FullName = fullName,
                Username = username,
                Email = $"{username}@buildone.com",
                Phone = GeneratePhoneNumber(),
                DepartmentId = departments[_random.Next(departments.Count)].Id
            };

            employees.Add(employee);
        }

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        return employees;
    }

    private async Task<List<Asset>> SeedAssetsAsync(List<Employee> employees, List<Department> departments)
    {
        var existingAssetsCount = await _context.Assets.CountAsync();
        
        if (existingAssetsCount >= 400)
        {
            _logger.LogInformation("Sufficient assets already exist, returning existing assets");
            return await _context.Assets.Take(400).ToListAsync();
        }

        var assets = new List<Asset>();
        var assetCounter = existingAssetsCount + 1;

        // Distribution: 50% laptops, 35% desktops, 15% tablets
        var laptopCount = 200;
        var desktopCount = 140;
        var tabletCount = 60;

        // Generate Laptops
        for (int i = 0; i < laptopCount; i++)
        {
            var brand = _laptopBrands[_random.Next(_laptopBrands.Length)];
            var asset = CreateAsset("Laptop", brand, assetCounter++, employees, departments);
            assets.Add(asset);
        }

        // Generate Desktops
        for (int i = 0; i < desktopCount; i++)
        {
            var brand = _desktopBrands[_random.Next(_desktopBrands.Length)];
            var asset = CreateAsset("Desktop", brand, assetCounter++, employees, departments);
            assets.Add(asset);
        }

        // Generate Tablets
        for (int i = 0; i < tabletCount; i++)
        {
            var brand = _tabletBrands[_random.Next(_tabletBrands.Length)];
            var asset = CreateAsset("Tablet", brand, assetCounter++, employees, departments);
            assets.Add(asset);
        }

        await _context.Assets.AddRangeAsync(assets);
        await _context.SaveChangesAsync();

        return assets;
    }

    private Asset CreateAsset(string type, string brand, int counter, List<Employee> employees, List<Department> departments)
    {
        var employee = employees[_random.Next(employees.Count)];
        var models = _brandModels.ContainsKey(brand) ? _brandModels[brand] : new[] { "Standard Model" };
        var model = models[_random.Next(models.Length)];
        
        var assetTag = $"AST-{type[..3].ToUpper()}-{counter:D5}";
        var pcId = $"PC-{counter:D6}";
        var serialNumber = $"{brand.ToUpper()}{_random.Next(100000, 999999)}";

        var status = _random.Next(100) < 70 ? AssetStatus.Assigned : 
                     _random.Next(100) < 50 ? AssetStatus.Active : AssetStatus.InStock;

        return new Asset
        {
            AssetTag = assetTag,
            PcId = pcId,
            Brand = brand,
            Model = model,
            SerialNumber = serialNumber,
            Type = type,
            WarrantyExpiry = DateTime.UtcNow.AddMonths(_random.Next(12, 48)),
            ImagingType = (ImagingType)_random.Next(1, 4),
            DeploymentType = DeploymentType.InPlace,
            Status = status,
            AssignedEmployeeId = status == AssetStatus.Assigned ? employee.Id : null,
            DepartmentId = employee.DepartmentId,
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365)),
            Notes = _random.Next(100) < 30 ? GenerateRandomNote() : null
        };
    }

    private async Task<List<ImagingJob>> SeedImagingJobsAsync(List<Asset> assets, List<Employee> employees)
    {
        var existingJobsCount = await _context.ImagingJobs.CountAsync();
        
        if (existingJobsCount >= 400)
        {
            _logger.LogInformation("Sufficient imaging jobs already exist, skipping");
            return await _context.ImagingJobs.Take(400).ToListAsync();
        }

        var imagingJobs = new List<ImagingJob>();

        // Create imaging jobs for the first 400 assets
        var assetsForJobs = assets.Take(400).ToList();

        foreach (var asset in assetsForJobs)
        {
            var technician = employees[_random.Next(employees.Count)];
            var createdDaysAgo = _random.Next(1, 90);
            var createdAt = DateTime.UtcNow.AddDays(-createdDaysAgo);

            var status = GenerateJobStatus(createdDaysAgo);
            var priority = (JobPriority)_random.Next(0, 4);

            var job = new ImagingJob
            {
                AssetId = asset.Id,
                TechnicianId = technician.Id,
                ImagingType = asset.ImagingType,
                ImageVersion = _imageVersions[_random.Next(_imageVersions.Length)],
                Status = status,
                Priority = priority,
                CreatedAt = createdAt,
                Notes = _random.Next(100) < 40 ? GenerateJobNote(status) : null
            };

            // Set dates based on status
            switch (status)
            {
                case JobStatus.Scheduled:
                    job.ScheduledAt = DateTime.UtcNow.AddDays(_random.Next(1, 14));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 7));
                    job.EstimatedDurationMinutes = _random.Next(60, 180);
                    break;

                case JobStatus.Pending:
                    job.ScheduledAt = createdAt.AddHours(_random.Next(1, 48));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 5));
                    job.EstimatedDurationMinutes = _random.Next(60, 180);
                    break;

                case JobStatus.InProgress:
                    job.ScheduledAt = createdAt.AddHours(_random.Next(1, 24));
                    job.StartedAt = DateTime.UtcNow.AddHours(-_random.Next(1, 4));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 3));
                    job.EstimatedDurationMinutes = _random.Next(90, 180);
                    break;

                case JobStatus.Completed:
                    job.ScheduledAt = createdAt.AddHours(_random.Next(1, 24));
                    job.StartedAt = job.ScheduledAt.Value.AddHours(_random.Next(1, 6));
                    job.CompletedAt = job.StartedAt.Value.AddMinutes(_random.Next(60, 180));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 7));
                    job.EstimatedDurationMinutes = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalMinutes;
                    break;

                case JobStatus.Failed:
                    job.ScheduledAt = createdAt.AddHours(_random.Next(1, 24));
                    job.StartedAt = job.ScheduledAt.Value.AddHours(_random.Next(1, 6));
                    job.CompletedAt = job.StartedAt.Value.AddMinutes(_random.Next(30, 120));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 7));
                    job.EstimatedDurationMinutes = _random.Next(90, 180);
                    break;

                case JobStatus.Cancelled:
                    job.ScheduledAt = createdAt.AddHours(_random.Next(1, 24));
                    job.DueDate = job.ScheduledAt.Value.AddDays(_random.Next(1, 7));
                    job.EstimatedDurationMinutes = _random.Next(60, 180);
                    break;
            }

            imagingJobs.Add(job);
        }

        await _context.ImagingJobs.AddRangeAsync(imagingJobs);
        await _context.SaveChangesAsync();

        return imagingJobs;
    }

    private JobStatus GenerateJobStatus(int daysAgo)
    {
        // More recent jobs are more likely to be in progress or pending
        if (daysAgo <= 7)
        {
            var rand = _random.Next(100);
            return rand switch
            {
                < 20 => JobStatus.InProgress,
                < 40 => JobStatus.Pending,
                < 70 => JobStatus.Completed,
                < 85 => JobStatus.Scheduled,
                < 95 => JobStatus.Failed,
                _ => JobStatus.Cancelled
            };
        }
        else if (daysAgo <= 30)
        {
            var rand = _random.Next(100);
            return rand switch
            {
                < 70 => JobStatus.Completed,
                < 85 => JobStatus.Failed,
                _ => JobStatus.Cancelled
            };
        }
        else
        {
            var rand = _random.Next(100);
            return rand switch
            {
                < 85 => JobStatus.Completed,
                < 95 => JobStatus.Failed,
                _ => JobStatus.Cancelled
            };
        }
    }

    private string GeneratePhoneNumber()
    {
        return $"+1-{_random.Next(200, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
    }

    private async Task<List<Data.Inventory>> SeedInventoryAsync()
    {
        var existingInventoryCount = await _context.Inventories.CountAsync();
        
        if (existingInventoryCount >= 80)
        {
            _logger.LogInformation("Sufficient inventory items already exist, skipping inventory seeding");
            return await _context.Inventories.Take(80).ToListAsync();
        }

        var items = new Dictionary<string, (string Category, string Unit, int MinQty, int MaxQty, int? WarrantyMonths)>
        {
            // Computer Accessories (with warranty)
            ["Logitech MX Master 3 Mouse"] = ("Computer Accessories", "pcs", 20, 100, 12),
            ["Dell USB Keyboard KB216"] = ("Computer Accessories", "pcs", 25, 120, 12),
            ["HP Wired Keyboard and Mouse Combo"] = ("Computer Accessories", "pcs", 30, 80, 12),
            ["Adjustable Laptop Stand"] = ("Computer Accessories", "pcs", 10, 50, 24),
            ["Logitech C920 Webcam"] = ("Computer Accessories", "pcs", 15, 40, 24),
            ["Jabra Evolve 40 Headset"] = ("Computer Accessories", "pcs", 20, 60, 24),
            ["Dell UltraSharp 24 Monitor"] = ("Computer Accessories", "pcs", 10, 35, 36),
            ["HP E24 G4 Monitor"] = ("Computer Accessories", "pcs", 8, 30, 36),
            ["Dell WD19 Docking Station"] = ("Computer Accessories", "pcs", 8, 25, 36),
            ["Kensington Laptop Lock"] = ("Computer Accessories", "pcs", 30, 100, 12),
            ["Cable Management Kit"] = ("Computer Accessories", "set", 15, 60, null),
            ["Ergonomic Mouse Pad"] = ("Computer Accessories", "pcs", 40, 150, null),
            
            // Office Supplies (no warranty)
            ["A4 Printer Paper (Ream 500 sheets)"] = ("Office Supplies", "pack", 100, 300, null),
            ["HP LaserJet Toner 85A"] = ("Office Supplies", "pcs", 15, 50, null),
            ["Canon Ink Cartridge Set"] = ("Office Supplies", "set", 20, 60, null),
            ["Heavy Duty Stapler"] = ("Office Supplies", "pcs", 25, 70, null),
            ["Spiral Notepad A4"] = ("Office Supplies", "pack", 50, 150, null),
            ["Ballpoint Pen Box (50pcs)"] = ("Office Supplies", "box", 60, 180, null),
            ["File Folders (Letter Size)"] = ("Office Supplies", "pack", 40, 120, null),
            ["Sticky Notes Assorted"] = ("Office Supplies", "pack", 80, 200, null),
            ["Desk Organizer"] = ("Office Supplies", "pcs", 20, 60, null),
            
            // Cables & Adapters (with warranty)
            ["Belkin HDMI Cable 2M"] = ("Cables & Adapters", "pcs", 40, 120, 12),
            ["Anker USB-C Cable 6ft"] = ("Cables & Adapters", "pcs", 60, 150, 18),
            ["Cable Matters DisplayPort Cable"] = ("Cables & Adapters", "pcs", 30, 90, 12),
            ["Apple USB-C to HDMI Adapter"] = ("Cables & Adapters", "pcs", 25, 80, 12),
            ["Monoprice Cat6 Ethernet Cable 10ft"] = ("Cables & Adapters", "pcs", 80, 200, 12),
            ["Universal Power Cable"] = ("Cables & Adapters", "pcs", 50, 120, 6),
            ["USB-C to USB-A Adapter"] = ("Cables & Adapters", "pcs", 70, 180, 12),
            ["VGA to HDMI Converter"] = ("Cables & Adapters", "pcs", 15, 50, 12),
            
            // Storage Devices (with warranty)
            ["Samsung T7 External SSD 1TB"] = ("Storage Devices", "pcs", 15, 45, 36),
            ["SanDisk Ultra USB 3.0 64GB"] = ("Storage Devices", "pcs", 40, 120, 24),
            ["WD My Passport 2TB External HDD"] = ("Storage Devices", "pcs", 20, 60, 36),
            ["SanDisk Ultra 128GB SD Card"] = ("Storage Devices", "pcs", 30, 90, 24),
            ["Crucial MX500 1TB SSD"] = ("Storage Devices", "pcs", 10, 40, 60),
            ["Kingston DataTraveler 32GB"] = ("Storage Devices", "pcs", 50, 150, 12),
            
            // Networking Equipment (with warranty)
            ["TP-Link Archer AX50 Wi-Fi Router"] = ("Networking Equipment", "pcs", 8, 25, 36),
            ["Netgear GS308 8-Port Switch"] = ("Networking Equipment", "pcs", 10, 30, 36),
            ["Ubiquiti UniFi AP AC Lite"] = ("Networking Equipment", "pcs", 8, 20, 24),
            ["TP-Link TL-SG1016D 16-Port Switch"] = ("Networking Equipment", "pcs", 5, 15, 36),
            ["Netgear Nighthawk Mesh System"] = ("Networking Equipment", "set", 5, 15, 24),
            
            // Hardware Components (with warranty)
            ["Corsair Vengeance DDR4 16GB RAM"] = ("Hardware Components", "pcs", 15, 50, 60),
            ["Samsung 970 EVO Plus 500GB NVMe"] = ("Hardware Components", "pcs", 20, 60, 60),
            ["Cooler Master Hyper 212"] = ("Hardware Components", "pcs", 10, 35, 24),
            ["EVGA 650W Power Supply"] = ("Hardware Components", "pcs", 8, 30, 120),
            ["Kingston A400 480GB SATA SSD"] = ("Hardware Components", "pcs", 15, 50, 36),
            
            // Software Licenses (with subscription period)
            ["Microsoft Office 365 Business (Annual)"] = ("Software Licenses", "license", 10, 50, 12),
            ["Norton 360 Antivirus (Annual)"] = ("Software Licenses", "license", 20, 80, 12),
            ["Adobe Creative Cloud (Annual)"] = ("Software Licenses", "license", 5, 25, 12),
            ["Zoom Pro License (Annual)"] = ("Software Licenses", "license", 15, 60, 12),
            ["Slack Business License (Annual)"] = ("Software Licenses", "license", 10, 40, 12),
            
            // Peripherals (with warranty)
            ["Zebra DS4308 Barcode Scanner"] = ("Peripherals", "pcs", 8, 30, 36),
            ["Dymo LabelWriter 450"] = ("Peripherals", "pcs", 5, 20, 36),
            ["Wacom Intuos Pro Tablet"] = ("Peripherals", "pcs", 5, 15, 24),
            ["Logitech MeetUp Conference Cam"] = ("Peripherals", "pcs", 3, 10, 24),
            ["Brother HL-L2350DW Printer"] = ("Peripherals", "pcs", 5, 20, 12),
            ["Epson WorkForce Scanner"] = ("Peripherals", "pcs", 8, 25, 24),
            
            // Additional Categories
            ["UPS 650VA Backup Battery"] = ("Power Management", "pcs", 10, 40, 36),
            ["Surge Protector 6-Outlet"] = ("Power Management", "pcs", 30, 100, 24),
            ["USB Hub 7-Port"] = ("Computer Accessories", "pcs", 20, 70, 12),
            ["Wireless Presenter Remote"] = ("Peripherals", "pcs", 15, 50, 12),
            ["Desktop Phone Holder"] = ("Office Supplies", "pcs", 25, 80, null),
            ["Monitor Privacy Screen"] = ("Computer Accessories", "pcs", 15, 50, null),
            ["Cleaning Kit for Electronics"] = ("Office Supplies", "kit", 30, 100, null),
            ["Cable Ties Assorted Pack"] = ("Office Supplies", "pack", 50, 150, null)
        };

        var inventory = new List<Data.Inventory>();
        var suppliers = new[] 
        { 
            "TechSupply Co.", "OfficeMax Pro", "CompuStore International", 
            "DataVend Solutions", "IT Hardware Plus", "Global Tech Supplies",
            "Digital Warehouse", "Prime Electronics", "Business Essentials Inc."
        };

        foreach (var item in items)
        {
            var config = item.Value;
            var currentQty = _random.Next(0, config.MaxQty + 30);
            
            // Determine stock status
            StockStatus status;
            if (currentQty == 0)
                status = StockStatus.OutOfStock;
            else if (currentQty <= config.MinQty)
                status = StockStatus.LowStock;
            else if (currentQty >= config.MaxQty)
                status = StockStatus.Overstocked;
            else if (currentQty >= (config.MaxQty * 0.8))
                status = StockStatus.FullyStocked;
            else
                status = StockStatus.InStock;

            // Generate warranty dates if applicable
            DateTime? warrantyStartDate = null;
            DateTime? warrantyEndDate = null;
            if (config.WarrantyMonths.HasValue && currentQty > 0)
            {
                warrantyStartDate = DateTime.UtcNow.AddDays(-_random.Next(30, 180));
                warrantyEndDate = warrantyStartDate.Value.AddMonths(config.WarrantyMonths.Value);
            }

            var inventoryItem = new Data.Inventory
            {
                ItemName = item.Key,
                SKU = $"INV-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Description = GetItemDescription(item.Key, config.Category),
                Category = config.Category,
                CurrentQuantity = currentQty,
                MinimumQuantity = config.MinQty,
                MaximumQuantity = config.MaxQty,
                Unit = config.Unit,
                StockStatus = status,
                StorageLocation = $"Warehouse {(char)('A' + _random.Next(0, 5))}, Section {_random.Next(1, 6)}, Shelf {_random.Next(1, 12)}",
                Supplier = suppliers[_random.Next(suppliers.Length)],
                WarrantyPeriodMonths = config.WarrantyMonths,
                WarrantyStartDate = warrantyStartDate,
                WarrantyEndDate = warrantyEndDate,
                LastRestocked = currentQty > 0 ? DateTime.UtcNow.AddDays(-_random.Next(1, 120)) : null,
                CreatedDate = DateTime.UtcNow.AddDays(-_random.Next(180, 450)),
                LastUpdated = DateTime.UtcNow,
                UpdatedBy = "System"
            };

            inventory.Add(inventoryItem);
        }

        await _context.Inventories.AddRangeAsync(inventory);
        await _context.SaveChangesAsync();

        // Create transaction history with various transaction types
        var transactions = new List<InventoryTransaction>();
        var transactionTypes = new[] { "Initial Stock", "Restock", "Withdrawal", "Adjustment" };
        var remarks = new[]
        {
            "Initial inventory setup",
            "Quarterly restock from supplier",
            "Issued to IT department",
            "Issued to HR for new employee setup",
            "Emergency restock due to high demand",
            "Inventory count adjustment",
            "Damaged items removed",
            "Returned to supplier",
            "Deployed to remote office",
            "Transferred to warehouse B"
        };

        foreach (var item in inventory.Where(i => i.CurrentQuantity > 0).Take(50))
        {
            // Initial stock transaction
            var initialTransaction = new InventoryTransaction
            {
                InventoryId = item.Id,
                TransactionType = "Initial Stock",
                Quantity = item.CurrentQuantity,
                PreviousQuantity = 0,
                NewQuantity = item.CurrentQuantity,
                Remarks = "Initial inventory setup - system migration",
                Reference = $"PO-{_random.Next(10000, 99999)}",
                PerformedBy = "System",
                TransactionDate = item.CreatedDate
            };
            transactions.Add(initialTransaction);

            // Add 2-5 random transactions for some items
            if (_random.Next(0, 100) < 60) // 60% chance of having additional transactions
            {
                var numTransactions = _random.Next(2, 6);
                var runningQty = item.CurrentQuantity;

                for (int i = 0; i < numTransactions; i++)
                {
                    var transType = transactionTypes[_random.Next(1, transactionTypes.Length)]; // Skip "Initial Stock"
                    var previousQty = runningQty;
                    int quantityChange;

                    if (transType == "Restock")
                    {
                        quantityChange = _random.Next(10, 50);
                        runningQty += quantityChange;
                    }
                    else if (transType == "Withdrawal")
                    {
                        quantityChange = -_random.Next(5, Math.Min(20, runningQty));
                        runningQty += quantityChange; // quantityChange is negative
                    }
                    else // Adjustment
                    {
                        quantityChange = _random.Next(-10, 15);
                        runningQty += quantityChange;
                    }

                    runningQty = Math.Max(0, runningQty); // Ensure non-negative

                    var transaction = new InventoryTransaction
                    {
                        InventoryId = item.Id,
                        TransactionType = transType,
                        Quantity = quantityChange,
                        PreviousQuantity = previousQty,
                        NewQuantity = runningQty,
                        Remarks = remarks[_random.Next(remarks.Length)],
                        Reference = transType == "Restock" ? $"PO-{_random.Next(10000, 99999)}" : null,
                        PerformedBy = _random.Next(0, 100) < 70 ? $"user{_random.Next(1, 10)}@company.com" : "System",
                        TransactionDate = item.CreatedDate.AddDays(_random.Next(1, 120))
                    };
                    transactions.Add(transaction);
                }
            }
        }

        await _context.InventoryTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        return inventory;
    }

    private string GetItemDescription(string itemName, string category)
    {
        return category switch
        {
            "Computer Accessories" => $"Professional-grade {itemName.ToLower()} designed for business productivity and reliability.",
            "Office Supplies" => $"Essential office supply - {itemName.ToLower()} for everyday business operations.",
            "Cables & Adapters" => $"High-quality {itemName.ToLower()} ensuring reliable connectivity and data transfer.",
            "Storage Devices" => $"Reliable {itemName.ToLower()} for secure data storage and backup solutions.",
            "Networking Equipment" => $"Enterprise-grade {itemName.ToLower()} for robust network infrastructure.",
            "Hardware Components" => $"Premium {itemName.ToLower()} for system upgrades and maintenance.",
            "Software Licenses" => $"Licensed {itemName} subscription for business productivity and security.",
            "Peripherals" => $"Professional {itemName.ToLower()} for enhanced workflow efficiency.",
            "Power Management" => $"Reliable {itemName.ToLower()} for power protection and continuity.",
            _ => $"Quality {itemName.ToLower()} for business use."
        };
    }

    private string GenerateRandomNote()
    {
        var notes = new[]
        {
            "Asset in good condition",
            "Requires software updates",
            "Hardware upgrade scheduled",
            "Minor cosmetic damage",
            "Performance issues reported",
            "Recently serviced",
            "Battery replacement needed",
            "Screen replacement completed",
            "Keyboard issues noted",
            "Pending disposal review"
        };
        return notes[_random.Next(notes.Length)];
    }

    private string GenerateJobNote(JobStatus status)
    {
        return status switch
        {
            JobStatus.Completed => "Imaging completed successfully. All drivers installed.",
            JobStatus.Failed => "Imaging failed due to hardware incompatibility. Requires review.",
            JobStatus.InProgress => "Imaging in progress. Approximately 45 minutes remaining.",
            JobStatus.Pending => "Awaiting technician availability.",
            JobStatus.Cancelled => "Job cancelled due to asset retirement.",
            JobStatus.Scheduled => "Scheduled for next maintenance window.",
            _ => "Standard imaging job."
        };
    }

    public async Task<(int Departments, int Employees, int Assets, int ImagingJobs)> GetSeedingStatisticsAsync()
    {
        var departments = await _context.Departments.CountAsync();
        var employees = await _context.Employees.CountAsync();
        var assets = await _context.Assets.CountAsync();
        var imagingJobs = await _context.ImagingJobs.CountAsync();

        return (departments, employees, assets, imagingJobs);
    }
}
