# Attachments & QR Code Quick Reference

## File Attachments

### Upload Files
**Imaging Jobs:**
1. Go to Imaging > Job Queue > Select a job
2. Scroll to "Attachments" card in right sidebar
3. Click "Select file" → choose file
4. Add description (optional)
5. Click "Upload Attachment"

**Assets:**
1. Go to Assets > Select an asset
2. Scroll to "Attachments" card in right sidebar
3. Same process as above

### Download/Delete Files
- **Download**: Click "Download" button next to any attachment
- **Delete**: Click "Delete" button, confirm deletion

### File Limits
- **Max size**: 10 MB per file
- **Supported images**: .jpg, .jpeg, .png, .gif, .bmp, .webp
- **Supported documents**: .pdf, .doc, .docx, .xls, .xlsx, .txt, .csv

---

## QR Codes

### View & Download QR Code
1. Go to Assets > Select an asset
2. QR Code card shows the code inline
3. Click "Download URL QR" to save PNG

### Scan QR Code
**Via Camera:**
1. Go to Assets > Click "Scan QR Code" button
2. Click "Start Camera Scanner"
3. Allow camera permissions
4. Point at QR code
5. Auto-displays asset details

**Manual Entry:**
1. On Scan page, type asset tag
2. Click "Lookup" or press Enter
3. View asset details

### Create Asset from Scan
1. Scan unknown asset tag (or type it)
2. Click "Create New Asset" in "Asset Not Found" card
3. Asset tag auto-fills in create form
4. Complete and save

---

## API Endpoints Quick Reference

### Attachments
```
POST   /api/attachments/job/{jobId}           - Upload to job
POST   /api/attachments/asset/{assetId}       - Upload to asset
GET    /api/attachments/job/{jobId}           - List job attachments
GET    /api/attachments/asset/{assetId}       - List asset attachments
GET    /api/attachments/download/job/{id}     - Download job attachment
GET    /api/attachments/download/asset/{id}   - Download asset attachment
DELETE /api/attachments/job/{id}              - Delete job attachment
DELETE /api/attachments/asset/{id}            - Delete asset attachment
```

### QR Codes
```
GET  /api/qrcode/asset/{id}/generate          - Download QR PNG (tag only)
GET  /api/qrcode/asset/{id}/generate-url      - Download QR PNG (with URL)
GET  /api/qrcode/asset/{id}/base64            - Get QR as base64 JSON
POST /api/qrcode/lookup                       - Lookup asset by scan data
```

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Camera not working | 1. Use HTTPS or localhost<br>2. Allow camera permissions<br>3. Use manual entry fallback |
| File too large | Max 10 MB - reduce file size or split into multiple files |
| Wrong file type | Only images and documents allowed - check supported extensions |
| Can't delete attachment | Requires admin permission - check user role |
| QR not displaying | Ensure logged in and asset exists - refresh page |

---

## Tips & Best Practices

### Attachments
- ✅ Add descriptions to attachments for easy identification
- ✅ Upload invoices, purchase orders, warranty docs to assets
- ✅ Upload screenshots, logs to imaging jobs
- ✅ Use images for before/after documentation
- ❌ Don't upload sensitive data without proper access controls

### QR Codes
- ✅ Print QR codes and attach to physical assets
- ✅ Use URL QR codes for sharing asset details externally
- ✅ Scan QR codes for quick inventory checks
- ✅ Use manual entry for quick lookups without camera
- ❌ Don't print very small QR codes (hard to scan)

---

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Manual lookup | Type asset tag + **Enter** |
| Close modal | **Esc** |
| Navigate back | **Alt + ←** (browser) |

---

**Need Help?** See full documentation in `ATTACHMENTS_QR_GUIDE.md`
