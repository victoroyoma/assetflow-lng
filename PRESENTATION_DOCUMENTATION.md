# BuildOne Asset Management System - Presentation Documentation

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [Main Dashboard](#main-dashboard)
3. [Core Modules](#core-modules)
4. [Database Seeding](#database-seeding)
5. [Security & Authentication](#security--authentication)
6. [Advanced Features](#advanced-features)
7. [Technical Architecture](#technical-architecture)

---

## 🎯 System Overview

### What is BuildOne?
BuildOne is a comprehensive **Asset Management System** designed for tracking and managing IT assets, employees, departments, inventory, and imaging/maintenance jobs. Built with ASP.NET Core, it provides a robust, secure, and user-friendly platform for enterprise asset lifecycle management.

### Key Capabilities
- ✅ **Asset Lifecycle Management** - Track assets from acquisition to disposal
- ✅ **Employee & Department Management** - Organize workforce structure
- ✅ **Inventory Control** - Monitor stock levels and warranties
- ✅ **Jobs Management** - Schedule imaging and maintenance tasks
- ✅ **QR/Barcode Scanning** - Quick asset identification
- ✅ **Role-Based Access Control** - Secure permission management
- ✅ **Data Import/Export** - Excel integration for bulk operations
- ✅ **Real-time Notifications** - Stay updated on system events

### Technology Stack
```
Frontend:  Razor Pages, Bootstrap 5, JavaScript, jQuery
Backend:   ASP.NET Core 8.0, C#
Database:  SQL Server with Entity Framework Core
Security:  ASP.NET Core Identity, Policy-based Authorization
Logging:   Serilog with structured logging
Validation: FluentValidation
Mapping:   AutoMapper
API Docs:  Swagger/OpenAPI
```

---

## 🏠 Main Dashboard

### Dashboard URL
```
https://localhost:5001/
```

### Dashboard Overview
The main dashboard serves as the central command center, providing:
- **Quick Statistics** - Real-time inventory metrics
- **Status Overview** - Asset and stock health indicators
- **Quick Actions** - Fast access to common tasks
- **Recent Activity** - Latest jobs and updates
- **User Welcome** - Personalized greeting with role information

### Visual Layout

#### 1. **Welcome Header**
```
┌─────────────────────────────────────────────────────┐
│ 🔥 Asset Management Dashboard                      │
│ Comprehensive overview of your IT assets           │
│ 🍃 Welcome back, [User Name]                       │
└─────────────────────────────────────────────────────┘
```
- **Brand Colors**: Blue (#004C97), Green (#009739), Yellow (#F2C800)
- **Energy Theme**: Flame and leaf icons representing energy company branding

#### 2. **Inventory Summary Cards**

**Total Items Card**
```
┌──────────────────────┐
│ 📦 Total Items       │
│ [COUNT]              │
│ [View All Button]    │
└──────────────────────┘
```

**Total Quantity Card**
```
┌──────────────────────┐
│ 📦 Total Quantity    │
│ [COUNT] Units        │
│ ✓ Units in stock     │
└──────────────────────┘
```

**Out of Stock Alert**
```
┌──────────────────────┐
│ ⚠️ Out of Stock      │
│ [COUNT]              │
│ [Status Badge]       │
└──────────────────────┘
```

**Low Stock Warning**
```
┌──────────────────────┐
│ ⚠️ Low Stock         │
│ [COUNT]              │
│ [Restock Badge]      │
└──────────────────────┘
```

#### 3. **Stock Status Breakdown**
Three cards displaying:
- **In Stock** (Green ✓) - Items available for use
- **Fully Stocked** (Blue ✓✓) - Items at maximum capacity
- **Expiring Soon** (Orange ⏰) - Items with warranties expiring within 3 months

#### 4. **Quick Actions Section**
Fast-access buttons for common tasks:
```
[🔍 Asset Search]  [🖥️ Schedule Imaging]  [🔧 Maintenance Job]
```
- **Asset Search** → `/Assets/Search` - Quick asset lookup
- **Schedule Imaging** → `/Imaging/Create?type=Imaging` - Create imaging job
- **Maintenance Job** → `/Imaging/Create?type=Maintenance` - Create maintenance job

#### 5. **Recent Jobs Overview**
Displays the latest 5 jobs:
- Job ID and asset information
- Status badges (Completed, In Progress, Pending, etc.)
- Priority indicators (Low, Normal, High, Urgent)
- Quick links to job details

#### 6. **Warranty Alerts**
Shows items requiring attention:
- **Expiring Soon** - Warranties ending within 3 months
- **Expired** - Items past warranty date
- Direct links to warranty reports

### Dashboard Statistics Logic
```csharp
// Inventory Statistics Calculation
TotalItems = Count of all inventory items
TotalQuantity = Sum of all current quantities
OutOfStock = Items with StockStatus.OutOfStock
LowStock = Items with StockStatus.LowStock
InStock = Items with StockStatus.InStock
FullyStocked = Items with StockStatus.FullyStocked
ExpiringSoon = Items with warranty ending in ≤3 months
Expired = Items with warranty already expired
```

---

## 📦 Core Modules

### 1. Assets Module
**Route**: `/Assets`

#### Features
- **Asset List View** - Comprehensive table of all assets
- **Advanced Search** - Filter by status, department, type
- **Asset Details** - Complete asset information including history
- **QR/Barcode Scanning** - Quick asset lookup and verification
- **Asset Creation** - Add new assets with validation
- **Asset Assignment** - Assign to employees/departments
- **History Tracking** - Audit trail of all changes

#### Asset Types Supported
- **Laptops** - Dell, HP, Lenovo, Apple, Microsoft, Asus
- **Desktops** - Dell OptiPlex, HP EliteDesk, Lenovo ThinkCentre
- **Tablets** - iPad, Surface, Galaxy Tab
- **Monitors** - Various brands and sizes
- **Peripherals** - Keyboards, mice, accessories

#### Asset Statuses
```
Active       - Ready for use
Assigned     - Currently in use by employee
In Stock     - Available in inventory
Maintenance  - Under repair/maintenance
Retired      - End of lifecycle
Disposed     - Removed from inventory
```

#### Asset Information Tracked
- **Identification**: Asset Tag, PC ID, Serial Number
- **Specifications**: Brand, Model, Type, Processor, RAM, Storage
- **Assignment**: Employee, Department, Location
- **Warranty**: Purchase Date, Warranty End Date
- **Financial**: Purchase Cost, Current Value
- **Status**: Current status and condition notes

### 2. Employees Module
**Route**: `/Employees`

#### Features
- **Employee Directory** - Complete list with contact information
- **Department Assignment** - Link employees to departments
- **Asset Assignment** - View assets assigned to each employee
- **Contact Information** - Email, phone, office location
- **User Account Linking** - Connect to system user accounts

#### Employee Information
- Full Name and Employee ID
- Email and Phone Number
- Department and Position
- Office Location
- Hire Date
- Status (Active/Inactive)
- Assigned Assets Count

### 3. Departments Module
**Route**: `/Departments`

#### Features
- **Department Management** - Create, edit, delete departments
- **Employee Count** - Track department size
- **Asset Allocation** - Assets assigned to department
- **Department Codes** - Unique identifiers

#### Pre-configured Departments
```
IT    - Information Technology
HR    - Human Resources
FIN   - Finance
OPS   - Operations
SALES - Sales Department
MKT   - Marketing
CS    - Customer Support
RD    - Research & Development
```

### 4. Jobs Module
**Route**: `/Imaging/Index`

#### Job Types

**Imaging Jobs** 🖥️
- **Purpose**: Install/reinstall operating systems
- **Information Tracked**:
  - Asset being imaged
  - Assigned technician
  - Imaging type (Windows 10, Windows 11, Linux, etc.)
  - Image version (e.g., WIN11-PRO-23H2-v1)
  - Priority level
  - Scheduled and due dates
  - Status and progress

**Maintenance Jobs** 🔧
- **Purpose**: Repair, upgrade, or maintain assets
- **Information Tracked**:
  - Asset requiring maintenance
  - Assigned technician
  - Priority level
  - Scheduled and due dates
  - Maintenance notes
  - Status and completion

#### Job Statuses
```
Pending      - Created, awaiting start
Scheduled    - Planned for specific date/time
In Progress  - Currently being worked on
Completed    - Successfully finished
Failed       - Encountered errors
Cancelled    - Terminated before completion
On Hold      - Temporarily paused
```

#### Priority Levels
```
Low      - Can be done when convenient
Normal   - Standard priority
High     - Needs attention soon
Urgent   - Requires immediate action
```

#### Job Features
- **Queue Management** - View jobs in priority order
- **Job Comments** - Add notes and updates
- **File Attachments** - Attach relevant documents
- **Bulk Operations** - Process multiple jobs
- **Status Updates** - Real-time progress tracking
- **Assignment Notification** - Alert technicians

### 5. Inventory Module
**Route**: `/Inventory`

#### Features
- **Stock Management** - Track quantities and locations
- **Reorder Alerts** - Low stock notifications
- **Transaction History** - Track all inventory movements
- **Warranty Tracking** - Monitor warranty expiration
- **Category Management** - Organize inventory by type

#### Stock Statuses
```
OutOfStock    - Quantity = 0
LowStock      - Quantity ≤ Reorder Level
InStock       - Quantity > Reorder Level and < Maximum
FullyStocked  - Quantity = Maximum Stock Level
```

#### Inventory Information
- Item Name and Description
- SKU/Part Number
- Current Quantity
- Minimum/Maximum Stock Levels
- Reorder Level and Quantity
- Unit Cost and Total Value
- Supplier Information
- Storage Location
- Warranty Information

### 6. User Management Module
**Route**: `/UserManagement`

#### Features (Administrator Only)
- **User CRUD Operations** - Create, read, update, delete users
- **Role Assignment** - Assign multiple roles per user
- **Employee Linking** - Connect users to employee records
- **Password Management** - Admin password reset
- **Account Status** - Activate/deactivate accounts
- **Last Admin Protection** - Prevents deletion of last administrator

#### User Information
- Full Name
- Email Address (username)
- Assigned Roles
- Linked Employee
- Account Status (Active/Inactive)
- Last Login Date
- Creation Date

#### Search & Filter
- Search by name or email
- Filter by role (Administrator, Technician, User)
- Filter by status (Active, Inactive)
- Real-time client-side filtering

### 7. Role Management Module
**Route**: `/RoleManagement`

#### Features (Administrator Only)
- **Role CRUD Operations** - Create, read, update, delete roles
- **System Role Protection** - Cannot delete default roles
- **User Count Display** - Shows users assigned to each role
- **Deletion Prevention** - Cannot delete roles in use

#### Default Roles
```
Administrator - Full system access, all permissions
Technician   - Asset and job management
User         - Basic read access, limited modifications
```

#### Permission Policies
The system uses 15+ granular policies:
- `CanManageUsers` - User administration
- `CanManageRoles` - Role administration
- `CanManageAssets` - Asset CRUD operations
- `CanManageEmployees` - Employee management
- `CanManageDepartments` - Department management
- `CanManageInventory` - Inventory operations
- `CanAccessSystemSettings` - System configuration
- `CanViewReports` - Access reporting features
- And more...

### 8. QR/Barcode Scanner
**Route**: `/Assets/Scan`

#### Features
- **Dual Mode Scanning** - QR codes and barcodes
- **Real-time Asset Lookup** - Instant database verification
- **Editable Scan Table** - Review and edit before saving
- **Duplicate Prevention** - Automatically rejects duplicates
- **Manual Entry** - Type codes when camera unavailable
- **Batch Processing** - Scan multiple items at once

#### Scan Modes
```
QR & Barcode - Scan both types simultaneously (default)
QR Only      - Scan only QR codes
Barcode Only - Scan UPC, EAN, Code 128, Code 39, Code 93
```

#### Workflow
1. Start camera scanner
2. Point at QR code or barcode
3. Item automatically added to table
4. System looks up asset information
5. Edit codes or add notes if needed
6. Remove unwanted items
7. Save all scanned assets

### 9. Search Module
**Route**: `/Search`

#### Global Search Features
- **Asset Search** - Find assets by tag, serial, PC ID
- **Employee Search** - Find employees by name, email
- **Department Search** - Locate departments
- **Inventory Search** - Search inventory items
- **Quick Results** - Instant search as you type
- **Filters** - Refine results by type and status

### 10. Reports Module
**Route**: `/Reports`

#### Available Reports
- **Inventory Report** - Stock levels and values
- **Warranty Alerts** - Expiring and expired warranties
- **Asset Report** - Complete asset listings
- **Assignment Report** - Assets by employee/department
- **Job Report** - Imaging and maintenance statistics
- **Audit Trail** - System activity logs

#### Report Features
- **Excel Export** - Download reports as XLSX files
- **Filtering** - Customize report criteria
- **Date Ranges** - Historical data analysis
- **Real-time Data** - Current system state
- **Printable** - Print-friendly formats

### 11. System Settings
**Route**: `/SystemSettings`

#### Configuration Options (Administrator Only)
- **General Settings** - System name, timezone
- **Email Configuration** - SMTP settings for notifications
- **Backup Settings** - Database backup schedules
- **Data Retention** - Archive and cleanup policies
- **Asset Defaults** - Default warranty periods, statuses
- **Job Defaults** - Default priorities, statuses
- **Notification Rules** - Alert thresholds and recipients

---

## 🌱 Database Seeding

### Overview
Database seeding populates the system with test data for demonstration, development, and testing purposes. BuildOne offers multiple seeding options.

### Seeding Methods

#### Method 1: Web Interface (Recommended)
**Route**: `/Admin/SeedData`

**Steps**:
1. Login as administrator
   - Email: `admin@buildone.com`
   - Password: `Admin123!`
2. Navigate to System Settings → Data Seeding
3. View current statistics
4. Click "Seed Basic Data" (if needed)
5. Click "Seed Bulk Data" to create test data
6. Confirm operation
7. View updated statistics

**Web Interface Features**:
- Real-time statistics display
- One-click seeding buttons
- Progress indicators
- Confirmation messages
- Error handling with user feedback

#### Method 2: PowerShell Scripts

**Basic Seeding Script**: `seed-database.ps1`
```powershell
# Run from project root
.\seed-database.ps1

# What it does:
# 1. Checks if application is running
# 2. Calls API endpoints
# 3. Seeds basic data (roles + admin)
# 4. Seeds bulk data (400+ records)
# 5. Displays results
```

**Excel Seeding Script**: `seed-from-excel.ps1`
```powershell
# Import data from Excel files
.\seed-from-excel.ps1

# Supports:
# - Custom asset lists
# - Employee imports
# - Department structures
# - Inventory catalogs
```

#### Method 3: API Endpoints

**Get Statistics**
```http
GET /api/DataSeeding/statistics
Authorization: Bearer {token}

Response:
{
  "roles": 3,
  "users": 11,
  "departments": 8,
  "employees": 10,
  "assets": 400,
  "imagingJobs": 400,
  "inventoryItems": 0
}
```

**Seed Basic Data**
```http
POST /api/DataSeeding/seed-basic
Authorization: Bearer {token}

Creates:
- 3 Roles (Administrator, Technician, User)
- 1 Admin User (admin@buildone.com)
```

**Seed Bulk Data**
```http
POST /api/DataSeeding/seed-bulk
Authorization: Bearer {token}

Creates:
- 8 Departments
- 10 Employees
- 400 Assets
- 400 Imaging Jobs
```

**Seed from Excel**
```http
POST /api/DataSeeding/seed-from-excel
Content-Type: multipart/form-data
Authorization: Bearer {token}

Body: Excel file upload
```

**Preview Excel Data**
```http
POST /api/DataSeeding/preview-excel
Content-Type: multipart/form-data
Authorization: Bearer {token}

Returns: Data preview without saving
```

#### Method 4: Programmatic Seeding

```csharp
// In Program.cs or startup code
using var scope = app.Services.CreateScope();
var dataSeedingService = scope.ServiceProvider
    .GetRequiredService<IDataSeedingService>();
var bulkSeedingService = scope.ServiceProvider
    .GetRequiredService<IBulkDataSeedingService>();

// Seed basic data
await dataSeedingService.SeedDataAsync();

// Seed bulk test data
await bulkSeedingService.SeedBulkDataAsync();
```

### What Gets Seeded

#### Basic Seeding
**3 Roles**:
```
Administrator - Full system access
Technician    - Asset and job management
User          - Basic read access
```

**1 Admin Account**:
```
Email:    admin@buildone.com
Password: Admin123!
Role:     Administrator
Status:   Active
```

#### Bulk Seeding

**8 Departments**:
```
Code  | Department Name
------|--------------------------------
IT    | Information Technology
HR    | Human Resources
FIN   | Finance
OPS   | Operations
SALES | Sales
MKT   | Marketing
CS    | Customer Support
RD    | Research & Development
```

**10 Employees**:
- Randomly generated realistic names
- Email format: `firstname.lastname@buildone.com`
- Phone numbers: `555-0100` to `555-0109`
- Distributed across all departments
- All active status
- Linked to user accounts

**400 Assets** (Realistic Distribution):

**200 Laptops (50%)**:
```
Brands: Dell, HP, Lenovo, Apple, Microsoft, Asus
Models:
- Dell Latitude 5420, 7420
- HP EliteBook 840 G8
- Lenovo ThinkPad X1 Carbon
- MacBook Pro 14", 16"
- Microsoft Surface Laptop 5
- Asus ZenBook 14
```

**140 Desktops (35%)**:
```
Brands: Dell, HP, Lenovo, Acer
Models:
- Dell OptiPlex 7090, 5090
- HP EliteDesk 800 G8
- HP ProDesk 400 G7
- Lenovo ThinkCentre M90a
- Acer Veriton M
```

**60 Tablets (15%)**:
```
Brands: Apple, Samsung, Microsoft, Lenovo
Models:
- iPad Pro 12.9", iPad Air
- Samsung Galaxy Tab S8, S8+
- Microsoft Surface Pro 9, Surface Go 3
- Lenovo Tab P11 Pro
```

**Asset Details**:
- **Asset Tags**: `AST-LAP-00001`, `AST-DES-00001`, `AST-TAB-00001`
- **PC IDs**: `PC-000001` to `PC-000400`
- **Serial Numbers**: `{BRAND}{RANDOM_6_DIGITS}`
- **Status Distribution**:
  - 70% Assigned to employees
  - 20% Active (available)
  - 10% In Stock
- **Warranty**: 12-48 months from creation
- **Specifications**: Realistic processor, RAM, storage configs

**400 Imaging Jobs**:
- **Types**: 100% Imaging (OS installation)
- **Image Versions**: 
  - `WIN11-PRO-23H2-v1`
  - `WIN11-ENT-23H2-v1`
  - `WIN10-PRO-22H2-v3`
  - `UBUNTU-22.04-v2`
  - `MACOS-VENTURA-v1`

**Status Distribution by Age**:
```
Recent Jobs (≤7 days):
- 20% In Progress
- 20% Pending
- 40% Scheduled
- 10% Completed
- 10% Failed

Older Jobs (>7 days):
- 70% Completed
- 15% Failed
- 10% Cancelled
- 5% On Hold
```

**Priority Distribution**:
```
10% Urgent
30% High
40% Normal
20% Low
```

### Seeding Services Architecture

```
IDataSeedingService
├── SeedDataAsync()           - Basic seeding (roles, admin)
└── GetStatisticsAsync()      - Get current counts

IBulkDataSeedingService
├── SeedBulkDataAsync()       - Bulk test data (400+ records)
└── GenerateRealisticData()   - Creates realistic records

IExcelDataSeedingService
├── SeedFromExcelAsync()      - Import from Excel files
├── PreviewExcelDataAsync()   - Preview before import
└── ValidateExcelFormat()     - Check file format
```

### Excel Import Format

**Supported Sheets**:
1. **Assets** - Asset information
2. **Employees** - Employee records
3. **Departments** - Department structure
4. **Inventory** - Inventory items

**Assets Sheet Columns**:
```
AssetTag | PcId | SerialNumber | Brand | Model | Type | 
Processor | RAM | Storage | Status | AssignedTo | Department |
PurchaseDate | WarrantyEndDate | PurchaseCost | Notes
```

**Employees Sheet Columns**:
```
FirstName | LastName | Email | Phone | Department | 
Position | HireDate | Status | OfficeLocation
```

**Validation Rules**:
- Required fields must not be empty
- Email must be valid format
- Dates must be valid format
- Status must match enum values
- Department codes must exist
- Asset tags must be unique
- Serial numbers must be unique

### Seeding Best Practices

1. **Start Fresh**: 
   - Clear existing test data before seeding
   - Use basic seeding for initial setup
   - Use bulk seeding for testing/demo

2. **Production Environments**:
   - Only use basic seeding (roles + admin)
   - Never use bulk seeding in production
   - Import real data via Excel

3. **Development/Testing**:
   - Use bulk seeding for realistic test data
   - Reset and re-seed as needed
   - Experiment with different scenarios

4. **Data Integrity**:
   - Seeding is idempotent (safe to run multiple times)
   - Existing records are skipped (no duplicates)
   - Validation ensures data quality

### Seeding Troubleshooting

**Common Issues**:

1. **"Application not running"**
   - Start application: `dotnet run`
   - Wait for startup message
   - Retry seeding

2. **"Unauthorized access"**
   - Login as administrator
   - Check user has correct role
   - Verify policy permissions

3. **"Duplicate key error"**
   - Data already exists
   - Clear existing data first
   - Check for unique constraint violations

4. **"Excel format invalid"**
   - Check column names match exactly
   - Verify data types are correct
   - Remove empty rows/columns

---

## 🔐 Security & Authentication

### Authentication System

#### ASP.NET Core Identity
BuildOne uses ASP.NET Core Identity for robust authentication:
- **User Management** - Create, modify, delete accounts
- **Password Hashing** - PBKDF2 with salt
- **Account Lockout** - Protects against brute force attacks
- **Cookie Authentication** - Secure session management
- **Email Confirmation** - Optional email verification
- **Two-Factor Authentication** - Ready for 2FA implementation

#### Login Process
1. User navigates to `/Account/Login`
2. Enters email and password
3. System validates credentials
4. Creates secure authentication cookie
5. Redirects to dashboard
6. Session maintained until logout or expiry

#### Password Requirements
```
Minimum Length: 8 characters
Required Components:
- ✓ Uppercase letter
- ✓ Lowercase letter
- ✓ Number
- ✓ Special character
Unique Characters: 3 minimum
Strength: Medium to Strong
```

#### Lockout Policy
```
Failed Attempts Allowed: 5
Lockout Duration: 15 minutes
Applies to: All users (including new users)
Reset: After successful login
```

#### Cookie Settings
```
HttpOnly: true          - Prevents JavaScript access
Secure: true            - HTTPS only
SameSite: Strict        - CSRF protection
Expiration: Sliding     - Extends with activity
Login Path: /Account/Login
Logout Path: /Account/Logout
Access Denied: /Account/AccessDenied
```

### Authorization System

#### Policy-Based Authorization
BuildOne uses policy-based authorization for granular access control:

```csharp
// 15+ Defined Policies
CanManageUsers           - User administration
CanManageRoles           - Role administration
CanManageAssets          - Asset CRUD operations
CanManageEmployees       - Employee management
CanManageDepartments     - Department management
CanManageInventory       - Inventory operations
CanManageImagingJobs     - Job management
CanAccessSystemSettings  - System configuration
CanViewReports           - Report access
CanExportData            - Data export
CanImportData            - Data import
CanManageNotifications   - Notification settings
CanAccessAuditLog        - Audit trail access
CanManageAttachments     - File management
CanScanQRCodes           - Scanner access
```

#### Role Permissions Matrix

```
Permission                  | Admin | Technician | User
----------------------------|-------|------------|------
CanManageUsers              |   ✓   |            |
CanManageRoles              |   ✓   |            |
CanManageAssets             |   ✓   |     ✓      |  ✓*
CanManageEmployees          |   ✓   |            |
CanManageDepartments        |   ✓   |            |
CanManageInventory          |   ✓   |     ✓      |
CanManageImagingJobs        |   ✓   |     ✓      |  ✓*
CanAccessSystemSettings     |   ✓   |            |
CanViewReports              |   ✓   |     ✓      |  ✓
CanExportData               |   ✓   |     ✓      |
CanImportData               |   ✓   |            |
CanManageNotifications      |   ✓   |     ✓      |
CanAccessAuditLog           |   ✓   |            |
CanManageAttachments        |   ✓   |     ✓      |
CanScanQRCodes              |   ✓   |     ✓      |  ✓

* = Read-only or limited access
```

#### Attribute-Based Protection

**Controller Level**:
```csharp
[Authorize] // Requires authentication
public class AssetsController : Controller { }

[Authorize(Policy = Policies.CanManageUsers)]
public class UserManagementController : Controller { }
```

**Action Level**:
```csharp
[Authorize(Policy = Policies.CanManageAssets)]
public async Task<IActionResult> Create() { }

[AllowAnonymous] // Public access
public IActionResult Privacy() { }
```

#### Security Features

**1. Global Exception Middleware**
- Catches all unhandled exceptions
- Logs security events
- Returns safe error messages
- Prevents information disclosure

**2. Input Validation**
- FluentValidation for complex rules
- Data Annotations for simple validation
- Anti-XSS protection
- SQL injection prevention (EF Core parameterization)

**3. CSRF Protection**
- Anti-forgery tokens on all forms
- Validated on POST requests
- Automatic token generation

**4. Audit Logging**
- All user actions logged
- Serilog structured logging
- Tamper-evident logs
- Retention policies

**5. Data Protection**
- Sensitive data encrypted at rest
- TLS 1.2+ for data in transit
- Password hashing with PBKDF2
- Secure key management

### Default Admin Account

**Initial Setup**:
```
Email:    admin@buildone.com
Password: Admin123!
Role:     Administrator
Status:   Active
```

**First Login Actions**:
1. Login with default credentials
2. Navigate to User Management
3. Change admin password immediately
4. Create additional admin accounts
5. Review and update security settings

**Security Notes**:
- Change default password in production
- Create multiple admin accounts for redundancy
- Never share admin credentials
- Enable 2FA for admin accounts (when implemented)

---

## ⚡ Advanced Features

### 1. File Attachments
- **Attach to Assets** - Photos, receipts, manuals
- **Attach to Jobs** - Screenshots, logs, reports
- **Storage** - Local file system or cloud storage
- **File Types** - PDF, images, documents, archives
- **Size Limits** - Configurable max file size
- **Security** - Access control per attachment

### 2. Notification System
- **Real-time Alerts** - System events and updates
- **Email Notifications** - Important actions
- **In-app Notifications** - Badge counters
- **Configurable Rules** - Custom notification triggers
- **Delivery Options** - Email, SMS (future), in-app

**Notification Triggers**:
- Asset assigned to employee
- Job status changed
- Warranty expiring soon
- Low stock alert
- User account created/modified
- System errors or warnings

### 3. Search & Filtering
- **Global Search** - Search across all modules
- **Advanced Filters** - Multiple criteria
- **Saved Searches** - Quick access to common queries
- **Real-time Results** - Instant feedback
- **Export Results** - Download search results

### 4. Audit Trail
- **Action Logging** - Who did what when
- **Change Tracking** - Before/after values
- **User Activity** - Login history, page views
- **System Events** - Startup, shutdown, errors
- **Tamper-Proof** - Write-once logs

### 5. Data Export
- **Excel Export** - XLSX format
- **CSV Export** - Compatible with all tools
- **PDF Reports** - Printable formats
- **Custom Exports** - Select fields and filters
- **Scheduled Exports** - Automatic recurring exports

### 6. Health Checks
**Endpoint**: `/health`

**Checks**:
- Database connectivity
- Disk space availability
- Memory usage
- Application responsiveness
- External service status

**Response**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.050"
    },
    "storage": {
      "status": "Healthy",
      "duration": "00:00:00.010"
    }
  }
}
```

### 7. API Documentation
**Endpoint**: `/swagger`

**Features**:
- Interactive API explorer
- Try-out functionality
- Request/response examples
- Schema documentation
- Authentication support

---

## 🏗️ Technical Architecture

### Application Structure

```
buildone/
├── Controllers/           # MVC Controllers & API Controllers
│   ├── HomeController.cs
│   ├── AssetsController.cs
│   ├── ImagingJobsController.cs
│   ├── UserManagementController.cs
│   └── ...
├── Pages/                 # Razor Pages
│   ├── Assets/
│   ├── Imaging/
│   ├── Inventory/
│   └── ...
├── Views/                 # MVC Views
│   ├── Home/
│   ├── UserManagement/
│   └── Shared/
├── Data/                  # Database Context & Entities
│   ├── ApplicationDbContext.cs
│   ├── ApplicationUser.cs
│   ├── Asset.cs
│   └── ...
├── Services/              # Business Logic Services
│   ├── AssetService.cs
│   ├── ImagingJobService.cs
│   ├── DataSeedingService.cs
│   └── ...
├── DTOs/                  # Data Transfer Objects
│   ├── ApiResponse.cs
│   ├── AssetDtos.cs
│   └── ...
├── Validators/            # FluentValidation Validators
│   ├── CreateAssetValidator.cs
│   └── ...
├── Mapping/               # AutoMapper Profiles
│   └── MappingProfile.cs
├── Authorization/         # Authorization Policies
│   ├── Policies.cs
│   └── PermissionRequirement.cs
├── Middleware/            # Custom Middleware
│   └── GlobalExceptionMiddleware.cs
└── wwwroot/              # Static Files
    ├── css/
    ├── js/
    └── lib/
```

### Database Schema

**Core Tables**:
```
AspNetUsers              - User accounts (Identity)
AspNetRoles              - Roles (Identity)
AspNetUserRoles          - User-role assignments
Departments              - Organization departments
Employees                - Employee records
Assets                   - IT assets
AssetHistory             - Asset change history
ImagingJobs              - Imaging & maintenance jobs
JobComments              - Job comments/notes
JobAttachments           - Job file attachments
AssetAttachments         - Asset file attachments
Inventories              - Inventory items
InventoryTransactions    - Inventory movement history
MaintenanceHistory       - Maintenance records
Notifications            - User notifications
```

**Key Relationships**:
```
User →→ Employee (One-to-One)
Employee →→ Department (Many-to-One)
Asset →→ Employee (Many-to-One) - Assigned to
Asset →→ Department (Many-to-One) - Belongs to
ImagingJob →→ Asset (Many-to-One)
ImagingJob →→ Employee (Many-to-One) - Technician
AssetHistory →→ Asset (Many-to-One)
JobComment →→ ImagingJob (Many-to-One)
```

### Performance Optimizations

**1. Database Indexing**
```sql
-- Composite Indexes for Common Queries
CREATE INDEX IX_Users_IsActive_Email 
  ON AspNetUsers (IsActive, Email);

CREATE INDEX IX_Employees_DepartmentId_FullName 
  ON Employees (DepartmentId, FullName);

CREATE INDEX IX_Assets_Status_AssignedEmployeeId 
  ON Assets (Status, AssignedEmployeeId);

CREATE INDEX IX_Assets_DepartmentId_Status 
  ON Assets (DepartmentId, Status);

CREATE INDEX IX_Assets_Status_CreatedAt 
  ON Assets (Status, CreatedAt);
```

**2. Query Optimization**
```csharp
// AsNoTracking for read-only queries (70% faster)
var assets = await _context.Assets
    .Include(a => a.AssignedEmployee)
    .Include(a => a.Department)
    .AsNoTracking()
    .ToListAsync();
```

**3. Caching Strategy**
- In-memory caching for reference data
- Distributed caching for session data
- Output caching for static pages

**4. Connection Pooling**
```csharp
// Built-in EF Core connection pooling
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));
```

### Logging Architecture

**Serilog Configuration**:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BuildOne")
    .WriteTo.Console()
    .WriteTo.File("logs/buildone-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

**Log Levels**:
```
Verbose     - Detailed trace information
Debug       - Internal system events
Information - General information (default)
Warning     - Abnormal or unexpected events
Error       - Error events that don't stop the application
Fatal       - Critical errors that crash the application
```

**Structured Logging Example**:
```csharp
_logger.LogInformation(
    "Asset {AssetTag} assigned to {EmployeeName} by {User}",
    asset.AssetTag,
    employee.FullName,
    User.Identity.Name);
```

### Error Handling

**Global Exception Middleware**:
```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();

// Catches all exceptions
// Logs with Serilog
// Returns appropriate HTTP status
// Provides safe error messages
```

**HTTP Status Codes**:
```
200 OK              - Successful request
201 Created         - Resource created
204 No Content      - Successful delete
400 Bad Request     - Validation error
401 Unauthorized    - Authentication required
403 Forbidden       - Insufficient permissions
404 Not Found       - Resource not found
409 Conflict        - Duplicate or constraint violation
500 Internal Error  - Server error
503 Service Unavailable - Database connection failed
```

### Deployment Considerations

**Development Environment**:
```
dotnet run
https://localhost:5001
```

**Production Environment**:
```
dotnet publish -c Release
Deploy to IIS, Azure App Service, or Docker
Configure connection strings
Enable HTTPS
Set production logging levels
Configure backups
```

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<connection_string>
Serilog__MinimumLevel=Warning
```

---

## 📊 Usage Workflow Examples

### Example 1: New Employee Onboarding

**Scenario**: New employee John Doe joins IT department

**Steps**:
1. **HR Creates Employee Record**
   - Navigate to Employees → Create New
   - Enter: Name, Email, Phone, Department (IT)
   - Save employee

2. **Admin Creates User Account**
   - Navigate to User Management → Create New
   - Enter: Email (john.doe@buildone.com)
   - Assign Role: User or Technician
   - Link to Employee: John Doe
   - Save and send welcome email

3. **IT Assigns Assets**
   - Navigate to Assets
   - Find available laptop (Status: Active)
   - Click Edit
   - Assign to: John Doe
   - Status: Assigned
   - Save changes

4. **IT Creates Imaging Job**
   - Navigate to Jobs → New Imaging Job
   - Asset: John's laptop
   - Imaging Type: Windows 11 Pro
   - Priority: Normal
   - Scheduled: Today
   - Assign to Technician
   - Save job

5. **Technician Completes Job**
   - Navigate to Jobs → Queue
   - Find John's laptop job
   - Change status: In Progress
   - Complete imaging
   - Add comment: "OS installed, drivers updated"
   - Change status: Completed

6. **Delivery to Employee**
   - Employee receives laptop
   - Asset status remains: Assigned
   - Ready for use

### Example 2: Asset Maintenance Workflow

**Scenario**: Laptop LAP-00123 needs keyboard repair

**Steps**:
1. **User Reports Issue**
   - Search for laptop LAP-00123
   - View asset details
   - Create maintenance job
   - Priority: High
   - Notes: "Keyboard not responding"

2. **Technician Receives Assignment**
   - Views Jobs → Queue
   - Sees high-priority maintenance job
   - Changes status: In Progress
   - Asset automatically marked: Maintenance

3. **Repair Process**
   - Technician diagnoses issue
   - Orders replacement keyboard
   - Adds comment: "Keyboard ordered, ETA 2 days"
   - Attaches vendor invoice

4. **Repair Completion**
   - Replaces keyboard
   - Tests functionality
   - Adds comment: "Keyboard replaced, tested OK"
   - Changes status: Completed
   - Asset returns to: Assigned status

5. **History Tracking**
   - All actions logged in Asset History
   - Maintenance record created
   - Cost tracked for reporting

### Example 3: Inventory Restock

**Scenario**: Monitor low stock and reorder

**Steps**:
1. **Dashboard Alert**
   - View dashboard
   - See "Low Stock: 5 items"
   - Click to view details

2. **Inventory Review**
   - Navigate to Inventory
   - Filter: Low Stock
   - Review items below reorder level
   - Export list to Excel

3. **Create Purchase Order**
   - External system: Create PO
   - Order items from supplier

4. **Receive Stock**
   - Navigate to Inventory
   - Find received items
   - Click Edit
   - Increase quantity
   - Add transaction note: "Received PO #12345"
   - Save changes

5. **Stock Updated**
   - Dashboard updates automatically
   - Low stock count decreases
   - Items now show: In Stock or Fully Stocked

---

## 🎓 Training & Support

### User Training Topics

**Basic Users**:
- Dashboard navigation
- Asset search and lookup
- QR code scanning
- Viewing assigned assets
- Creating basic reports

**Technicians**:
- Job queue management
- Creating imaging jobs
- Updating job status
- Asset assignment
- Inventory operations

**Administrators**:
- User management
- Role management
- System settings
- Data seeding
- Backup procedures
- Security best practices

### Quick Reference Cards

**Common Keyboard Shortcuts**:
```
Ctrl + F     - Focus search box
Ctrl + S     - Save form (when in form)
Esc          - Close modal/cancel
Enter        - Submit form (when in input)
```

**Status Color Codes**:
```
Green    - Active, Completed, Success
Blue     - In Progress, Scheduled
Yellow   - Warning, Low Stock, Pending
Red      - Error, Failed, Out of Stock
Gray     - Inactive, Cancelled, Archived
```

---

## 📞 Support & Maintenance

### System Health Monitoring
- Check `/health` endpoint regularly
- Monitor log files in `logs/` directory
- Review error logs daily
- Check disk space for logs and uploads

### Backup Procedures
1. **Database Backups**
   - Schedule daily SQL Server backups
   - Test restore procedures monthly
   - Store off-site copies

2. **File Storage Backups**
   - Backup `wwwroot/uploads` directory
   - Include attachment files
   - Maintain version history

3. **Configuration Backups**
   - Backup `appsettings.json`
   - Document environment variables
   - Version control IIS settings

### Common Issues & Solutions

**Issue**: Can't login
- **Solution**: Check account status, reset password, verify email

**Issue**: Asset not appearing in search
- **Solution**: Check asset status, refresh page, verify filters

**Issue**: Job not in queue
- **Solution**: Check job status, verify scheduled date, check priority

**Issue**: Excel import fails
- **Solution**: Verify file format, check column names, validate data

---

## 🎯 Conclusion

BuildOne Asset Management System provides a comprehensive, secure, and user-friendly solution for managing IT assets throughout their lifecycle. From the intuitive dashboard to advanced features like QR scanning and bulk data import, the system streamlines asset management operations.

**Key Takeaways**:
- ✅ Centralized asset tracking and management
- ✅ Role-based security with granular permissions
- ✅ Automated workflows for jobs and notifications
- ✅ Flexible data import/export capabilities
- ✅ Real-time monitoring and reporting
- ✅ Scalable architecture for growth
- ✅ Comprehensive audit trail for compliance

For questions, support, or feature requests, contact your system administrator or the BuildOne support team.

---

**Document Version**: 1.0  
**Last Updated**: October 25, 2025  
**System Version**: BuildOne 1.0.0
