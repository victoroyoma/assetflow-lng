<#
.SYNOPSIS
    Seeds the BuildOne database from an Excel (.xlsx) file.

.DESCRIPTION
    This script uploads an Excel file to the BuildOne API endpoint for data seeding.
    The Excel file should contain sheets named: Departments, Employees, Assets, Inventory
    (any combination is acceptable; missing sheets will be skipped).

.PARAMETER ExcelFilePath
    Path to the Excel (.xlsx) file to upload.

.PARAMETER ApiUrl
    Base URL of the BuildOne API. Default: https://localhost:5001

.PARAMETER Username
    Username for authentication (optional if already logged in with cookies).

.PARAMETER Password
    Password for authentication as SecureString (optional if already logged in with cookies).

.EXAMPLE
    .\seed-from-excel.ps1 -ExcelFilePath "C:\data\seed-data.xlsx"
    
.EXAMPLE
    $pass = ConvertTo-SecureString "Admin@123" -AsPlainText -Force
    .\seed-from-excel.ps1 -ExcelFilePath ".\seed-data.xlsx" -Username "admin@buildone.com" -Password $pass

.EXAMPLE
    .\seed-from-excel.ps1 -ExcelFilePath ".\seed-data.xlsx" -ApiUrl "https://myserver.com"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ExcelFilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "https://localhost:5001",
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [SecureString]$Password
)

# Ensure the file exists
if (-not (Test-Path $ExcelFilePath)) {
    Write-Error "Excel file not found: $ExcelFilePath"
    exit 1
}

# Get absolute path
$ExcelFilePath = Resolve-Path $ExcelFilePath

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  BuildOne Excel Data Seeding Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Excel File: $ExcelFilePath" -ForegroundColor Yellow
Write-Host "API URL: $ApiUrl" -ForegroundColor Yellow
Write-Host ""

# Create a session container for cookies
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Authenticate if credentials are provided
if ($Username -and $Password) {
    Write-Host "Authenticating as $Username..." -ForegroundColor Green
    
    $loginUrl = "$ApiUrl/Account/Login"
    
    # Convert SecureString to plain text for form post
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    $loginBody = @{
        email = $Username
        password = $plainPassword
        returnUrl = "/"
    }
    
    try {
        Invoke-WebRequest -Uri $loginUrl -Method POST -Body $loginBody -SessionVariable session -UseBasicParsing -ErrorAction Stop | Out-Null
        Write-Host "✓ Authentication successful" -ForegroundColor Green
    }
    catch {
        Write-Error "Authentication failed: $_"
        exit 1
    }
}

# Upload the Excel file
Write-Host ""
Write-Host "Uploading Excel file and seeding database..." -ForegroundColor Green

$uploadUrl = "$ApiUrl/api/dataseeding/seed-from-excel"

try {
    # Read file as bytes
    $fileBytes = [System.IO.File]::ReadAllBytes($ExcelFilePath)
    $fileName = [System.IO.Path]::GetFileName($ExcelFilePath)
    
    # Create multipart form data
    $boundary = [System.Guid]::NewGuid().ToString()
    $LF = "`r`n"
    
    $bodyLines = @(
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
        "Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
        "--$boundary--",
        ""
    )
    
    $body = $bodyLines -join $LF
    
    # Make the request
    $headers = @{
        "Content-Type" = "multipart/form-data; boundary=$boundary"
    }
    
    $response = Invoke-RestMethod -Uri $uploadUrl -Method POST -Body $body -Headers $headers -WebSession $session -ErrorAction Stop
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✓ Seeding Completed Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Results:" -ForegroundColor Cyan
    Write-Host "  Departments: $($response.departments) created" -ForegroundColor White
    Write-Host "  Employees: $($response.employees) created" -ForegroundColor White
    Write-Host "  Assets: $($response.assets) created" -ForegroundColor White
    Write-Host "  Inventory: $($response.inventory) created" -ForegroundColor White
    Write-Host ""
    Write-Host "Timestamp: $($response.timestamp)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ✗ Seeding Failed" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "HTTP Status: $statusCode" -ForegroundColor Red
        
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            $errorJson = $errorBody | ConvertFrom-Json
            Write-Host "Error: $($errorJson.error)" -ForegroundColor Red
            if ($errorJson.details) {
                Write-Host "Details: $($errorJson.details)" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
    exit 1
}
