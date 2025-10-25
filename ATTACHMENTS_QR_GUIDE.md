# File Attachments & QR Code Features - Implementation Guide

## Overview
This document describes the file attachment and QR code features added to the BuildOne asset management system.

## Features Implemented

### 1. File Attachments System

#### Backend Components

**Database Entities** (Already existed):
- `JobAttachment` - Stores metadata for files attached to imaging jobs
- `AssetAttachment` - Stores metadata for files attached to assets

**File Storage Service** (`Services/FileStorageService.cs`):
- Saves uploaded files to `wwwroot/uploads/{folder}/{guid_filename}`
- Provides file download streams
- Handles file deletion
- Validates file existence

**API Endpoints** (`Controllers/AttachmentsController.cs`):

Upload Endpoints:
- `POST /api/attachments/job/{jobId}` - Upload file to imaging job
- `POST /api/attachments/asset/{assetId}` - Upload file to asset

List Endpoints:
- `GET /api/attachments/job/{jobId}` - Get all job attachments
- `GET /api/attachments/asset/{assetId}` - Get all asset attachments

Download Endpoints:
- `GET /api/attachments/download/job/{attachmentId}` - Download job attachment
- `GET /api/attachments/download/asset/{attachmentId}` - Download asset attachment

Delete Endpoints:
- `DELETE /api/attachments/job/{attachmentId}` - Delete job attachment (requires `CanManageImagingJobs` policy)
- `DELETE /api/attachments/asset/{attachmentId}` - Delete asset attachment (requires `CanManageAssets` policy)

**Supported File Types**:
- Images: .jpg, .jpeg, .png, .gif, .bmp, .webp
- Documents: .pdf, .doc, .docx, .xls, .xlsx, .txt, .csv
- Maximum file size: 10 MB per file

#### Frontend Components

**Imaging Job Details** (`Pages/Imaging/Details.cshtml`):
- Attachments card in right sidebar
- File input with description field
- Upload and Refresh buttons
- Attachment list with thumbnails (for images)
- Download and Delete buttons for each attachment
- Client-side JavaScript for AJAX upload/delete operations

**Asset Details** (`Pages/Assets/Details.cshtml`):
- Attachments card in right sidebar
- Same UI pattern as imaging job attachments
- Integrated with asset-specific API endpoints

#### Usage Examples

**Upload a File**:
1. Navigate to Imaging Job Details or Asset Details page
2. In the Attachments card, click "Select file" and choose a file
3. Optionally add a description
4. Click "Upload Attachment"
5. File uploads via AJAX and the list refreshes automatically

**Download a File**:
- Click the "Download" button next to any attachment
- File opens in a new tab or downloads depending on browser settings

**Delete a File**:
- Click the "Delete" button next to any attachment
- Confirm the deletion
- File is removed from storage and database

### 2. QR Code System

#### Backend Components

**QR Code Service** (`Services/QRCodeService.cs`):
- Uses QRCoder library to generate QR codes
- `GenerateAssetQRCode(assetTag, size)` - Generates PNG bytes
- `GenerateAssetQRCodeBase64(assetTag, size)` - Returns base64 string
- `GenerateAssetURLQRCode(assetId, baseUrl, size)` - Generates QR with URL to asset details

**API Endpoints** (`Controllers/QRCodeController.cs`):

Generate Endpoints:
- `GET /api/qrcode/asset/{assetId}/generate` - Download QR code PNG (asset tag only)
- `GET /api/qrcode/asset/{assetId}/generate-url` - Download QR code PNG (full URL to details page)
- `GET /api/qrcode/asset/{assetId}/base64` - Get QR code as base64 JSON (for inline display)

Lookup Endpoint:
- `POST /api/qrcode/lookup` - Lookup asset by scanned QR code data
  - Accepts: `{ "scannedData": "ASSET-001" }` (asset tag or URL)
  - Returns: Full asset details including ID, tag, status, assignments, etc.
  - Returns 404 if asset not found

#### Frontend Components

