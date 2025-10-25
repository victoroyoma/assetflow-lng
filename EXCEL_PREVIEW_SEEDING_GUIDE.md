# Excel Preview Seeding Guide

## Overview

The Database Seeding Management page now includes a **preview-before-seed** workflow for Excel imports. This allows administrators to upload an Excel file, preview the data in an organized tabbed interface, and confirm before actually seeding the database.

## Features

### 1. Upload and Preview
- Upload Excel files (.xlsx format)
- Client-side parsing using SheetJS
- Preview data organized by sheet type
- See record counts for each sheet
- Review data in formatted tables before committing

### 2. Supported Sheet Types
The system looks for these specific sheet names:
- **Departments** - Department information
- **Employees** - Employee records
- **Assets** - Asset inventory
- **Inventory** - Inventory items

### 3. Safety Features
- Preview all data before seeding
- Confirmation prompt before database changes
- Back button to select a different file
- Shows created vs. skipped record counts
- Automatic duplicate detection (handled by backend)

## How to Use

### Step 1: Access Seeding Management
1. Log in with System Administrator credentials
2. Navigate to **Admin** → **Database Seeding**
3. Click **Upload Excel File** button

### Step 2: Select Excel File
1. Click **Choose File** in the upload modal
2. Select your prepared Excel file (.xlsx)
3. Click **Preview Data**

### Step 3: Review Preview
1. The system parses the file client-side (no upload yet)
2. Each sheet appears as a tab with a count badge
3. Review the data in scrollable tables
4. Check for:
   - Correct number of records
   - Proper data formatting
   - Expected columns and values
5. Click **Back to Upload** if you need to select a different file

### Step 4: Seed to Database
1. Click **Seed to Database** button
2. Confirm the action in the prompt
3. Wait for the seeding process (progress spinner shows)
4. Review results:
   - Success message with created/skipped counts
   - Or error message with details
5. Click **OK** to close modal and see updated statistics

## Excel File Format

### Departments Sheet
Required columns:
- **Name** (string) - Department name
- **Description** (string, optional) - Department description

### Employees Sheet
Required columns:
- **EmployeeNumber** (string) - Unique employee identifier
- **FirstName** (string) - First name
- **LastName** (string) - Last name
- **Email** (string) - Email address
- **Department** (string) - Department name (must exist)
- **Position** (string, optional) - Job title

### Assets Sheet
Required columns:
- **AssetTag** (string) - Unique asset identifier
- **AssetType** (string) - Type of asset
- **Manufacturer** (string) - Manufacturer name
- **Model** (string) - Model name
- **AssignedTo** (string, optional) - Employee number
- **Department** (string) - Department name
- **Status** (string) - Available, InUse, Maintenance, Retired
- **PurchaseDate** (date, optional) - Purchase date
- **WarrantyExpiration** (date, optional) - Warranty end date

### Inventory Sheet
Required columns:
- **SKU** (string) - Stock keeping unit
- **Name** (string) - Item name
- **Description** (string, optional) - Item description
- **Category** (string) - Item category
- **Quantity** (number) - Stock quantity
- **Location** (string) - Storage location
- **ReorderLevel** (number) - Minimum quantity before reorder

## Technical Details

### Frontend Implementation
- **Library**: SheetJS (xlsx.full.min.js v0.20.1)
- **Parsing**: Client-side ArrayBuffer reading
- **UI**: Bootstrap 5 modal with tabs
- **API Calls**: Fetch API with FormData

### Backend Endpoint
- **URL**: `POST /api/dataseeding/seed-from-excel`
- **Input**: Multipart form with Excel file
- **Output**: JSON with created/skipped counts per entity type
- **Service**: `IExcelDataSeedingService`

### Workflow Steps
1. User selects file → FileReader reads as ArrayBuffer
2. SheetJS parses workbook → Extract sheet data as JSON
3. Display preview → Generate tabs and tables dynamically
4. User confirms → POST file to API endpoint
5. Backend processes → ClosedXML parses server-side
6. Database seeding → EF Core creates entities
7. Return results → Display success/error message

## Security and Validation

### Client-Side Validation
- File type checking (Excel files only)
- Sheet name validation (only supported sheets)
- Empty data check

### Server-Side Validation
- File format verification (ClosedXML)
- Column validation (required fields)
- Data type validation
- Duplicate checking (by unique identifiers)
- Authorization check (System Administrator role)

### Error Handling
- Invalid file format → Alert with error message
- Missing required sheets → Warning about no data found
- Missing columns → Backend returns validation errors
- Duplicate records → Automatically skipped with count
- Network errors → Display error in results section

## Advantages

### Preview Before Commit
- **Visual Confirmation**: See exactly what will be seeded
- **Error Prevention**: Catch formatting issues before database changes
- **Count Verification**: Ensure expected number of records
- **Sheet Review**: Check each entity type independently

### User Experience
- **No Data Loss**: File stays in browser until confirmed
- **Easy Correction**: Back button to choose different file
- **Clear Feedback**: Detailed success/error messages
- **Live Updates**: Statistics refresh after seeding

### Performance
- **Client-Side Parsing**: Fast preview without server load
- **Single Upload**: File sent only once during actual seeding
- **Batch Processing**: Backend handles all entities efficiently
- **Progress Indicators**: Spinner shows work in progress

## Troubleshooting

### Preview Shows Empty
- **Cause**: Sheet names don't match expected names
- **Solution**: Rename sheets to: Departments, Employees, Assets, Inventory
- **Note**: Sheet names are case-sensitive

### Seeding Fails with Validation Error
- **Cause**: Missing required columns or invalid data
- **Solution**: 
  1. Click "See Format Requirements" in modal
  2. Compare your sheet columns to required columns
  3. Fix Excel file and try again

### Some Records Skipped
- **Cause**: Duplicate identifiers already exist in database
- **Solution**: This is normal - existing records are not overwritten
- **Check**: Review created vs. skipped counts in results

### Network Error During Seeding
- **Cause**: Large file or slow connection
- **Solution**: 
  1. Check file size (recommend < 5MB)
  2. Reduce number of records if possible
  3. Try again - preview doesn't require re-selection

## Best Practices

### File Preparation
1. Use provided format guide (click "See Format Requirements")
2. Keep sheet names exactly as specified
3. Include headers in first row
4. Remove empty rows at the end
5. Validate dates are in Excel date format
6. Check for special characters in text fields

### Testing Strategy
1. Start with small test file (5-10 records per sheet)
2. Verify preview shows all data correctly
3. Check seeding results match preview counts
4. Review database statistics after seeding
5. Scale up to full dataset after validation

### Data Management
1. Seed Departments first (referenced by Employees and Assets)
2. Seed Employees before assigning to Assets
3. Use consistent naming (Department and Employee references)
4. Track which files have been seeded (results show counts)
5. Keep backup of original Excel files

## Related Features

- **Basic Seeding**: Creates roles and default admin user
- **Bulk Seeding**: Generates 400+ test records for demo
- **Statistics Dashboard**: Shows current record counts
- **Seed History**: Check when data was last seeded (timestamp in results)

## Support

For issues or questions about Excel seeding:
1. Check format requirements in the modal
2. Review error messages in results section
3. Verify sheet names and column headers
4. Check application logs for detailed errors
5. Test with sample data first
