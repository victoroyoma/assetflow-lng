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

            _logger.LogInformation("Bulk data seeding completed successfully!");
            _logger.LogInformation("Summary: {Depts} departments, {Emps} employees, {Assets} assets, {Jobs} imaging jobs",
                departments.Count, employees.Count, assets.Count, imagingJobs.Count);
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
