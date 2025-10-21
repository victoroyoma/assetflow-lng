# BuildOne Database Seeding Script
# This script seeds the database with test data for development purposes

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BuildOne Database Seeding Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://localhost:5001"  # Update this if your app runs on a different port

Write-Host "Step 1: Checking if the application is running..." -ForegroundColor Yellow

try {
    # Test if the application is running
    $null = Invoke-WebRequest -Uri $baseUrl -Method GET -UseBasicParsing -ErrorAction Stop
    Write-Host "✓ Application is running" -ForegroundColor Green
}
catch {
    Write-Host "✗ Application is not running. Please start the application first using 'dotnet run'" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Authentication Required" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Please log in to the application first by visiting:" -ForegroundColor Yellow
Write-Host "$baseUrl/Account/Login" -ForegroundColor White
Write-Host ""
Write-Host "Default credentials:" -ForegroundColor Yellow
Write-Host "  Email: admin@buildone.com" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "After logging in, visit the Data Seeding page:" -ForegroundColor Yellow
Write-Host "$baseUrl/Admin/SeedData" -ForegroundColor White
Write-Host ""
Write-Host "Or use the API endpoints:" -ForegroundColor Yellow
Write-Host "  Basic Seeding: POST $baseUrl/api/DataSeeding/seed-basic" -ForegroundColor White
Write-Host "  Bulk Seeding:  POST $baseUrl/api/DataSeeding/seed-bulk" -ForegroundColor White
Write-Host "  Statistics:    GET  $baseUrl/api/DataSeeding/statistics" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "What will be created?" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Basic Seeding:" -ForegroundColor Yellow
Write-Host "  • 3 Roles (Administrator, Technician, User)" -ForegroundColor White
Write-Host "  • 1 Admin User (admin@buildone.com)" -ForegroundColor White
Write-Host ""
Write-Host "Bulk Seeding:" -ForegroundColor Yellow
Write-Host "  • 8 Departments" -ForegroundColor White
Write-Host "  • 10 Employees" -ForegroundColor White
Write-Host "  • 400 Assets (200 Laptops, 140 Desktops, 60 Tablets)" -ForegroundColor White
Write-Host "  • 400 Imaging Jobs" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Read-Host "Press Enter to open the browser to the Data Seeding page..."

# Open the browser
Start-Process "$baseUrl/Admin/SeedData"

Write-Host ""
Write-Host "Browser opened. Please complete the seeding process in the web interface." -ForegroundColor Green
Write-Host ""
