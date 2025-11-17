# Complete Shader Injection Test Script
# This script tests the full shader injection pipeline

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "   Display Shaders - Complete System Test   " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

# Step 1: Build the C# project
Write-Host "[Step 1] Building C# Application..." -ForegroundColor Yellow
dotnet build -c Debug --nologo --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ? C# build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "  ? C# build successful" -ForegroundColor Green
Write-Host ""

# Step 2: Check for native DLL
Write-Host "[Step 2] Checking for DisplayShaderHook.dll..." -ForegroundColor Yellow
$binPath = "bin\Debug\net8.0-windows"
$dllPath = Join-Path $binPath "DisplayShaderHook.dll"

if (Test-Path $dllPath) {
    Write-Host "  ? DLL found: $dllPath" -ForegroundColor Green
    
    try {
        $fileInfo = Get-Item $dllPath
        Write-Host "    Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
        Write-Host "    Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
        
        $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath)
        if ($version.FileMajorPart -gt 0) {
            Write-Host "    Version: $($version.FileMajorPart).$($version.FileMinorPart).$($version.FileBuildPart)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "    Could not read DLL info" -ForegroundColor Gray
    }
} else {
    Write-Host "  ? DLL not found - shader mode will not be available" -ForegroundColor Yellow
    Write-Host "    Build the Native project to enable shader injection" -ForegroundColor Gray
    Write-Host "    For now, only ClearType mode will work" -ForegroundColor Gray
}
Write-Host ""

# Step 3: Check for test processes
Write-Host "[Step 3] Checking for injectable processes..." -ForegroundColor Yellow
$targetProcesses = @("notepad", "code", "chrome", "firefox", "msedge")
$runningTargets = @()

foreach ($procName in $targetProcesses) {
    $procs = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($procs) {
        $runningTargets += $procName
        Write-Host "  ? Found: $procName ($($procs.Count) instance(s))" -ForegroundColor Green
    }
}

if ($runningTargets.Count -eq 0) {
    Write-Host "  ? No target processes running" -ForegroundColor Yellow
    Write-Host "    Consider opening Notepad or Chrome for testing" -ForegroundColor Gray
} else {
    Write-Host "  ? Found $($runningTargets.Count) injectable process type(s)" -ForegroundColor Green
}
Write-Host ""