**Asset Details QR Display** (`Pages/Assets/Details.cshtml`):
- QR Code card in right sidebar
- Displays QR code inline using base64 image
- "Download URL QR" button to download PNG file
- QR encodes the full URL to the asset details page

**QR Code Scanner Page** (`Pages/Assets/Scan.cshtml`):
- Accessible via `/Assets/Scan` or "Scan QR Code" button on Assets Index
- Uses html5-qrcode library (loaded from CDN)
- Two input methods:
  1. **Camera Scanner**: Opens device camera to scan QR codes in real-time
  2. **Manual Entry**: Type or paste asset tag directly

**Scanner Features**:
- Real-time camera QR scanning with visual feedback
- Automatic asset lookup after successful scan
- Displays found asset details (tag, brand, model, status, assignment, etc.)
- "View Full Details" button to navigate to asset details page
- "Asset Not Found" handling with "Create New Asset" button (pre-fills asset tag)
- "Scan Another" button to quickly scan multiple assets

#### Usage Examples

**Display QR Code on Asset**:
1. Navigate to Asset Details page
2. QR Code card in sidebar shows the QR code image
3. Click "Download URL QR" to save PNG file for printing

**Scan QR Code to Lookup Asset**:
1. Go to Assets > "Scan QR Code" button
2. Click "Start Camera Scanner"
3. Allow camera permissions in browser
4. Point camera at QR code
5. Scanner automatically reads code and looks up asset
6. Asset details displayed with link to full details page

**Manual Asset Lookup**:
1. On Scan page, enter asset tag in the input field
2. Click "Lookup" or press Enter
3. Asset details displayed if found

**Create Asset from Scan**:
1. Scan or enter an asset tag that doesn't exist
2. "Asset Not Found" card displays
3. Click "Create New Asset"
4. Redirects to Create Asset page with asset tag pre-filled
5. Complete the form and save

## Technical Architecture

### File Storage
- **Location**: `wwwroot/uploads/`
- **Structure**: `{type}/{id}/{guid}_{filename}`
  - Example: `wwwroot/uploads/jobs/5/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf`
- **File naming**: GUID prefix prevents filename collisions

### Security
- All endpoints require `[Authorize]` attribute (user must be logged in)
- Delete endpoints protected by policies:
  - `CanManageImagingJobs` for job attachments
  - `CanManageAssets` for asset attachments
- File type validation on upload (whitelist approach)
- File size validation (10 MB limit)

### Client-Side Implementation
- Uses Fetch API for AJAX requests with `credentials: 'same-origin'` for cookie auth
- FormData used for multipart file uploads
- Error handling with user-friendly alerts
- Automatic UI refresh after upload/delete operations

### QR Code Library
- **Backend**: QRCoder (NuGet package v1.6.0)
- **Frontend**: html5-qrcode (CDN v2.3.8)
- QR codes use error correction level Q (medium-high)

## Configuration

### Required NuGet Packages
```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
```

### Dependency Injection Registration (Program.cs)
```csharp
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
```

### File Size Limit
Currently set to 10 MB in `AttachmentsController.cs`:
```csharp
private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
```

To change, update this constant and rebuild.

## Browser Compatibility

### Camera Scanner Requirements
- HTTPS required (or localhost for development)
- Modern browser with camera access (Chrome, Firefox, Safari, Edge)
- User must grant camera permissions

### Fallback Options
- Manual entry input always available
- QR code download works in all browsers
- File upload/download works in all modern browsers

## Testing Checklist

### File Attachments
- [ ] Upload image to imaging job
- [ ] Upload document to asset
- [ ] View image thumbnail in attachment list
- [ ] Download attachment
- [ ] Delete attachment (verify file removed from disk)
- [ ] Try uploading file > 10 MB (should fail)
- [ ] Try uploading unsupported file type (should fail)
- [ ] Upload multiple files to same job/asset

### QR Codes
- [ ] View QR code on asset details page
- [ ] Download QR code PNG
- [ ] Scan QR code with camera on mobile device
- [ ] Manual lookup using asset tag
- [ ] Lookup existing asset (verify details displayed)
- [ ] Lookup non-existent asset (verify "not found" message)
- [ ] Create new asset from scan (verify asset tag pre-filled)
- [ ] Scan multiple assets in sequence

