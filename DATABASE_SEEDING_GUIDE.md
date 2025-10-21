# Database Seeding Guide

## Overview

This guide explains how to populate the BuildOne Asset Management System database with test data before implementing Single Sign-On (SSO). The seeding process creates realistic test data including departments, employees, assets, and imaging jobs.

## Prerequisites

1. The BuildOne application must be running
2. You must have administrative access (use admin@buildone.com / Admin123!)
3. Database connection must be configured correctly

## What Gets Created

### Basic Seeding
- **3 Roles**: Administrator, Technician, User
- **1 Admin Account**: admin@buildone.com (Password: Admin123!)

### Bulk Seeding
- **8 Departments**:
  - Information Technology (IT)
  - Human Resources (HR)
  - Finance (FIN)
  - Operations (OPS)
  - Sales (SALES)
  - Marketing (MKT)
  - Customer Support (CS)
  - Research & Development (RD)

- **10 Employees**:
  - Randomly generated realistic names
  - Distributed across all departments
  - Each with email (firstname.lastname@buildone.com) and phone number

- **400 Assets**:
  - **200 Laptops (50%)**:
    - Brands: Dell, HP, Lenovo, Apple, Microsoft, Asus
    - Models: Dell Latitude 5420/7420, HP EliteBook 840, Lenovo ThinkPad X1, MacBook Pro, Surface Laptop, etc.
  - **140 Desktops (35%)**:
    - Brands: Dell, HP, Lenovo, Acer
    - Models: Dell OptiPlex, HP EliteDesk/ProDesk, Lenovo ThinkCentre
  - **60 Tablets (15%)**:
    - Brands: Apple, Samsung, Microsoft, Lenovo
    - Models: iPad Pro/Air, Galaxy Tab, Surface Pro/Go

- **400 Imaging Jobs**:
  - Various statuses: Completed, In Progress, Pending, Scheduled, Failed, Cancelled
  - Priorities: Low, Normal, High, Urgent
  - Image versions: WIN11-PRO-23H2-v1, WIN11-ENT-23H2-v1, WIN10-PRO-22H2-v3, etc.
  - Realistic date distributions based on job age

## Methods to Seed Data

### Method 1: Web Interface (Recommended)

1. **Start the application**:
   ```powershell
   dotnet run
   ```

2. **Login to the application**:
   - Navigate to: `https://localhost:5001/Account/Login`
   - Email: `admin@buildone.com`
   - Password: `Admin123!`

3. **Access the Data Seeding page**:
   - Navigate to: `https://localhost:5001/Admin/SeedData`
   - Or use the System Settings menu → Data Seeding

4. **Perform seeding**:
   - Click "Seed Basic Data" to create roles and admin user (if not already done)
   - Click "Seed Bulk Data" to create all test data (400+ records)
   - Wait for confirmation message

5. **View statistics**:
   - The page displays real-time statistics showing how many records exist

### Method 2: PowerShell Script

1. **Run the provided script**:
   ```powershell
   .\seed-database.ps1
   ```

2. Follow the on-screen instructions

### Method 3: API Endpoints

Use a tool like Postman, cURL, or PowerShell to call the API endpoints:

#### Get Current Statistics
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/DataSeeding/statistics" -Method GET -UseDefaultCredentials
$response
```

#### Seed Basic Data
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/DataSeeding/seed-basic" -Method POST -UseDefaultCredentials
```

#### Seed Bulk Data
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/DataSeeding/seed-bulk" -Method POST -UseDefaultCredentials
```

### Method 4: Direct Database Seeding (Code)

In your application startup or a migration, you can programmatically call:

```csharp
using var scope = app.Services.CreateScope();
var dataSeedingService = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();

// Seed basic data (roles and admin)
await dataSeedingService.SeedDataAsync();

