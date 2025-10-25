# Excel Import Template Guide

## Overview
The BuildOne Asset Management System supports importing data from Excel (.xlsx) files. This guide explains how to create a properly formatted Excel file for data seeding.

## Supported Sheets
Your Excel workbook can contain any combination of the following sheets. All sheets are optional - include only the data you want to import:

- **Departments**
- **Employees**
- **Assets**
- **Inventory**

## Sheet Specifications

### 1. Departments Sheet

**Required Columns:**
- `Name` - Department name (e.g., "Information Technology")

**Optional Columns:**
- `Code` - Department code (e.g., "IT")

**Example:**
| Name | Code |
|------|------|
| Information Technology | IT |
| Human Resources | HR |
| Finance | FIN |
| Sales | SALES |

**Duplicate Handling:**
- Records are skipped if a department with the same `Code` or `Name` already exists

---

### 2. Employees Sheet

**Required Columns (at least one):**
- `Username` - Employee username
- `Email` - Employee email address

**Optional Columns:**
- `FullName` or `Name` - Employee's full name
- `Phone` - Phone number
- `DepartmentCode` or `Department` - Links to Department by Code or Name

**Example:**
| FullName | Username | Email | Phone | DepartmentCode |
|----------|----------|-------|-------|----------------|
| John Smith | john.smith | john.smith@company.com | +1-555-1234 | IT |
| Jane Doe | jane.doe | jane.doe@company.com | +1-555-5678 | HR |

**Duplicate Handling:**
- Records are skipped if an employee with the same `Username` or `Email` already exists

**Notes:**
- If `DepartmentCode` is provided, it must match an existing department's `Code` or `Name`
- If `FullName` is missing, the system uses `Username` as the display name

---

### 3. Assets Sheet

**Recommended Columns:**
- `AssetTag` - Unique asset identifier (e.g., "AST-LAP-00001")
- `SerialNumber` or `Serial` - Device serial number

**Optional Columns:**
- `PcId` or `PCID` - PC/Computer ID
- `Brand` - Manufacturer (e.g., "Dell", "HP", "Apple")
- `Model` - Device model (e.g., "Latitude 5420")
- `Type` or `Category` - Asset type (e.g., "Laptop", "Desktop", "Tablet")
- `AssignedTo` or `AssignedUsername` - Username or email of assigned employee

**Example:**
| AssetTag | PcId | Brand | Model | SerialNumber | Type | AssignedTo |
|----------|------|-------|-------|--------------|------|------------|
| AST-LAP-00001 | PC-000001 | Dell | Latitude 5420 | DL123456 | Laptop | john.smith |
| AST-DSK-00001 | PC-000002 | HP | EliteDesk 800 | HP789012 | Desktop | jane.doe |

**Duplicate Handling:**
- Records are skipped if an asset with the same `AssetTag` or `SerialNumber` already exists

**Notes:**
- If `AssignedTo` is provided, the system will look up the employee by `Username` or `Email`
- If the employee is not found, the asset will be created but not assigned

---

### 4. Inventory Sheet

**Required Columns:**
- `ItemName` or `Name` - Name of the inventory item

**Optional Columns:**
- `SKU` - Stock Keeping Unit (auto-generated if not provided)
- `Description` - Item description
- `Category` - Item category (e.g., "Computer Accessories", "Office Supplies")
- `Unit` - Unit of measurement (e.g., "pcs", "box", "pack")
- `Supplier` - Supplier name
- `CurrentQuantity` - Current stock quantity (default: 0)
- `MinimumQuantity` - Minimum stock level (default: 0)
- `MaximumQuantity` - Maximum stock level (default: 0)
- `WarrantyPeriodMonths` - Warranty period in months
- `WarrantyStartDate` - Warranty start date (format: YYYY-MM-DD or Excel date)
- `WarrantyEndDate` - Warranty end date (format: YYYY-MM-DD or Excel date)

**Example:**
| ItemName | SKU | Category | Unit | CurrentQuantity | MinimumQuantity | MaximumQuantity | Supplier |
|----------|-----|----------|------|-----------------|-----------------|-----------------|----------|
| Dell USB Keyboard | KB-001 | Computer Accessories | pcs | 50 | 20 | 100 | TechSupply Co. |
| A4 Printer Paper | PP-500 | Office Supplies | pack | 200 | 100 | 500 | OfficeMax Pro |

**Duplicate Handling:**
- Records are skipped if an inventory item with the same `SKU` or `ItemName` already exists

**Notes:**
- If `SKU` is not provided, the system will auto-generate one (format: INV-XXXXXXXX)
- Stock status is automatically calculated based on quantity levels

---

## General Guidelines

