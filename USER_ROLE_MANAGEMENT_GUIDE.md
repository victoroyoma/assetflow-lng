# User Management & Role Management - Complete Documentation

## Overview
This document provides a comprehensive guide to the User Management and Role Management features implemented in the BuildOne Asset Management System.

## Table of Contents
1. [Features](#features)
2. [File Structure](#file-structure)
3. [User Management](#user-management)
4. [Role Management](#role-management)
5. [Security Features](#security-features)
6. [Usage Guide](#usage-guide)

---

## Features

### User Management Features
- ✅ **User CRUD Operations**: Create, Read, Update, Delete users
- ✅ **Advanced Search & Filter**: Search by name/email, filter by role and status
- ✅ **Role Assignment**: Assign multiple roles to users
- ✅ **Employee Linking**: Link users to employee records
- ✅ **Password Management**: Admin password reset functionality
- ✅ **Status Management**: Activate/deactivate user accounts
- ✅ **Last Administrator Protection**: Prevents deletion of the last admin
- ✅ **Real-time Filtering**: Client-side search and filter without page reload
- ✅ **User Statistics**: Visual display of user counts and filters

### Role Management Features
- ✅ **Role CRUD Operations**: Create, Read, Update, Delete roles
- ✅ **System Role Protection**: Administrator, User, and Technician roles are protected
- ✅ **User Count Display**: Shows how many users are assigned to each role
- ✅ **Deletion Prevention**: Cannot delete roles that are assigned to users
- ✅ **Role Status**: Active/inactive role management
- ✅ **Visual Statistics**: Dashboard cards showing role metrics
- ✅ **User Assignment Tracking**: View which users have specific roles

---

## File Structure

```
Controllers/
├── UserManagementController.cs      # User CRUD operations
└── RoleManagementController.cs      # Role CRUD operations

Views/
├── UserManagement/
│   ├── Index.cshtml                 # User list with search/filter
│   ├── Create.cshtml                # Create new user
│   ├── Edit.cshtml                  # Edit existing user
│   └── ResetPassword.cshtml         # Reset user password
└── RoleManagement/
    ├── Index.cshtml                 # Role list with statistics
    ├── Create.cshtml                # Create new role
    ├── Edit.cshtml                  # Edit existing role
    ├── Details.cshtml               # View role details
    └── Delete.cshtml                # Delete role confirmation

Data/
├── ApplicationUser.cs               # User entity model
└── ApplicationRole.cs               # Role entity model
```

---

## User Management

### 1. User List (Index)
**Route**: `/UserManagement/Index`

**Features**:
- Display all users in a responsive table
- Search by name or email (real-time)
- Filter by role (Administrator, Technician, User)
- Filter by status (Active, Inactive)
- Clear filters button
- Visual indicators for user status
- Role badges with color coding
- Employee linking status
- Last login tracking
- Action buttons: Edit, Reset Password, Delete

**Search & Filter Options**:
```javascript
- Search Input: Searches name and email fields
- Role Filter: All Roles | Administrator | Technician | User
- Status Filter: All Status | Active | Inactive
- Clear Filters: Resets all filters
```

**Visual Elements**:
- Avatar circles with user initials
- Color-coded role badges:
  - Administrator: Red (bg-danger)
  - Technician: Blue (bg-info)
  - User: Primary (bg-primary)
- Status badges (Active: Green, Inactive: Gray)
- Hover effects on table rows

### 2. Create User
**Route**: `/UserManagement/Create`

**Form Fields**:
- Full Name (required)
- Email (required, validates email format)
- Password (required, min 6 characters)
- Confirm Password (must match)
- Phone (optional)
- Employee Link (optional dropdown)
- Roles (multiple selection with checkboxes)
- Active Status (checkbox, default: true)

**Validation**:
- Email format validation
- Password strength (minimum 6 characters)
- Password confirmation match
- Duplicate email prevention

### 3. Edit User
**Route**: `/UserManagement/Edit/{id}`

**Capabilities**:
- Update user information
- Change role assignments
- Update employee linking
- Change active status
- Cannot change username (locked to email)
- Link to reset password

**Note**: Email changes also update the username automatically.

### 4. Reset Password
**Route**: `/UserManagement/ResetPassword/{id}`

**Features**:
- Admin can reset any user's password
- No old password required
- Password confirmation validation
- Minimum 6 characters requirement

**Security**:
- Only administrators can access
- Generates secure password reset token
- Uses ASP.NET Identity UserManager

### 5. Delete User
**Route**: `/UserManagement/Delete` (POST)

**Protection Rules**:
- Cannot delete the last administrator
- Confirmation dialog required
- Soft delete option (via IsActive flag)

---

## Role Management

### 1. Role List (Index)
**Route**: `/RoleManagement/Index`

**Dashboard Statistics**:
- Total Roles count
- Active Roles count
- System Roles count

**Table Columns**:
- Role Name (with system role badge)
- Description
- Status (Active/Inactive)
- Created Date
- Actions (View, Edit, Delete)

**Visual Features**:
- Icon-based role identification:
  - Administrator: Crown icon (gold)
  - Technician: Tools icon (blue)
  - User: User icon (primary)
  - Custom: Shield icon (gray)
- Color-coded statistics cards
- Hover effects on rows
- Auto-dismiss alerts (5 seconds)

### 2. Create Role
**Route**: `/RoleManagement/Create`

**Form Fields**:
- Role Name (required, unique)
- Description (optional, 200 char max)
- Active Status (checkbox, default: true)

**Validation**:
- Unique role name
- Required fields validation
- Description length limit

### 3. Edit Role
**Route**: `/RoleManagement/Edit/{id}`

**Restrictions**:
- Cannot edit system roles (Administrator, User, Technician)
- Shows warning for protected roles
- Updates affect all assigned users

**Editable Fields**:
- Role Name
- Description
- Active Status

### 4. View Role Details
**Route**: `/RoleManagement/Details/{id}`

**Information Displayed**:
- Role Name
- Description
- Active/Inactive Status
- Created Date
- Number of users assigned
- List of all users with this role (names and emails)

**Action Buttons**:
- Back to List
- Edit (if not system role)
- Delete (if not system role)

### 5. Delete Role
**Route**: `/RoleManagement/Delete/{id}`

**Protection Rules**:
1. Cannot delete system roles
2. Cannot delete roles with assigned users
3. Shows list of users if deletion blocked

**Confirmation Flow**:
- Shows role details
- Confirms no users are assigned
- Requires double confirmation
- JavaScript confirmation dialog
- Server-side validation

---

## Security Features

### Authentication & Authorization
```csharp
[Authorize(Roles = "Administrator")]
```
- All management controllers require Administrator role
- ASP.NET Identity integration
- Role-based access control (RBAC)

### Data Protection
1. **Anti-Forgery Tokens**: All forms include CSRF protection
2. **Password Security**: 
   - Minimum 6 characters
   - ASP.NET Identity password hashing
   - Secure password reset tokens
3. **Input Validation**: 
   - Server-side validation
   - Client-side validation
   - ModelState validation
4. **SQL Injection Prevention**: Entity Framework parameterized queries

### Business Logic Protection
1. **Last Admin Protection**: Cannot delete the last administrator
2. **System Role Protection**: Cannot edit/delete system roles
3. **User Assignment Check**: Cannot delete roles with users
4. **Concurrent Update Handling**: DbUpdateConcurrencyException handling

---

## Usage Guide

### For Administrators

#### Creating a New User
1. Navigate to **User Management**
2. Click **Create New User** button
3. Fill in required fields (Name, Email, Password)
4. Select appropriate roles
5. Optionally link to an employee record
6. Set active status
7. Click **Create User**

#### Managing User Roles
1. Go to **User Management** → Find user → Click **Edit**
2. Check/uncheck role checkboxes
3. Save changes
4. User's permissions update immediately

#### Creating a Custom Role
1. Navigate to **Role Management**
2. Click **Add New Role**
3. Enter role name and description
4. Set active status
5. Click **Create Role**
6. Assign role to users in User Management

#### Resetting User Password
1. Go to **User Management** → Find user
2. Click **Reset Password** (key icon)
3. Enter new password (min 6 characters)
4. Confirm password
5. Click **Reset Password**

#### Deleting a User
1. Find user in User Management
2. Click **Delete** (trash icon)
3. Confirm deletion in dialog
4. User is permanently removed

**Note**: Cannot delete if user is the last administrator.

#### Viewing Role Details
1. Navigate to **Role Management**
2. Click **View Details** (eye icon) on any role
3. See assigned users and role information
4. Edit or delete from this page

---

## Technical Implementation

### View Models

**UserViewModel**:
```csharp
public class UserViewModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; }
    public string? EmployeeName { get; set; }
}
```

**CreateUserViewModel**:
```csharp
public class CreateUserViewModel
{
    [Required] public string FullName { get; set; }
    [Required][EmailAddress] public string Email { get; set; }
    [Required][MinLength(6)] public string Password { get; set; }
    [Compare("Password")] public string ConfirmPassword { get; set; }
    public string? Phone { get; set; }
    public int? EmployeeId { get; set; }
    public List<string>? SelectedRoles { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Client-Side Filtering (JavaScript)

The User Management page includes real-time filtering:
```javascript
// Filters users by:
- Search term (name/email)
- Selected role
- Active/Inactive status

// Updates visible count dynamically
// Shows "No results" message when appropriate
```

### Styling Features

**Color Scheme**:
- Primary: Bootstrap primary blue
- Success: Green (active status)
- Warning: Orange (edit, reset password)
- Danger: Red (delete, administrator role)
- Info: Light blue (technician role, system roles)

**Responsive Design**:
- Mobile-friendly tables
- Collapsible columns on small screens
- Touch-friendly buttons
- Bootstrap 5 grid system

---

## API Endpoints

### User Management
- `GET /UserManagement/Index` - List all users
- `GET /UserManagement/Create` - Show create form
- `POST /UserManagement/Create` - Create user
- `GET /UserManagement/Edit/{id}` - Show edit form
- `POST /UserManagement/Edit/{id}` - Update user
- `GET /UserManagement/ResetPassword/{id}` - Show reset form
- `POST /UserManagement/ResetPassword/{id}` - Reset password
- `POST /UserManagement/Delete/{id}` - Delete user

### Role Management
- `GET /RoleManagement/Index` - List all roles
- `GET /RoleManagement/Create` - Show create form
- `POST /RoleManagement/Create` - Create role
- `GET /RoleManagement/Details/{id}` - View role details
- `GET /RoleManagement/Edit/{id}` - Show edit form
- `POST /RoleManagement/Edit/{id}` - Update role
- `GET /RoleManagement/Delete/{id}` - Show delete confirmation
- `POST /RoleManagement/Delete` - Delete role

---

## Database Schema

### ApplicationUser (extends IdentityUser)
```sql
- Id (string, PK)
- FullName (string, required, max 100)
- Email (string, required)
- UserName (string, required)
- Phone (string, optional, max 20)
- EmployeeId (int?, FK to Employee)
- IsActive (bool, default true)
- CreatedAt (datetime)
- LastLoginAt (datetime?)
```

### ApplicationRole (extends IdentityRole)
```sql
- Id (string, PK)
- Name (string, required, unique)
- Description (string, optional, max 200)
- IsActive (bool, default true)
- CreatedAt (datetime)
```

---

## Best Practices

### For Administrators
1. **Always maintain multiple administrators** for redundancy
2. **Use descriptive role names** that clearly indicate permissions
3. **Link users to employees** when applicable for better tracking
4. **Regularly review inactive users** and remove if no longer needed
5. **Create custom roles** for specific departmental needs
6. **Document role descriptions** clearly

### For Developers
1. **Never bypass authorization** attributes
2. **Always validate input** on both client and server
3. **Use view models** instead of exposing domain models
4. **Implement proper error handling** with user-friendly messages
5. **Log all administrative actions** for audit trails
6. **Test role permissions** thoroughly before deployment

---

## Troubleshooting

### Common Issues

**Issue**: Cannot delete a user
- **Solution**: Check if user is the last administrator. Promote another user to admin first.

**Issue**: Cannot delete a role
- **Solution**: Remove all users from the role first, then delete.

**Issue**: Search not working
- **Solution**: Ensure JavaScript is enabled. Check browser console for errors.

**Issue**: Password reset fails
- **Solution**: Verify password meets minimum requirements (6 characters).

**Issue**: Cannot edit system role
- **Solution**: System roles (Administrator, User, Technician) are protected and cannot be edited.

---

## Future Enhancements

Potential improvements for future versions:
- [ ] Bulk user import from CSV
- [ ] User activity logs and audit trail
- [ ] Password complexity requirements (configurable)
- [ ] Email verification for new users
- [ ] Two-factor authentication (2FA)
- [ ] Role permissions matrix (granular permissions)
- [ ] User groups/departments
- [ ] Self-service password reset
- [ ] Advanced reporting and analytics
- [ ] Export user/role lists to Excel
- [ ] User profile pictures/avatars
- [ ] Session management and timeout

---

## Support & Contact

For issues or questions regarding User and Role Management:
- Check the application logs for detailed error messages
- Review this documentation for usage guidelines
- Contact the system administrator for access issues
- Refer to ASP.NET Identity documentation for advanced customization

---

**Last Updated**: October 6, 2025  
**Version**: 1.0  
**Author**: BuildOne Development Team