// Seed bulk data (400+ records)
await dataSeedingService.SeedBulkDataAsync();
```

## Asset Details Generated

### Asset Tag Format
- Laptops: `AST-LAP-00001`, `AST-LAP-00002`, etc.
- Desktops: `AST-DES-00001`, `AST-DES-00002`, etc.
- Tablets: `AST-TAB-00001`, `AST-TAB-00002`, etc.

### PC ID Format
- `PC-000001`, `PC-000002`, etc.

### Serial Numbers
- Format: `{BRAND}{RANDOM_6_DIGITS}`
- Example: `DELL123456`, `HP789012`, `LENOVO345678`

### Asset Status Distribution
- 70% Assigned to employees
- 20% Active (available)
- 10% In Stock

### Warranty Information
- All assets have warranty expiry dates
- Randomly set between 12-48 months from creation

## Imaging Job Details

### Job Status Distribution
- **Recent Jobs (≤7 days old)**:
  - 20% In Progress
  - 20% Pending
  - 30% Completed
  - 15% Scheduled
  - 10% Failed
  - 5% Cancelled

- **Older Jobs (>7 days)**:
  - 85% Completed
  - 10% Failed
  - 5% Cancelled

### Imaging Types
- Bare Metal
- Wipe and Load
- Fresh

### Estimated Duration
- Between 60-180 minutes depending on job type and status

## Important Notes

### Idempotent Operations
- The seeding process is **idempotent** - running it multiple times will NOT create duplicates
- Existing records are preserved and reused
- Only missing records are created

### Performance
- Bulk seeding creates 400+ database records
- Expected duration: 30-60 seconds depending on your system
- Progress is logged in the application console

### Data Relationships
- Assets are properly assigned to employees
- Imaging jobs reference existing assets and technicians
- All foreign key relationships are maintained

## Verification

After seeding, verify the data by:

1. **Check the Statistics**:
   - Visit `/Admin/SeedData` to see current counts
   - Should show: 8 departments, 10 employees, 400 assets, 400 imaging jobs

2. **Browse the Data**:
   - Visit `/Assets` to see all assets
   - Visit `/Employees` to see all employees
   - Visit `/ImagingJobs` to see all imaging jobs

3. **Check Database**:
   ```sql
   SELECT 'Departments' as Entity, COUNT(*) as Count FROM Departments
   UNION ALL
   SELECT 'Employees', COUNT(*) FROM Employees
   UNION ALL
   SELECT 'Assets', COUNT(*) FROM Assets
   UNION ALL
   SELECT 'ImagingJobs', COUNT(*) FROM ImagingJobs
   ```

## Troubleshooting

### "Failed to seed data"
- Check database connection string in `appsettings.json`
- Ensure database exists and migrations are applied
- Check application logs in `logs/buildone-{date}.log`

### "Insufficient permissions"
- Ensure you're logged in as Administrator
- Default credentials: admin@buildone.com / Admin123!

### Slow Performance
- First-time seeding may take longer due to indexes
- Subsequent runs are faster (idempotent checks)
- Check SQL Server performance

### Duplicate Data Concerns
- The seeding service checks for existing records
- No duplicates will be created
- Safe to run multiple times

## Next Steps: Single Sign-On (SSO)

After populating the database with test data:

1. **Analyze Current Authentication**:
   - Review the existing Identity implementation in `Program.cs`
   - Check `ApplicationUser` and `ApplicationRole` models

2. **Choose SSO Provider**:
   - Azure Active Directory (Microsoft Entra ID)
   - Okta
   - Auth0
   - Google Workspace
   - Custom SAML/OAuth provider

3. **Integration Steps**:
   - Install required NuGet packages (e.g., `Microsoft.Identity.Web`)
   - Configure SSO settings in `appsettings.json`
   - Update authentication middleware in `Program.cs`
   - Map SSO claims to `ApplicationUser` properties
   - Test with existing test data

4. **Migration Considerations**:
   - Decide how to map SSO users to existing employees
   - Consider using email as the matching key
   - Plan for role assignment from SSO groups/claims

## Support

For issues or questions:
- Check application logs: `logs/buildone-{date}.log`
- Review Serilog output in console
- Contact the development team

---

**Last Updated**: October 7, 2025
**Version**: 1.0
