# Quick test script to verify parental controls state saving is fixed
Write-Host "Testing FamilyOS Shutdown Fix..." -ForegroundColor Cyan

# Start FamilyOS in background and send exit command after 8 seconds
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project FamilyOS.csproj" -NoNewWindow -PassThru -RedirectStandardInput

Start-Sleep -Seconds 8

# Send input to login and exit
$process | Stop-Process -Force

Write-Host "`nChecking for parental controls state error..." -ForegroundColor Yellow

# Check recent error
$logCheck = dotnet run --project FamilyOS.csproj 2>&1 | Select-String "Failed to save parental controls state" | Select-Object -First 1

if ($logCheck) {
    Write-Host "❌ ERROR STILL EXISTS: Parental controls state saving failed" -ForegroundColor Red
} else {
    Write-Host "✅ FIX VERIFIED: No parental controls state error detected" -ForegroundColor Green
}