### Column Names
- **Case-Insensitive:** Column names are not case-sensitive ("Name", "name", "NAME" are all valid)
- **Flexible Matching:** Some columns accept alternate names (e.g., "Name" or "FullName" for employees)
- **Extra Columns:** Additional columns in your Excel file will be ignored

### Data Types
- **Text:** Most fields accept text (strings)
- **Numbers:** Quantity fields expect integers (whole numbers)
- **Dates:** Date fields accept:
  - Excel date values (numeric serial dates)
  - Text dates in format: YYYY-MM-DD, MM/DD/YYYY, etc.

### Best Practices
1. **Use the first row for headers** - The first row must contain column names
2. **No empty sheets** - If a sheet is included, it should have at least one data row
3. **Clean data** - Remove empty rows between data
4. **Test with small files first** - Start with a few rows to validate your format
5. **Check for duplicates** - The system will skip duplicates but won't report warnings

### Foreign Key Relationships
The system automatically resolves relationships:
- Employees → Departments (by `DepartmentCode`)
- Assets → Employees (by `AssignedTo` username/email)

If a referenced record doesn't exist:
- For Employees: The record is created without a department link
- For Assets: The record is created without an assigned employee

---

## Import Process

### Using PowerShell Script
```powershell
# Basic usage
.\seed-from-excel.ps1 -ExcelFilePath ".\seed-data.xlsx"

# With authentication
$pass = ConvertTo-SecureString "Admin123!" -AsPlainText -Force
.\seed-from-excel.ps1 -ExcelFilePath ".\seed-data.xlsx" -Username "admin@buildone.com" -Password $pass

# With custom API URL
.\seed-from-excel.ps1 -ExcelFilePath ".\seed-data.xlsx" -ApiUrl "https://myserver.com"
```

### Expected Output
```
========================================
  BuildOne Excel Data Seeding Tool
========================================

Excel File: C:\data\seed-data.xlsx
API URL: https://localhost:5001

Uploading Excel file and seeding database...

========================================
  ✓ Seeding Completed Successfully!
========================================

Results:
  Departments: 4 created
  Employees: 10 created
  Assets: 25 created
  Inventory: 50 created

Timestamp: 2025-10-25T14:30:00Z
```

---

## Example Excel File Structure

```
📊 seed-data.xlsx
├── 📄 Departments
│   ├── Name | Code
│   ├── Information Technology | IT
│   ├── Human Resources | HR
│   └── Finance | FIN
│
├── 📄 Employees
│   ├── FullName | Username | Email | Phone | DepartmentCode
│   ├── John Smith | john.smith | john.smith@company.com | +1-555-1234 | IT
│   └── Jane Doe | jane.doe | jane.doe@company.com | +1-555-5678 | HR
│
├── 📄 Assets
│   ├── AssetTag | Brand | Model | SerialNumber | Type | AssignedTo
│   ├── AST-LAP-001 | Dell | Latitude 5420 | DL123456 | Laptop | john.smith
│   └── AST-DSK-001 | HP | EliteDesk 800 | HP789012 | Desktop | jane.doe
│
└── 📄 Inventory
    ├── ItemName | SKU | Category | Unit | CurrentQuantity | MinimumQuantity
    ├── Dell USB Keyboard | KB-001 | Computer Accessories | pcs | 50 | 20
    └── A4 Printer Paper | PP-500 | Office Supplies | pack | 200 | 100
```

---

## Troubleshooting

### Common Issues

**Error: "No file uploaded or file is empty"**
- Solution: Ensure the Excel file path is correct and the file exists

**Error: "Authentication failed"**
- Solution: Check username and password; ensure you have Administrator role

**No records created (all 0)**
- Cause: All records may already exist (duplicates)
- Solution: Check database for existing records with same identifiers

**Some records not created**
- Cause: Likely duplicates or missing required fields
- Solution: Check logs for detailed error messages

**Foreign key issues (e.g., Employee has no department)**
- Cause: Referenced record doesn't exist yet
- Solution: Ensure sheets are in correct order or pre-create referenced records

### Validation Tips
1. Open your Excel file and verify:
   - First row contains headers
   - No completely empty rows
   - Column names match expected format
2. Test with a small subset first (2-3 rows per sheet)
3. Check application logs for detailed error messages
4. Use Swagger UI to test the API endpoint directly

---

## Security Notes
- Only users with Administrator role can import data
- The import script requires authentication
- Uploaded files are processed in-memory and not permanently stored on the server
- All import operations are logged with the user who performed them

---

## Next Steps
After successful import:
1. Navigate to the respective pages to verify imported data:
   - Departments: `/Departments`
   - Employees: `/Employees`
   - Assets: `/Assets`
   - Inventory: `/Inventory`
2. Check statistics: `/Admin/SeedData` or API endpoint `/api/DataSeeding/statistics`
3. Review application logs for any warnings or skipped records

---

**Last Updated:** October 25, 2025
