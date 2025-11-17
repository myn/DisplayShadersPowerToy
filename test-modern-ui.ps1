# Test the new modern UI design

Write-Host "=== Testing Modern UI Design ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "Creating backup of current MainWindow..." -ForegroundColor Yellow
if (Test-Path "MainWindow.xaml") {
    Copy-Item "MainWindow.xaml" "MainWindow.xaml.backup" -Force
    Copy-Item "MainWindow.xaml.cs" "MainWindow.xaml.cs.backup" -Force
    Write-Host "? Backup created" -ForegroundColor Green
}

Write-Host ""
Write-Host "Switching to modern UI..." -ForegroundColor Yellow
Copy-Item "MainWindow_Modern.xaml" "MainWindow.xaml" -Force
Copy-Item "MainWindow_Modern.xaml.cs" "MainWindow.xaml.cs" -Force
Write-Host "? Modern UI activated" -ForegroundColor Green

Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "=== Starting Modern UI ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "New features:" -ForegroundColor Yellow
    Write-Host "  • Clean, single-column layout" -ForegroundColor Gray
    Write-Host "  • Large status card at top" -ForegroundColor Gray
    Write-Host "  • Quick enable/disable toggle" -ForegroundColor Gray
    Write-Host "  • Simplified display configuration" -ForegroundColor Gray
    Write-Host "  • Real-time active process list" -ForegroundColor Gray
    Write-Host "  • Modern color scheme" -ForegroundColor Gray
    Write-Host ""
    
    & "bin\Debug\net8.0-windows\DisplayShadersPowerToy.exe"
    
    Write-Host ""
    Write-Host "=== Testing Complete ===" -ForegroundColor Cyan
    Write-Host ""
    
    $response = Read-Host "Keep modern UI? (Y/N)"
    
    if ($response -ne "Y" -and $response -ne "y") {
        Write-Host ""
        Write-Host "Restoring original UI..." -ForegroundColor Yellow
        Copy-Item "MainWindow.xaml.backup" "MainWindow.xaml" -Force
        Copy-Item "MainWindow.xaml.cs.backup" "MainWindow.xaml.cs" -Force
        Write-Host "? Original UI restored" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "? Modern UI will be used" -ForegroundColor Green
        Write-Host "  Old UI backed up as MainWindow.xaml.backup" -ForegroundColor Gray
    }
} else {
    Write-Host "? Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Restoring original UI..." -ForegroundColor Yellow
    Copy-Item "MainWindow.xaml.backup" "MainWindow.xaml" -Force
    Copy-Item "MainWindow.xaml.cs.backup" "MainWindow.xaml.cs" -Force
    Write-Host "? Original UI restored" -ForegroundColor Green
}

Write-Host ""
Write-Host "Cleaning up backup files if keeping modern UI..." -ForegroundColor Gray
$cleanUp = Read-Host "Delete backup files? (Y/N)"
if ($cleanUp -eq "Y" -or $cleanUp -eq "y") {
    Remove-Item "MainWindow.xaml.backup" -ErrorAction SilentlyContinue
    Remove-Item "MainWindow.xaml.cs.backup" -ErrorAction SilentlyContinue
    Write-Host "? Backup files deleted" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
