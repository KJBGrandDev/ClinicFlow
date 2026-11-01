# ==============================================================================
# CLINICFLOW UNIFIED — AUTOMATED DATABASE SETUP & MIGRATION BOOTSTRAPPER
# Requirement: REQ-028 (Automated Database Setup Package / Installer)
# ==============================================================================

param(
    [string]$ServerInstance = "localhost",
    [string]$DatabaseName = "ClinicFlowUnifiedDb"
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "CLINICFLOW UNIFIED DATABASE SETUP & BOOTSTRAPPER" -ForegroundColor Cyan
Write-Host "Target Server   : $ServerInstance" -ForegroundColor Cyan
Write-Host "Target Database : $DatabaseName" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

# 1. Test SQL Server Connectivity & Create Database if Missing
Write-Host "`n[Step 1/3] Verifying SQL Server instance and database catalog..." -ForegroundColor Yellow
$masterSql = @"
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = '$DatabaseName')
BEGIN
    PRINT 'Database [$DatabaseName] not found. Creating database...';
    CREATE DATABASE [$DatabaseName];
END
ELSE
BEGIN
    PRINT 'Database [$DatabaseName] already exists.';
END
"@

$createOutput = sqlcmd -S $ServerInstance -E -C -Q $masterSql 2>&1
Write-Host $createOutput -ForegroundColor Gray

# 2. Locate and Apply Migration Scripts Sequentially
Write-Host "`n[Step 2/3] Applying SQL migration scripts sequentially..." -ForegroundColor Yellow
$scriptDir = Join-Path $PSScriptRoot "Scripts"
if (!(Test-Path $scriptDir)) {
    # Fallback to current directory or relative
    $scriptDir = Join-Path (Get-Location) "Database\Scripts"
}

if (!(Test-Path $scriptDir)) {
    Write-Host "[ERROR] Could not locate migration scripts directory at: $scriptDir" -ForegroundColor Red
    exit 1
}

$scripts = Get-ChildItem -Path $scriptDir -Filter "*.sql" | Sort-Object Name
Write-Host "Found $($scripts.Count) migration scripts in $scriptDir" -ForegroundColor Cyan

foreach ($script in $scripts) {
    Write-Host "  -> Executing: $($script.Name)..." -NoNewline
    $result = sqlcmd -S $ServerInstance -d $DatabaseName -E -C -i $script.FullName 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host " [FAILED]" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    Write-Host " [OK]" -ForegroundColor Green
}

# 3. Verify Final Schema
Write-Host "`n[Step 3/3] Verifying database schema integrity..." -ForegroundColor Yellow
$verifySql = "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
$tableCountOut = sqlcmd -S $ServerInstance -d $DatabaseName -E -C -h -1 -W -Q $verifySql 2>&1

Write-Host "Total Base Tables Deployed: $($tableCountOut.Trim())" -ForegroundColor Green
Write-Host "`n=================================================================" -ForegroundColor Green
Write-Host "DATABASE SETUP COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "ClinicFlow-Unified is now configured and ready to run." -ForegroundColor Green
Write-Host "=================================================================" -ForegroundColor Green
