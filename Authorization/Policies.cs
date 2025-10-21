namespace buildone.Authorization;

/// <summary>
/// Defines authorization policy names used throughout the application
/// </summary>
public static class Policies
{
    // User Management Policies
    public const string CanManageUsers = "CanManageUsers";
    public const string CanViewUsers = "CanViewUsers";
    
    // Asset Management Policies
    public const string CanManageAssets = "CanManageAssets";
    public const string CanViewAssets = "CanViewAssets";
    public const string CanAssignAssets = "CanAssignAssets";
    
    // Department Management Policies
    public const string CanManageDepartments = "CanManageDepartments";
    public const string CanViewDepartments = "CanViewDepartments";
    
    // Employee Management Policies
    public const string CanManageEmployees = "CanManageEmployees";
    public const string CanViewEmployees = "CanViewEmployees";
    
    // Imaging Jobs Policies
    public const string CanManageImagingJobs = "CanManageImagingJobs";
    public const string CanViewImagingJobs = "CanViewImagingJobs";
    public const string CanPerformImaging = "CanPerformImaging";
    
    // Role Management Policies
    public const string CanManageRoles = "CanManageRoles";
    public const string CanViewRoles = "CanViewRoles";
    
    // System Settings Policies
    public const string CanAccessSystemSettings = "CanAccessSystemSettings";
    public const string CanModifySystemSettings = "CanModifySystemSettings";
    
    // Reports Policies
    public const string CanViewReports = "CanViewReports";
    public const string CanExportReports = "CanExportReports";
}

/// <summary>
/// Defines role names used throughout the application
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Technician = "Technician";
    public const string User = "User";
}
