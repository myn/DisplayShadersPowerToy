# Simple Production Package Creator
param([string]$Version = "2.0.0")

Write-Host "Creating production package v$Version..." -ForegroundColor Cyan

# Create dist folder
$distFolder = "dist"
$packageName = "DisplayShadersPowerToy-v$Version-Full"
$packageFolder = Join-Path $distFolder $packageName

if (Test-Path $distFolder) {
    Remove-Item $distFolder -Recurse -Force
}

New-Item -ItemType Directory -Path $packageFolder -Force | Out-Null

# Copy application files
Write-Host "Copying application files..." -ForegroundColor Yellow
Copy-Item "bin\Release\net8.0-windows\*" $packageFolder -Recurse -Force

# Copy documentation
$docs = @("README.md", "LICENSE", "GETTING_STARTED.md", "FAQ.md", "CHANGELOG.md", "START_HERE.md")
foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Copy-Item $doc $packageFolder -Force
        Write-Host "  Copied $doc" -ForegroundColor Gray
    }
}

# Create README
$packageReadme = @"
# Display Shaders PowerToy v$Version - FULL SHADER MODE

## Installation

1. Extract all files to a folder
2. Run DisplayShadersPowerToy.exe as Administrator
3. Select your display's subpixel layout
4. Click "Apply"

## Requirements

- Windows 10/11 (x64)
- .NET 8.0 Runtime (included)
- Administrator privileges (for DLL injection)

## Features

? Real DirectWrite/D3D shader hooks
? WOLED WRGB fix (Blue in middle)
? QD-OLED triangular fix (vertical fringing)
? ClearType fallback mode
? System tray integration

## Files Included

- DisplayShadersPowerToy.exe - Main application
- DisplayShaderHook.dll - Native shader hook DLL
- All dependencies

## Documentation

See README.md, GETTING_STARTED.md, and FAQ.md

Built: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

Set-Content -Path (Join-Path $packageFolder "INSTALLATION.txt") -Value $packageReadme

# Create ZIP
Write-Host "Creating ZIP archive..." -ForegroundColor Yellow
$zipPath = Join-Path $distFolder "$packageName.zip"
Compress-Archive -Path "$packageFolder\*" -DestinationPath $zipPath -Force

# Show results
Write-Host ""
Write-Host "? Package created successfully!" -ForegroundColor Green
Write-Host "  Location: $zipPath" -ForegroundColor Cyan
Write-Host "  Size: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Gray
Write-Host ""
Write-Host "Package contents:" -ForegroundColor White
Get-ChildItem $packageFolder | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}
