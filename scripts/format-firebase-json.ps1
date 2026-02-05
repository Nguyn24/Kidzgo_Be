# Script để format Firebase Service Account JSON cho appsettings.json
# Usage: .\format-firebase-json.ps1 -JsonFile "path/to/firebase-service-account.json"

param(
    [Parameter(Mandatory=$true)]
    [string]$JsonFile
)

if (-not (Test-Path $JsonFile)) {
    Write-Host "Error: File not found: $JsonFile" -ForegroundColor Red
    exit 1
}

Write-Host "Reading JSON file: $JsonFile" -ForegroundColor Green

# Read JSON file
$jsonContent = Get-Content $JsonFile -Raw

# Escape JSON for appsettings.json
# Replace " with \"
$escaped = $jsonContent -replace '"', '\"'
# Replace actual newlines with \n
$escaped = $escaped -replace "`r`n", '\n' -replace "`n", '\n' -replace "`r", '\n'

Write-Host "`n=== Formatted JSON String ===" -ForegroundColor Yellow
Write-Host $escaped -ForegroundColor Cyan

Write-Host "`n=== Copy the above string and paste into appsettings.json ===" -ForegroundColor Green
Write-Host 'Under "FCM": { "ServiceAccountJson": "..." }' -ForegroundColor Gray

# Also save to clipboard if available
try {
    $escaped | Set-Clipboard
    Write-Host "`n✅ JSON string has been copied to clipboard!" -ForegroundColor Green
} catch {
    Write-Host "`n⚠️  Could not copy to clipboard. Please copy manually." -ForegroundColor Yellow
}

