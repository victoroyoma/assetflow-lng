# Jobs Module - Quick Reference Guide

## What Changed?

### Before
- "Imaging Jobs" module only for imaging tasks
- Maintenance card on dashboard linked to separate page
- No unified job management

### After
- "Jobs" module for both imaging AND maintenance
- Maintenance card creates a maintenance job
- All jobs managed in one place
- Search all assets from Jobs module

## How to Use

### Creating Jobs

#### Option 1: From Dashboard Quick Actions
```
Dashboard → Quick Actions
├── "Schedule Imaging" → Creates Imaging Job
└── "Maintenance Job" → Creates Maintenance Job
```

#### Option 2: From Jobs Module
```
Sidebar → Jobs → All Jobs
├── "New Imaging Job" button (green)
└── "New Maintenance Job" button (yellow)
```

### Viewing Jobs

```
Sidebar → Jobs → All Jobs
```
Shows:
- ✅ All Imaging jobs
- ✅ All Maintenance jobs
- ✅ Job status for each
- ✅ Asset information

### Finding Assets Under Maintenance

**Method 1: Filter by Job Type**
1. Go to Jobs → All Jobs
2. Look for jobs with type "Maintenance"
3. Check "In Progress" status

**Method 2: Search Asset**
1. Go to Jobs → All Jobs
2. Use search box to find specific asset
3. See all jobs (imaging + maintenance) for that asset

## UI Changes Summary

### Dashboard
| Component | Before | After |
|-----------|--------|-------|
| Main Icon | 🔥 | 🔥 |
| Maintenance Card | → Assets/Maintenance | → Create Maintenance Job |
| Quick Action #2 | "Schedule Imaging" | "Schedule Imaging" |
| Quick Action #3 | "Maintenance" | "Maintenance Job" |
| Jobs Section Title | "Imaging Jobs" | "Jobs" |

### Sidebar Navigation
| Component | Before | After |
|-----------|--------|-------|
| Menu Label | "Imaging Jobs" | "Jobs" |
| Icon | 💿 Compact Disc | ✅ Tasks |
| Sub-items | All Jobs, Queue, Create | (Same) |

### Jobs Index Page
| Component | Before | After |
|-----------|--------|-------|
| Page Title | "Imaging Jobs" | "Jobs Management" |
| Subtitle | "...imaging jobs" | "...imaging and maintenance jobs" |
| Create Button | "Create New Job" | 2 buttons: "New Imaging Job" + "New Maintenance Job" |

### Create Job Page
| Field | Imaging Job | Maintenance Job |
|-------|-------------|-----------------|
| **Page Title** | "Create Imaging Job" | "Create Maintenance Job" |
| **Header Color** | Blue (Primary) | Yellow (Warning) |
| **Icon** | 🖥️ Desktop | 🔧 Tools |
| Asset | ✅ Required | ✅ Required |
| Technician | ✅ Optional | ✅ Optional |
| **Imaging Type** | ✅ Required | ❌ Hidden |
| **Image Version** | ✅ Optional | ❌ Hidden |
| Priority | ✅ Required | ✅ Required |
| Scheduled Date | ✅ Optional | ✅ Optional |
| Due Date | ✅ Optional | ✅ Optional |
| Notes | ✅ Optional | ✅ Optional |

## Job Types Explained

### Imaging Job 🖥️
**Purpose**: Install or reinstall operating system on an asset

**Required Fields**:
- Asset
- Imaging Type (Fresh/Wipe and Load/Bare Metal)
- Priority

**Optional Fields**:
- Image Version (e.g., "Windows11-2024.1")
- Technician
- Scheduled Date/Time
- Due Date
- Notes

**Example Use Cases**:
- New laptop setup
- Computer reimaging
- OS upgrade
- Hard drive replacement requiring fresh install

### Maintenance Job 🔧
**Purpose**: Repair, maintain, or service an asset

**Required Fields**:
- Asset
- Priority

**Optional Fields**:
- Technician
- Scheduled Date/Time
- Due Date
- Notes (describe what maintenance is needed)

**Example Use Cases**:
- Hardware repair
- Component replacement
- Cleaning and servicing
- Diagnostic work
- Preventive maintenance

## Workflow Examples

### Example 1: Creating a Maintenance Job
```
1. User clicks "Maintenance" card on dashboard
   ↓
2. System navigates to /Imaging/Create?jobType=Maintenance
   ↓
3. Form loads with:
   - Yellow header "Create Maintenance Job"
   - Asset dropdown
   - Technician dropdown (optional)
   - Priority dropdown
   - Schedule fields
   - Notes field
   - No Imaging Type or Image Version fields
   ↓
4. User fills form and submits
   ↓
5. System creates job with JobType = Maintenance
   ↓
6. Success message: "Maintenance job for asset 'LAPTOP001' has been scheduled"
   ↓
7. Redirects to Job Queue
```

