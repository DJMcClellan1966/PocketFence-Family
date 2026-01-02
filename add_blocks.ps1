# Add multiple blocks via CLI for testing
$urls = @(
    "https://malware-download.net/virus.exe",
    "https://adult-videos.xxx/watch",
    "https://phishing-bank.com/login",
    "https://hate-speech-forum.org/posts",
    "https://weapons-marketplace.onion/guns",
    "https://drug-dealer.com/buy",
    "https://violent-content.com/gore",
    "https://casino-online.net/slots",
    "https://hack-tools.io/password-cracker",
    "https://piracy-site.org/movies"
)

Write-Host "Adding blocks via CLI..." -ForegroundColor Cyan

foreach ($url in $urls) {
    Write-Host "Checking: $url" -ForegroundColor Yellow
    Write-Output "check $url`nexit" | dotnet run | Out-Null
    Start-Sleep -Milliseconds 500
}

Write-Host "`n✅ Added $($urls.Count) test blocks!" -ForegroundColor Green
Write-Host "View them at: http://localhost:5000/Blocked" -ForegroundColor Cyan
