using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace buildone.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<ImagingJob> ImagingJobs { get; set; }
    public DbSet<AssetHistory> AssetHistory { get; set; }
    public DbSet<JobComment> JobComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity entities
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with Employee (optional)
            entity.HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.FullName);
            entity.HasIndex(e => e.CreatedAt);
            
            // Performance: Composite index for common queries
            entity.HasIndex(e => new { e.IsActive, e.Email });
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Department entity
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(10);
            
            // Add index on department name
            entity.HasIndex(e => e.Name);
            // Add unique constraint on code if it's provided
            entity.HasIndex(e => e.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            
            // Add unique constraint on username
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.HasIndex(e => e.FullName);
            
            // Performance: Composite index for common queries
            entity.HasIndex(e => new { e.DepartmentId, e.FullName });
            
            // Configure relationship with Department
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Asset entity
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssetTag).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PcId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Add unique constraints
            entity.HasIndex(e => e.AssetTag).IsUnique();
            entity.HasIndex(e => e.PcId).IsUnique();
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Brand, e.Model });
            
            // Performance: Composite indexes for common queries
            entity.HasIndex(e => new { e.Status, e.AssignedEmployeeId });
            entity.HasIndex(e => new { e.DepartmentId, e.Status });
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            
            // Configure relationship with Employee
            entity.HasOne(a => a.AssignedEmployee)
                .WithMany(e => e.Assets)
                .HasForeignKey(a => a.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure relationship with Department
            entity.HasOne(a => a.Department)
                .WithMany(d => d.Assets)
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ImagingJob entity
        modelBuilder.Entity<ImagingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageVersion).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Add indexes for performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.ImagingType);
            entity.HasIndex(e => new { e.AssetId, e.Status });
            
            // Configure relationship with Asset
            entity.HasOne(ij => ij.Asset)
                .WithMany(a => a.ImagingJobs)
                .HasForeignKey(ij => ij.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with Technician (Employee)
            entity.HasOne(ij => ij.Technician)
                .WithMany(e => e.ImagingJobs)
                .HasForeignKey(ij => ij.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AssetHistory entity
        modelBuilder.Entity<AssetHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.FromValue)
                  .HasMaxLength(500);
            entity.Property(e => e.ToValue)
                  .HasMaxLength(500);
            entity.Property(e => e.Notes)
                  .HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
                  
            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.History)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Actor)
                  .WithMany(emp => emp.AssetHistoryEntries)
                  .HasForeignKey(e => e.ActorId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(e => e.AssetId);
            entity.HasIndex(e => e.ActorId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Action);
        });

        // Configure JobComment entity
        modelBuilder.Entity<JobComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment)
                  .IsRequired()
                  .HasMaxLength(2000);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
                  
            entity.HasOne(e => e.ImagingJob)
                  .WithMany(ij => ij.Comments)
                  .HasForeignKey(e => e.ImagingJobId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Employee)
                  .WithMany()
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => e.ImagingJobId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Seed Departments - keeping minimal data for proper system function
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Information Technology", Code = "IT" },
            new Department { Id = 2, Name = "Human Resources", Code = "HR" },
            new Department { Id = 3, Name = "Finance", Code = "FIN" }
        );

        // Note: User and Role seeding is now handled by DataSeedingService at startup
        // This ensures proper password hashing and avoids migration issues

        // Demo data commented out - tables will show only user-entered data
        /*
        // Seed Employees
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FullName = "Alice Wilson", Username = "awilson", Email = "alice.wilson@company.com", DepartmentId = 1, Phone = "555-0101" },
            new Employee { Id = 2, FullName = "David Brown", Username = "dbrown", Email = "david.brown@company.com", DepartmentId = 1, Phone = "555-0102" },
            new Employee { Id = 3, FullName = "Sarah Davis", Username = "sdavis", Email = "sarah.davis@company.com", DepartmentId = 2, Phone = "555-0103" },
            new Employee { Id = 4, FullName = "Michael Johnson", Username = "mjohnson", Email = "michael.johnson@company.com", DepartmentId = 3, Phone = "555-0104" },
            new Employee { Id = 5, FullName = "Jennifer Lee", Username = "jlee", Email = "jennifer.lee@company.com", DepartmentId = 2, Phone = "555-0105" }
        );

        // Seed Assets
        modelBuilder.Entity<Asset>().HasData(
            new Asset { Id = 1, AssetTag = "LAPTOP001", PcId = "PC001", Brand = "Dell", Model = "Latitude 7420", SerialNumber = "DL001234", ImagingType = Data.Enums.ImagingType.Fresh, DeploymentType = Data.Enums.DeploymentType.AutomaticDeployment, Status = Data.Enums.AssetStatus.Deployed, AssignedEmployeeId = 1, DepartmentId = 1, Notes = "Primary laptop for IT Manager", WarrantyExpiry = new DateTime(2025, 12, 15), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 2, AssetTag = "LAPTOP002", PcId = "PC002", Brand = "HP", Model = "EliteBook 840", SerialNumber = "HP001234", ImagingType = Data.Enums.ImagingType.WipeAndLoad, DeploymentType = Data.Enums.DeploymentType.InPlace, Status = Data.Enums.AssetStatus.Assigned, AssignedEmployeeId = 2, DepartmentId = 1, Notes = "Development laptop", WarrantyExpiry = new DateTime(2025, 10, 30), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 3, AssetTag = "DESKTOP001", PcId = "PC003", Brand = "Dell", Model = "OptiPlex 7090", SerialNumber = "DL005678", ImagingType = Data.Enums.ImagingType.BareMetal, DeploymentType = Data.Enums.DeploymentType.PostDeploymentSupport, Status = Data.Enums.AssetStatus.InStock, Notes = "Available for assignment", WarrantyExpiry = new DateTime(2026, 3, 20), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 4, AssetTag = "LAPTOP003", PcId = "PC004", Brand = "HP", Model = "ProBook 450", SerialNumber = "HP005678", ImagingType = Data.Enums.ImagingType.Fresh, DeploymentType = Data.Enums.DeploymentType.AutomaticDeployment, Status = Data.Enums.AssetStatus.Assigned, AssignedEmployeeId = 3, DepartmentId = 2, Notes = "HR Department laptop", WarrantyExpiry = new DateTime(2025, 11, 15), CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 5, AssetTag = "DESKTOP002", PcId = "PC005", Brand = "Dell", Model = "OptiPlex 5090", SerialNumber = "DL009876", ImagingType = Data.Enums.ImagingType.WipeAndLoad, DeploymentType = Data.Enums.DeploymentType.InPlace, Status = Data.Enums.AssetStatus.Deployed, AssignedEmployeeId = 4, DepartmentId = 3, Notes = "Finance workstation", WarrantyExpiry = new DateTime(2026, 2, 10), CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 6, AssetTag = "LAPTOP004", PcId = "PC006", Brand = "HP", Model = "ZBook Studio", SerialNumber = "HP112233", ImagingType = Data.Enums.ImagingType.BareMetal, DeploymentType = Data.Enums.DeploymentType.PostDeploymentSupport, Status = Data.Enums.AssetStatus.InStock, Notes = "High-performance workstation for design work", WarrantyExpiry = new DateTime(2026, 4, 5), CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 7, AssetTag = "TABLET001", PcId = "PC007", Brand = "Dell", Model = "Latitude 7320 Detachable", SerialNumber = "DL445566", ImagingType = Data.Enums.ImagingType.Fresh, DeploymentType = Data.Enums.DeploymentType.AutomaticDeployment, Status = Data.Enums.AssetStatus.Assigned, AssignedEmployeeId = 5, DepartmentId = 2, Notes = "Mobile device for presentations", WarrantyExpiry = new DateTime(2025, 9, 25), CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 8, AssetTag = "DESKTOP003", PcId = "PC008", Brand = "HP", Model = "EliteDesk 800", SerialNumber = "HP778899", ImagingType = Data.Enums.ImagingType.WipeAndLoad, DeploymentType = Data.Enums.DeploymentType.InPlace, Status = Data.Enums.AssetStatus.Maintenance, Notes = "Undergoing hardware upgrade", WarrantyExpiry = new DateTime(2024, 12, 31), CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 9, AssetTag = "LAPTOP005", PcId = "PC009", Brand = "Dell", Model = "Inspiron 7510", SerialNumber = "DL334455", ImagingType = Data.Enums.ImagingType.BareMetal, DeploymentType = Data.Enums.DeploymentType.PostDeploymentSupport, Status = Data.Enums.AssetStatus.InStock, Notes = "Budget laptop for temporary assignments", WarrantyExpiry = new DateTime(2025, 8, 15), CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Asset { Id = 10, AssetTag = "WORKSTATION001", PcId = "PC010", Brand = "HP", Model = "Z4 G4", SerialNumber = "HP990011", ImagingType = Data.Enums.ImagingType.Fresh, DeploymentType = Data.Enums.DeploymentType.AutomaticDeployment, Status = Data.Enums.AssetStatus.Retired, Notes = "End of life - scheduled for disposal", WarrantyExpiry = new DateTime(2024, 6, 1), CreatedAt = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed ImagingJobs
        modelBuilder.Entity<ImagingJob>().HasData(
            new ImagingJob { Id = 1, AssetId = 1, TechnicianId = 1, ImagingType = Data.Enums.ImagingType.Fresh, ImageVersion = "Windows11-2024.1", Status = Data.Enums.JobStatus.Completed, ScheduledAt = new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Utc), StartedAt = new DateTime(2024, 1, 2, 9, 15, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2024, 1, 2, 11, 30, 0, DateTimeKind.Utc), Notes = "Successfully imaged with standard corporate image", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 2, AssetId = 2, TechnicianId = 2, ImagingType = Data.Enums.ImagingType.WipeAndLoad, ImageVersion = "DevEnvironment-2024.1", Status = Data.Enums.JobStatus.InProgress, ScheduledAt = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc), StartedAt = new DateTime(2024, 1, 3, 10, 5, 0, DateTimeKind.Utc), Notes = "Installing development tools and environment", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 3, AssetId = 3, ImagingType = Data.Enums.ImagingType.BareMetal, ImageVersion = "Windows11-2024.1", Status = Data.Enums.JobStatus.Pending, ScheduledAt = new DateTime(2024, 1, 5, 14, 0, 0, DateTimeKind.Utc), Notes = "Waiting for hardware preparation", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 4, AssetId = 4, TechnicianId = 1, ImagingType = Data.Enums.ImagingType.Fresh, ImageVersion = "HROffice-2024.1", Status = Data.Enums.JobStatus.Completed, ScheduledAt = new DateTime(2024, 1, 8, 8, 0, 0, DateTimeKind.Utc), StartedAt = new DateTime(2024, 1, 8, 8, 10, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2024, 1, 8, 10, 45, 0, DateTimeKind.Utc), Notes = "HR suite with specialized applications", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 5, AssetId = 5, TechnicianId = 2, ImagingType = Data.Enums.ImagingType.WipeAndLoad, ImageVersion = "FinanceSecure-2024.1", Status = Data.Enums.JobStatus.Completed, ScheduledAt = new DateTime(2024, 1, 10, 9, 0, 0, DateTimeKind.Utc), StartedAt = new DateTime(2024, 1, 10, 9, 5, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2024, 1, 10, 12, 30, 0, DateTimeKind.Utc), Notes = "Financial software with enhanced security", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 6, AssetId = 6, TechnicianId = 1, ImagingType = Data.Enums.ImagingType.BareMetal, ImageVersion = "Workstation-2024.1", Status = Data.Enums.JobStatus.Scheduled, ScheduledAt = new DateTime(2024, 1, 15, 13, 0, 0, DateTimeKind.Utc), Notes = "High-performance image with CAD software", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 7, AssetId = 8, TechnicianId = 2, ImagingType = Data.Enums.ImagingType.WipeAndLoad, ImageVersion = "Windows11-2024.1", Status = Data.Enums.JobStatus.Failed, ScheduledAt = new DateTime(2024, 1, 12, 11, 0, 0, DateTimeKind.Utc), StartedAt = new DateTime(2024, 1, 12, 11, 10, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2024, 1, 12, 11, 45, 0, DateTimeKind.Utc), Notes = "Hardware failure detected during imaging", CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new ImagingJob { Id = 8, AssetId = 9, TechnicianId = 1, ImagingType = Data.Enums.ImagingType.BareMetal, ImageVersion = "BasicOffice-2024.1", Status = Data.Enums.JobStatus.Cancelled, ScheduledAt = new DateTime(2024, 1, 18, 14, 0, 0, DateTimeKind.Utc), Notes = "Cancelled due to change in requirements", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) }
        );
        */
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => (e.Entity is Asset || e.Entity is ImagingJob) 
                && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Modified)
            {
                switch (entityEntry.Entity)
                {
                    case Asset asset:
                        asset.UpdatedAt = DateTime.UtcNow;
                        break;
                    case ImagingJob imagingJob:
                        imagingJob.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}