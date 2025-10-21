# User & Role Management - Quick Reference

## 🎯 Quick Access

### Navigation
```
Sidebar → Administration → User Management
Sidebar → Administration → Role Management
```

## 👤 User Management

### Creating a User
1. Click **Create New User**
2. Enter: Name, Email, Password
3. Select Roles (check boxes)
4. Link to Employee (optional)
5. Click **Create User**

### Editing a User
1. Find user in list
2. Click pencil icon (Edit)
3. Update information
4. Check/uncheck roles
5. Click **Update User**

### Resetting Password
1. Find user
2. Click key icon
3. Enter new password
4. Confirm password
5. Click **Reset Password**

### Searching Users
- **Search Box**: Type name or email
- **Role Filter**: Filter by role type
- **Status Filter**: Active/Inactive
- **Clear Button**: Reset all filters

## 🛡️ Role Management

### Dashboard Statistics
- **Total Roles**: All roles in system
- **Active Roles**: Currently usable roles
- **System Roles**: Protected roles (3)

### Creating a Role
1. Click **Add New Role**
2. Enter Role Name
3. Enter Description (optional)
4. Set Active status
5. Click **Create Role**

### Viewing Role Details
1. Click eye icon on any role
2. See assigned users
3. View role information
4. Edit or Delete (if allowed)

### Editing a Role
1. Click pencil icon (Edit)
2. Update Name/Description
3. Change Active status
4. Click **Update Role**

### Deleting a Role
1. Click trash icon (Delete)
2. Confirm no users assigned
3. Double-confirm deletion
4. Role is removed

## 🔒 System Roles (Protected)

These roles **CANNOT** be edited or deleted:
- **Administrator**: Full system access
- **Technician**: Technical operations
- **User**: Standard user access

## ⚠️ Important Rules

### User Management
- ❌ Cannot delete the last administrator
- ✅ Can assign multiple roles to a user
- ✅ Can link user to employee record
- ✅ Inactive users cannot login

### Role Management
- ❌ Cannot edit/delete system roles
- ❌ Cannot delete roles with assigned users
- ✅ Can create unlimited custom roles
- ✅ Inactive roles cannot be assigned

## 🎨 Visual Indicators

### Role Badges
- 🔴 **Red**: Administrator
- 🔵 **Blue**: Technician
- 🟢 **Primary**: User
- ⚫ **Gray**: Custom roles

### Status Badges
- ✅ **Green**: Active
- ⏸️ **Gray**: Inactive

### Icons
- 👁️ View Details
- ✏️ Edit
- 🔑 Reset Password
- 🗑️ Delete
- 🔒 Protected/Locked

## 🔍 Search & Filter

### User List Filters
```
Search: [Type name or email]
Role:   [All | Admin | Tech | User]
Status: [All | Active | Inactive]
[Clear] button to reset
```

### Results Display
- Shows "X of Y users"
- Real-time filtering
- No page reload needed
- "No results" message when empty

## 🚀 Keyboard Shortcuts

- **Enter** in search box: Activates filter
- **Escape**: Can close modals
- **Tab**: Navigate through form fields

## 📊 Page Sections

### User Management Index
```
┌─────────────────────────────────┐
│ Header + Create Button          │
├─────────────────────────────────┤
│ Success/Error Messages          │
├─────────────────────────────────┤
│ Search & Filter Bar             │
├─────────────────────────────────┤
│ User Table with Actions         │
└─────────────────────────────────┘
```

### Role Management Index
```
┌─────────────────────────────────┐
│ Header + Create Button          │
├─────────────────────────────────┤
│ Statistics Cards (3)            │
├─────────────────────────────────┤
│ Success/Error Messages          │
├─────────────────────────────────┤
│ Role Table with Actions         │
└─────────────────────────────────┘
```

## 🆘 Common Actions

### Activate/Deactivate User
Edit → Uncheck "Is Active" → Save

### Assign Multiple Roles
Edit User → Check multiple role boxes → Save

### View Who Has a Role
Role Details → See "Users in Role" section

### Remove User from Role
Edit User → Uncheck role → Save

### Change User Email
Edit User → Update Email → Save
*(Username automatically updates)*

## 💡 Tips & Tricks

1. **Filter First**: Use filters to find users quickly
2. **Employee Links**: Link users to employees for better tracking
3. **Role Descriptions**: Write clear descriptions for custom roles
4. **Regular Cleanup**: Review and remove inactive users periodically
5. **Multiple Admins**: Always have 2+ administrators
6. **Test Roles**: Create test users to verify role permissions

## 📱 Mobile Access

All pages are responsive and work on:
- 📱 Phones
- 📱 Tablets
- 💻 Desktops

Touch-friendly buttons and forms included.

## ⚡ Performance Notes

- Search/filter is **instant** (client-side)
- No page reloads during filtering
- Auto-dismissing alerts (5 seconds)
- Hover effects for better UX

## 🔐 Security Features

- ✅ Administrator-only access
- ✅ CSRF protection on all forms
- ✅ Password validation
- ✅ Secure password reset
- ✅ Anti-forgery tokens
- ✅ Protected system roles
- ✅ Last admin protection

## 📞 Need Help?

- Check full documentation: `USER_ROLE_MANAGEMENT_GUIDE.md`
- Contact system administrator
- Review application logs
- Check browser console for errors

---

**Quick Reference v1.0** | Updated: October 6, 2025