## Future Enhancements

### Attachments
- [ ] Add bulk delete for attachments
- [ ] Add attachment preview modal (PDF viewer, image lightbox)
- [ ] Add drag-and-drop file upload
- [ ] Add progress bar for large file uploads
- [ ] Add file search/filter in attachment lists
- [ ] Add attachment categories/tags
- [ ] Implement file versioning

### QR Codes
- [ ] Add QR code printing template page
- [ ] Add batch QR code generation for multiple assets
- [ ] Add QR code scanner to mobile app (if applicable)
- [ ] Add configurable QR code size
- [ ] Add custom QR code branding/logo
- [ ] Add QR code inventory tracking workflow
- [ ] Generate QR codes for other entities (employees, departments)

## Troubleshooting

### Camera Scanner Not Working
1. **Check HTTPS**: Camera access requires HTTPS (or localhost)
2. **Check Permissions**: User must allow camera access in browser
3. **Check Browser Support**: Use modern browser (Chrome/Firefox/Safari/Edge)
4. **Fallback**: Use manual entry if camera unavailable

### File Upload Fails
1. **Check File Size**: Must be ≤ 10 MB
2. **Check File Type**: Must be in allowed extensions list
3. **Check Disk Space**: Ensure server has sufficient storage
4. **Check Permissions**: Ensure wwwroot/uploads folder is writable

### QR Code Not Displaying
1. **Check API Response**: Open browser dev tools and check /api/qrcode/asset/{id}/base64
2. **Check Asset ID**: Ensure valid asset ID in URL
3. **Check Authorization**: Ensure user is logged in

### Attachment Download Fails
1. **Check File Exists**: File may have been manually deleted from disk
2. **Check Database**: Attachment record may exist but file missing
3. **Check Permissions**: Ensure web server can read uploads folder

## API Response Examples

### Upload Attachment Success
```json
{
  "id": 42,
  "fileName": "invoice.pdf",
  "fileSize": "1.2 MB",
  "uploadedAt": "2025-10-25T10:30:00Z",
  "message": "File uploaded successfully"
}
```

### List Attachments Response
```json
[
  {
    "id": 42,
    "fileName": "invoice.pdf",
    "fileSize": "1.2 MB",
    "contentType": "application/pdf",
    "description": "Q1 Invoice",
    "uploadedBy": "john.doe@company.com",
    "uploadedAt": "2025-10-25T10:30:00Z",
    "isImage": false,
    "isDocument": true,
    "downloadUrl": "/api/attachments/download/asset/42"
  }
]
```

### QR Lookup Success
```json
{
  "asset": {
    "id": 123,
    "assetTag": "ASSET-001",
    "pcId": "PC-2024-001",
    "brand": "Dell",
    "model": "Latitude 7420",
    "serialNumber": "SN123456",
    "type": "Laptop",
    "status": "Active",
    "warrantyExpiry": "2026-12-31T00:00:00Z",
    "assignedTo": {
      "id": 5,
      "fullName": "John Doe",
      "email": "john.doe@company.com"
    },
    "department": {
      "id": 2,
      "name": "IT Department",
      "code": "IT"
    },
    "detailsUrl": "/Assets/Details/123"
  },
  "lookupMethod": "assetTag"
}
```

## Performance Considerations

- **File Storage**: Local filesystem storage is simple but consider cloud storage (Azure Blob, AWS S3) for scalability
- **QR Generation**: QR codes are generated on-demand; consider caching for frequently accessed assets
- **Attachment Lists**: Paginate if assets have many attachments (>50)
- **Image Thumbnails**: Consider generating thumbnails for large images

## Maintenance

### Cleanup Tasks
- Periodically check for orphaned files (files without database records)
- Monitor disk usage in uploads folder
- Archive old attachments from completed/retired assets
- Regular database backup including attachment metadata

---

**Last Updated**: October 25, 2025
**Version**: 1.0
