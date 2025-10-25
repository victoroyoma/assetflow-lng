# QR Code & Barcode Scanner with Editable Table

## Overview
The enhanced scanner page (`/Assets/Scan`) now supports both **QR codes** and **barcodes** with a review table where scanned items can be edited before saving.

## Key Features

### 1. **Dual Mode Scanning**
- **QR & Barcode** (Default) - Scans both types simultaneously
- **QR Only** - Scans only QR codes
- **Barcode Only** - Scans only barcodes (UPC, EAN, Code 128, Code 39, Code 93)

### 2. **Editable Scanned Items Table**
All scanned items are added to a table where you can:
- **Review** - See all scanned codes before processing
- **Edit** - Modify asset tags or add notes
- **Remove** - Delete unwanted scans
- **Clear All** - Reset the entire table

### 3. **Real-time Asset Lookup**
When an item is scanned, the system automatically:
- Checks if the asset exists in the database
- Displays asset information (tag, brand, model)
- Shows status badges (Found, Not Found, Error)

### 4. **Duplicate Prevention**
- Automatically detects and prevents duplicate scans
- Shows warning message for duplicate items

## How to Use

### Starting the Scanner

1. **Navigate to Scanner Page**
   - Go to Assets > Click "Scan QR/Barcode" button
   - Or visit `/Assets/Scan` directly

2. **Select Scan Mode** (Optional)
   - Choose QR & Barcode (default), QR Only, or Barcode Only
   - Toggle options before starting the scanner

3. **Start Camera**
   - Click "Start Camera Scanner"
   - Allow camera permissions if prompted
   - Point camera at QR codes or barcodes

4. **Scan Multiple Items**
   - Keep scanner running to scan multiple items
   - Each scan is automatically added to the table
   - Duplicate scans are rejected with a warning

5. **Stop Scanner**
   - Click "Stop Scanner" when finished
   - Scanner can be restarted at any time

### Manual Entry

If camera is unavailable or for quick entry:
1. Type or paste code in the text field
2. Click "Add to Table" or press Enter
3. Item is added and looked up automatically

### Working with the Table

#### View Scanned Items
The table shows:
- **#** - Row number
- **Code Type** - Detected format (QR Code, Barcode, etc.)
- **Scanned Code** - The actual scanned value
- **Status** - Lookup result (Found, Not Found, Checking, Error)
- **Asset Info** - Asset details if found (tag, brand, model)
- **Actions** - Edit and Remove buttons

#### Edit Scanned Item
1. Click "Edit" button on any row
2. Modal dialog opens with:
   - **Scanned Code** (read-only)
   - **Asset Tag** (editable)
   - **Notes** (editable)
3. Modify as needed
4. Click "Save Changes"

Use cases for editing:
- Correct misread codes
- Map barcode to internal asset tag
- Add context notes for processing

#### Remove Item
1. Click trash icon button
2. Confirm deletion
3. Item removed from table

#### Clear All Items
1. Click "Clear All" button (top right)
2. Confirm action
3. All items removed, table reset

### Saving Assets

**Current Behavior** (Placeholder):
- Click "Save All Assets" button
- System shows summary of scanned items:
  - Found: Items already in system
  - Not Found: Items to be created
- Confirms intent to proceed

**Next Steps** (To Implement):
You can extend this to:
1. Batch create assets for "Not Found" items
2. Update existing assets
3. Create inventory transactions
4. Generate reports

## Technical Details

### Supported Barcode Formats
- **EAN-13** - European Article Number (13 digits)
- **EAN-8** - European Article Number (8 digits)
- **UPC-A** - Universal Product Code (12 digits)
- **UPC-E** - UPC compressed (6 digits)
- **Code 128** - High-density alphanumeric
- **Code 39** - Alphanumeric and symbols
- **Code 93** - Alphanumeric, more compact than Code 39

### Code Type Detection
The system uses heuristics to detect code types:
- **QR Code**: Long strings (>20 chars), URLs, special characters
- **Barcode (UPC/EAN)**: 8-14 digit numbers
- **Barcode**: Alphanumeric codes with specific patterns
- **QR Code / Barcode**: Default when type is ambiguous

### Asset Lookup Flow
1. Item scanned/added
2. POST request to `/api/qrcode/lookup`
3. Status updated based on response:
   - **200 OK** → Status: Found (asset details shown)
   - **404 Not Found** → Status: Not Found
   - **Error** → Status: Error

### Data Structure
Each scanned item in the table contains:
```javascript
{
  id: number,              // Unique ID
  scannedCode: string,     // Raw scanned value
  codeType: string,        // Detected format
  assetTag: string,        // Mapped asset tag (editable)
  status: string,          // pending|found|not-found|error
  assetInfo: object|null,  // Asset details if found
  notes: string            // User notes (editable)
}
```

