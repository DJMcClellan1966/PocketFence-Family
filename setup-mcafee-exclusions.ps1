# Add McAfee Exclusions for PocketFence AI
# Run as Administrator

Write-Host "=== McAfee Exclusion Setup for PocketFence AI ===" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Paths to exclude
$projectPath = $PSScriptRoot
$dotnetPath = "C:\Program Files\dotnet"
$userDotnetPath = "$env:USERPROFILE\.dotnet"

Write-Host "Adding exclusions to Windows Defender..." -ForegroundColor Yellow
Write-Host ""

# Add Windows Defender exclusions (works even with McAfee)
try {
    Add-MpPreference -ExclusionPath $projectPath -ErrorAction Stop
    Write-Host "[OK] Added project folder: $projectPath" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not add project folder to Defender" -ForegroundColor Yellow
}

try {
    Add-MpPreference -ExclusionPath $dotnetPath -ErrorAction Stop
    Write-Host "[OK] Added .NET runtime: $dotnetPath" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not add .NET runtime to Defender" -ForegroundColor Yellow
}

try {
    Add-MpPreference -ExclusionPath $userDotnetPath -ErrorAction Stop
    Write-Host "[OK] Added user .NET folder: $userDotnetPath" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not add user .NET folder to Defender" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== McAfee Manual Configuration Required ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "McAfee does not support PowerShell automation for exclusions." -ForegroundColor Yellow
Write-Host "Please follow these manual steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Open McAfee Security Center" -ForegroundColor White
Write-Host "2. Click the gear icon (Settings)" -ForegroundColor White
Write-Host "3. Go to: Real-Time Scanning > Excluded Files" -ForegroundColor White
Write-Host "4. Click 'Add File' and add these executables:" -ForegroundColor White
Write-Host ""
Write-Host "   [FILE] $dotnetPath\dotnet.exe" -ForegroundColor Cyan
Write-Host "   [FILE] $projectPath\bin\Debug\net8.0\PocketFence-AI.exe" -ForegroundColor Cyan
Write-Host "   [FILE] $projectPath\bin\Release\net8.0\PocketFence-AI.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "5. If you need to build/run with 'dotnet run', also add:" -ForegroundColor White
Write-Host "   [FILE] $projectPath\bin\Debug\net8.0\*.dll" -ForegroundColor Cyan
Write-Host "   [FILE] $projectPath\bin\Release\net8.0\*.dll" -ForegroundColor Cyan
Write-Host ""
Write-Host "6. Click 'Apply' and close McAfee" -ForegroundColor White
Write-Host ""
Write-Host "NOTE: McAfee only supports file exclusions, not folder exclusions." -ForegroundColor Yellow
Write-Host "Use the helper batch files to run the compiled .exe directly." -ForegroundColor Yellow
Write-Host ""

# Create helper batch files
Write-Host "Creating helper scripts..." -ForegroundColor Yellow

$cliContent = "@echo off`r`necho Starting PocketFence CLI...`r`n`"$projectPath\bin\Release\net8.0\PocketFence-AI.exe`"`r`npause"
$dashboardContent = "@echo off`r`necho Starting PocketFence Dashboard...`r`n`"$projectPath\bin\Release\net8.0\PocketFence-AI.exe`" dashboard`r`npause"

try {
    $cliContent | Out-File -FilePath "$projectPath\run-cli.bat" -Encoding ASCII -Force
    Write-Host "[OK] Created: run-cli.bat" -ForegroundColor Green
    
    $dashboardContent | Out-File -FilePath "$projectPath\run-dashboard.bat" -Encoding ASCII -Force
    Write-Host "[OK] Created: run-dashboard.bat" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not create helper scripts" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Ready! ===" -ForegroundColor Green
Write-Host ""
Write-Host "After adding McAfee exclusions, run PocketFence using:" -ForegroundColor Cyan
Write-Host "  - Double-click: run-cli.bat (for CLI)" -ForegroundColor White
Write-Host "  - Double-click: run-dashboard.bat (for Dashboard)" -ForegroundColor White
Write-Host "  - Or use: .\bin\Release\net8.0\PocketFence-AI.exe" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit"