# Step 4: Check ClearType registry
Write-Host "[Step 4] Checking ClearType registry..." -ForegroundColor Yellow
try {
    $regPath = "HKCU:\Control Panel\Desktop"
    $fontSmoothing = Get-ItemProperty -Path $regPath -Name "FontSmoothing" -ErrorAction SilentlyContinue
    $fontSmoothingType = Get-ItemProperty -Path $regPath -Name "FontSmoothingType" -ErrorAction SilentlyContinue
    
    Write-Host "  Current ClearType settings:" -ForegroundColor Gray
    Write-Host "    FontSmoothing: $($fontSmoothing.FontSmoothing)" -ForegroundColor Gray
    Write-Host "    FontSmoothingType: $($fontSmoothingType.FontSmoothingType)" -ForegroundColor Gray
} catch {
    Write-Host "  ? Could not read registry" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Launch application
Write-Host "[Step 5] Launching application..." -ForegroundColor Yellow
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "   MANUAL TESTING INSTRUCTIONS              " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $dllPath) {
    Write-Host "? SHADER MODE AVAILABLE" -ForegroundColor Green
    Write-Host ""
    Write-Host "When the app launches, verify:" -ForegroundColor White
    Write-Host ""
    Write-Host "1. Status Badge (Top of window)" -ForegroundColor Yellow
    Write-Host "   Before Apply:" -ForegroundColor Gray
    Write-Host "   • Should show: '? Active: ClearType Optimization'" -ForegroundColor Gray
    Write-Host "   • Should show: '• Shader DLL Ready' (orange)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   After clicking Apply:" -ForegroundColor Gray
    Write-Host "   • Should change to: '? Active: Display Shaders'" -ForegroundColor Gray
    Write-Host "   • Should show: '• X processes hooked' (green)" -ForegroundColor Gray
    Write-Host "   • Hover to see list of injected processes" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "2. Settings Application" -ForegroundColor Yellow
    Write-Host "   • Select a subpixel layout (e.g., WRGB for OLED)" -ForegroundColor Gray
    Write-Host "   • Set intensity (e.g., 80%)" -ForegroundColor Gray
    Write-Host "   • Click 'Apply'" -ForegroundColor Gray
    Write-Host "   • Should see success message with process count" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "3. Verify Injection" -ForegroundColor Yellow
    Write-Host "   • Open Notepad (if not already open)" -ForegroundColor Gray
    Write-Host "   • Type some text" -ForegroundColor Gray
    Write-Host "   • Click Apply again" -ForegroundColor Gray
    Write-Host "   • Process count should increase" -ForegroundColor Gray
    Write-Host "   • Text rendering should show shader effects" -ForegroundColor Gray
    Write-Host ""
    
} else {
    Write-Host "??  CLEARTYPE MODE ONLY" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "When the app launches, verify:" -ForegroundColor White
    Write-Host ""
    Write-Host "1. Status Badge" -ForegroundColor Yellow
    Write-Host "   • Should show: '? Active: ClearType Optimization'" -ForegroundColor Gray
    Write-Host "   • Should NOT show shader indicator" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "2. Settings Application" -ForegroundColor Yellow
    Write-Host "   • Select a subpixel layout" -ForegroundColor Gray
    Write-Host "   • Click 'Apply'" -ForegroundColor Gray
    Write-Host "   • Registry settings should update" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "4. Process Monitoring (Advanced)" -ForegroundColor Yellow
Write-Host "   Run in separate PowerShell window:" -ForegroundColor Gray
Write-Host "   " -NoNewline
Write-Host 'Get-Process | Where-Object {$_.Modules.ModuleName -contains "DisplayShaderHook.dll"}' -ForegroundColor Cyan
Write-Host "   This shows which processes have the DLL loaded" -ForegroundColor Gray
Write-Host ""

Write-Host "5. Debug Output (if running from VS)" -ForegroundColor Yellow
Write-Host "   • Check Output window for injection logs" -ForegroundColor Gray
Write-Host "   • Should see '[InjectionManager] Injected into...' messages" -ForegroundColor Gray
Write-Host ""

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to launch the application..." -ForegroundColor White
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

$exePath = Join-Path $binPath "DisplayShadersPowerToy.exe"
if (Test-Path $exePath) {
    Start-Process $exePath
    Write-Host ""
    Write-Host "? Application launched!" -ForegroundColor Green
    Write-Host ""
    
    # Wait a bit and check if DLL gets loaded
    if (Test-Path $dllPath) {
        Write-Host "Waiting 3 seconds for injection..." -ForegroundColor Gray
        Start-Sleep -Seconds 3
        
        Write-Host ""
        Write-Host "Checking for loaded DLL in processes..." -ForegroundColor Yellow
        
        $loadedProcesses = Get-Process | Where-Object {
            try {
                $_.Modules.ModuleName -contains "DisplayShaderHook.dll"
            } catch {
                $false
            }
        }
        
        if ($loadedProcesses) {
            Write-Host "  ? DLL loaded in processes:" -ForegroundColor Green
            foreach ($proc in $loadedProcesses) {
                Write-Host "    • $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Green
            }
        } else {
            Write-Host "  ??  DLL not yet loaded (normal - injection happens on Apply)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "? Executable not found: $exePath" -ForegroundColor Red
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "   Test Complete - Verify UI Manually       " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Create a monitoring script
$monitorScript = @'
# Monitor DLL injection in real-time
Write-Host "Monitoring DisplayShaderHook.dll injection..." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
Write-Host ""

while ($true) {
    $procs = Get-Process | Where-Object {
        try {
            $_.Modules.ModuleName -contains "DisplayShaderHook.dll"
        } catch {
            $false
        }
    }
    
    Clear-Host
    Write-Host "=== DisplayShaderHook.dll Monitor ===" -ForegroundColor Cyan
    Write-Host ""
    
    if ($procs) {
        Write-Host "DLL loaded in $($procs.Count) process(es):" -ForegroundColor Green
        Write-Host ""
        foreach ($proc in $procs) {
            Write-Host "  ? $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Green
        }
    } else {
        Write-Host "DLL not currently loaded in any process" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "Last updated: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
    Write-Host "Press Ctrl+C to stop monitoring" -ForegroundColor Gray
    
    Start-Sleep -Seconds 2
}
'@

$monitorScript | Out-File "monitor-injection.ps1" -Encoding UTF8
Write-Host "?? Tip: Run " -NoNewline -ForegroundColor Gray
Write-Host ".\monitor-injection.ps1" -NoNewline -ForegroundColor Cyan
Write-Host " to monitor DLL injection in real-time" -ForegroundColor Gray
Write-Host ""