### Client-Side Storage
- Items stored in memory (`scannedItems` array)
- Data lost on page refresh
- Persist to localStorage or database as needed

## Use Cases

### 1. **Asset Inventory Audit**
- Scan multiple assets quickly
- Review all scans in table
- Identify missing or mismatched assets
- Edit tags for discrepancies
- Save batch updates

### 2. **Asset Check-In/Out**
- Scan assets being returned
- Verify against system records
- Add notes about condition
- Process batch transactions

### 3. **New Asset Registration**
- Scan product barcodes
- Items show as "Not Found"
- Edit to add internal asset tags
- Batch create new assets

### 4. **Quality Control**
- Scan items for verification
- Check against expected list
- Flag errors for review
- Document issues in notes

## Configuration

### Camera Settings
```javascript
{
  facingMode: "environment",  // Use back camera
  fps: 10,                    // Frames per second
  qrbox: { width: 250, height: 250 }  // Scan area size
}
```

### Scan Format Selection
Modify the format configuration in `start-scanner-btn` click handler:
```javascript
formatsToSupport.push(
  Html5QrcodeSupportedFormats.QR_CODE,
  Html5QrcodeSupportedFormats.EAN_13,
  // Add or remove formats as needed
);
```

## Troubleshooting

### Camera Not Working
- **Check HTTPS**: Camera requires HTTPS (or localhost)
- **Permissions**: Allow camera access in browser
- **Browser Support**: Use Chrome, Firefox, Safari, or Edge
- **Fallback**: Use manual entry field

### Barcode Not Scanning
- **Lighting**: Ensure good lighting conditions
- **Distance**: Hold camera 4-6 inches from barcode
- **Angle**: Keep camera perpendicular to barcode
- **Mode**: Try switching to "Barcode Only" mode
- **Quality**: Ensure barcode is not damaged

### Duplicate Detections
- Scanner may read same code multiple times
- System automatically rejects duplicates
- If needed, manually remove and re-scan

### Wrong Code Type Detected
- Detection is heuristic-based
- Edit the item to correct asset tag
- Manual entry bypasses auto-detection

## Future Enhancements

### High Priority
- [ ] **Batch Save API** - Implement backend endpoint to save/update multiple assets
- [ ] **Export Table** - Export scanned items as CSV/Excel
- [ ] **LocalStorage Persistence** - Save table data locally to survive page refresh
- [ ] **Import Expected List** - Compare scans against expected assets

### Medium Priority
- [ ] **Scan History** - Track all scan sessions with timestamps
- [ ] **Barcode Generation** - Generate barcodes for assets without them
- [ ] **Custom Mapping Rules** - Map barcode patterns to asset tag formats
- [ ] **Mobile App** - Native mobile app for better scanning performance

### Low Priority
- [ ] **Scan Statistics** - Show metrics (scans/minute, success rate)
- [ ] **Sound Feedback** - Audio confirmation on successful scan
- [ ] **Vibration Feedback** - Haptic feedback on mobile devices
- [ ] **Advanced Filtering** - Filter table by status, code type, etc.

## API Integration Points

### Current
- `POST /api/qrcode/lookup` - Lookup asset by scanned code

### Suggested for Full Implementation
```
POST /api/assets/batch-create
  Body: [{ assetTag, scannedCode, notes, ... }]
  
POST /api/assets/batch-update
  Body: [{ id, updates... }]
  
POST /api/scan-sessions
  Body: { scannedItems, timestamp, user, notes }
```

## Tips & Best Practices

### Scanning
✅ **Do:**
- Ensure good lighting
- Hold device steady
- Keep 4-6 inches from code
- Scan at perpendicular angle
- Use "QR & Barcode" mode for versatility

❌ **Don't:**
- Scan in direct sunlight (causes glare)
- Move too fast (causes blur)
- Tilt camera at sharp angles
- Scan damaged or dirty codes

### Table Management
✅ **Do:**
- Review all items before saving
- Add notes for unusual items
- Edit asset tags for consistency
- Remove accidental scans
- Save frequently if needed

❌ **Don't:**
- Save without reviewing
- Ignore "Not Found" items without investigation
- Forget to clear table after processing
- Mix different inventory sessions

### Workflow
1. **Prepare** - Clear table from previous session
2. **Configure** - Select appropriate scan mode
3. **Scan** - Scan all items continuously
4. **Review** - Check table for errors/missing items
5. **Edit** - Correct any issues
6. **Save** - Process batch (when implemented)
7. **Report** - Document results

---

**Last Updated**: October 25, 2025  
**Version**: 2.0 (Enhanced with Barcode Support)