### Example 2: Viewing All Assets Under Maintenance
```
1. Go to Sidebar → Jobs → All Jobs
   ↓
2. Jobs index loads showing all jobs
   ↓
3. Filter/search for:
   - JobType = Maintenance (in UI)
   - Status = In Progress
   ↓
4. View list of assets currently under maintenance
   ↓
5. Click any job to see details
   ↓
6. Update status: Pending → In Progress → Completed
```

### Example 3: Searching for a Specific Asset
```
1. Go to Jobs → All Jobs
   ↓
2. Use search box (top of page)
   ↓
3. Enter asset tag (e.g., "LAPTOP001")
   ↓
4. System shows ALL jobs for that asset:
   - Past imaging jobs
   - Current maintenance job
   - Pending jobs
   ↓
5. See complete job history for the asset
```

## Database Structure

### Jobs Table
```
┌─────────────────┬──────────────┬──────────────┐
│ Field           │ Type         │ Notes        │
├─────────────────┼──────────────┼──────────────┤
│ Id              │ int          │ PK           │
│ AssetId         │ int          │ FK, Required │
│ TechnicianId    │ int?         │ FK, Optional │
│ JobType         │ int          │ NEW! 1=Imaging, 2=Maintenance │
│ ImagingType     │ int?         │ Changed to nullable │
│ ImageVersion    │ string?      │              │
│ Status          │ int          │              │
│ Priority        │ int          │              │
│ ScheduledAt     │ datetime?    │              │
│ DueDate         │ datetime?    │              │
│ StartedAt       │ datetime?    │              │
│ CompletedAt     │ datetime?    │              │
│ Notes           │ string?      │              │
│ CreatedAt       │ datetime     │              │
│ UpdatedAt       │ datetime?    │              │
└─────────────────┴──────────────┴──────────────┘

Indexes:
- Status
- ScheduledAt  
- ImagingType
- JobType (NEW!)
- (AssetId, Status)
- (JobType, Status) (NEW!)
```

## Key Benefits

### For Users
✅ **Unified Interface**: Manage all job types in one place
✅ **Easy Job Creation**: Direct links from dashboard
✅ **Complete Visibility**: See all asset jobs (imaging + maintenance)
✅ **Flexible Search**: Find assets and their jobs quickly
✅ **Status Tracking**: Track maintenance jobs just like imaging jobs

### For System
✅ **No Breaking Changes**: Existing code continues to work
✅ **Backward Compatible**: Old "Imaging Jobs" seamlessly become "Jobs"
✅ **Scalable**: Easy to add more job types in future
✅ **Performance**: Indexed by JobType for fast queries
✅ **Clean Data Model**: Single table, clear relationships

## URLs Quick Reference

| Action | URL |
|--------|-----|
| **View all jobs** | `/Imaging/Index` |
| **Job queue** | `/Imaging/Queue` |
| **Create imaging job** | `/Imaging/Create?jobType=Imaging` |
| **Create maintenance job** | `/Imaging/Create?jobType=Maintenance` |
| **Job details** | `/Imaging/Details?id={jobId}` |

## Tips & Tricks

💡 **Tip 1**: Use the dashboard for quick job creation
- Imaging jobs → Blue "Schedule Imaging" button
- Maintenance jobs → Yellow "Maintenance Job" button

💡 **Tip 2**: To see only maintenance jobs
- Go to Jobs → All Jobs
- Look for yellow tool icons 🔧
- Or add filter in future enhancement

💡 **Tip 3**: Track asset maintenance history
- Search for asset in Jobs module
- See all past and current maintenance jobs
- Compare with imaging job history

💡 **Tip 4**: Bulk maintenance scheduling
- Create multiple maintenance jobs
- Assign to different technicians
- Track all from Job Queue

## Common Questions

**Q: Where did "Imaging Jobs" go?**
A: It's now called "Jobs" and includes both imaging AND maintenance jobs.

**Q: Can I still create imaging jobs?**
A: Yes! Use "New Imaging Job" button or dashboard quick action.

**Q: How do I see which assets are in maintenance?**
A: Go to Jobs → All Jobs and look for maintenance job types with "In Progress" status.

**Q: Do maintenance jobs work the same as imaging jobs?**
A: Yes! Same workflow: Pending → In Progress → Completed. Just without imaging-specific fields.

**Q: Can I search for all jobs of a specific asset?**
A: Yes! Use the search box in Jobs → All Jobs to find any asset.

**Q: Will my old imaging jobs still show?**
A: Yes! All existing jobs are preserved and show as "Imaging" jobs.

## Support

For issues or questions:
1. Check this guide first
2. Review JOBS_MODULE_ENHANCEMENT.md for technical details
3. Test in dev environment before production use
