# Quick UI Test Script
# Run this to verify the UI improvements are working correctly

Write-Host "=== Display Shaders PowerToy - UI Verification ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if DLL exists
Write-Host "[Test 1] Checking for DisplayShaderHook.dll..." -ForegroundColor Yellow
$binPath = "bin\Debug\net8.0-windows"
$dllPath = Join-Path $binPath "DisplayShaderHook.dll"

if (Test-Path $dllPath) {
    Write-Host "  ? DLL Found: $dllPath" -ForegroundColor Green
    Write-Host "  Expected UI: 'ClearType Optimization' + 'Shader DLL Ready'" -ForegroundColor Gray
    
    # Check version
    try {
        $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath)
        Write-Host "  DLL Version: $($version.FileMajorPart).$($version.FileMinorPart)" -ForegroundColor Gray
    } catch {
        Write-Host "  DLL Version: Unable to read" -ForegroundColor Gray
    }
} else {
    Write-Host "  ? DLL Not Found" -ForegroundColor Yellow
    Write-Host "  Expected UI: 'ClearType Optimization' only" -ForegroundColor Gray
}

Write-Host ""

# Test 2: Check config file location
Write-Host "[Test 2] Checking shader_config.ini location..." -ForegroundColor Yellow
$configPath = Join-Path $binPath "shader_config.ini"

if (Test-Path $configPath) {
    Write-Host "  ? Config exists: $configPath" -ForegroundColor Green
    Write-Host "  Contents:" -ForegroundColor Gray
    Get-Content $configPath | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
} else {
    Write-Host "  ? Config not created yet (will be created when settings are applied)" -ForegroundColor Gray
}

Write-Host ""

# Test 3: Check ClearType registry
Write-Host "[Test 3] Checking current ClearType registry settings..." -ForegroundColor Yellow
try {
    $regPath = "HKCU:\Control Panel\Desktop"
    $fontSmoothing = Get-ItemProperty -Path $regPath -Name "FontSmoothing" -ErrorAction SilentlyContinue
    $fontSmoothingType = Get-ItemProperty -Path $regPath -Name "FontSmoothingType" -ErrorAction SilentlyContinue
    $fontSmoothingGamma = Get-ItemProperty -Path $regPath -Name "FontSmoothingGamma" -ErrorAction SilentlyContinue
    
    Write-Host "  FontSmoothing: $($fontSmoothing.FontSmoothing)" -ForegroundColor Gray
    Write-Host "  FontSmoothingType: $($fontSmoothingType.FontSmoothingType)" -ForegroundColor Gray
    if ($fontSmoothingGamma) {
        Write-Host "  FontSmoothingGamma: $($fontSmoothingGamma.FontSmoothingGamma)" -ForegroundColor Gray
    } else {
        Write-Host "  FontSmoothingGamma: Not set" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ? Unable to read registry" -ForegroundColor Yellow
}

Write-Host ""

# Test 4: Build and run
Write-Host "[Test 4] Building and launching application..." -ForegroundColor Yellow
Write-Host "  Building..." -ForegroundColor Gray

dotnet build -c Debug --nologo --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ? Build successful" -ForegroundColor Green
    Write-Host ""
    Write-Host "=== MANUAL VERIFICATION STEPS ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "The application will now launch. Please verify:" -ForegroundColor White
    Write-Host ""
    Write-Host "  1. Look at the top of the window under the title" -ForegroundColor Yellow
    Write-Host "     ? Should show a blue badge with:" -ForegroundColor Gray
    Write-Host "       '? Active: ClearType Optimization'" -ForegroundColor Gray
    
    if (Test-Path $dllPath) {
        Write-Host "       '• Shader DLL Ready' (orange text)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "  2. Hover over 'Shader DLL Ready' (if shown)" -ForegroundColor Yellow
    Write-Host "     ? Tooltip should explain DLL is ready but not injecting" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. Check the settings section" -ForegroundColor Yellow
    Write-Host "     ? Header should say 'Text Rendering Settings'" -ForegroundColor Gray
    Write-Host "     ? Info box should say 'Current Mode: ClearType Registry Optimization'" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  4. Try changing settings and clicking Apply" -ForegroundColor Yellow
    Write-Host "     ? Status badge should remain 'ClearType Optimization'" -ForegroundColor Gray
    Write-Host "     ? Settings should save successfully" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  5. Toggle between Light and Dark mode" -ForegroundColor Yellow
    Write-Host "     ? Status badge should remain visible and readable" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press any key to launch the application..." -ForegroundColor Cyan
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # Launch the app
    $exePath = Join-Path $binPath "DisplayShadersPowerToy.exe"
    if (Test-Path $exePath) {
        Start-Process $exePath
        Write-Host ""
        Write-Host "? Application launched!" -ForegroundColor Green
        Write-Host "  Verify the UI changes above ^^^" -ForegroundColor Gray
    } else {
        Write-Host "? Executable not found: $exePath" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ? Build failed" -ForegroundColor Red
    Write-Host "  Check for compilation errors" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
