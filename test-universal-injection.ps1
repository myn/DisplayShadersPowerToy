# Test Universal Continuous Injection
# This script demonstrates the new automatic injection system

Write-Host "=== UNIVERSAL CONTINUOUS INJECTION TEST ===" -ForegroundColor Cyan
Write-Host ""

# Check if DLL exists
$dllPath = "bin\Debug\net8.0-windows\DisplayShaderHook.dll"
if (Test-Path $dllPath) {
    Write-Host "? DisplayShaderHook.dll found" -ForegroundColor Green
} else {
    Write-Host "? DisplayShaderHook.dll NOT found - build Native project first!" -ForegroundColor Red
    Write-Host "  Path: $dllPath" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "This test will:" -ForegroundColor Yellow
Write-Host "1. Monitor injected processes in real-time"
Write-Host "2. Open several test applications"
Write-Host "3. Verify automatic injection (2-3 sec delay)"
Write-Host "4. Show continuous monitoring in action"
Write-Host ""

# Function to count hooked processes
function Get-HookedProcessCount {
    $count = 0
    Get-Process | ForEach-Object {
        try {
            if ($_.Modules.ModuleName -contains "DisplayShaderHook.dll") {
                $count++
            }
        } catch {
            # Access denied - skip
        }
    }
    return $count
}

# Function to list hooked processes
function Get-HookedProcesses {
    $hooked = @()
    Get-Process | ForEach-Object {
        try {
            if ($_.Modules.ModuleName -contains "DisplayShaderHook.dll") {
                $hooked += "$($_.ProcessName) (PID: $($_.Id))"
            }
        } catch {
            # Access denied - skip
        }
    }
    return $hooked | Sort-Object
}

Write-Host "Step 1: Initial state" -ForegroundColor Cyan
Write-Host "----------------------------------------------"
$initial = Get-HookedProcessCount
Write-Host "Currently hooked processes: $initial" -ForegroundColor Yellow
if ($initial -gt 0) {
    Write-Host ""
    Get-HookedProcesses | ForEach-Object {
        Write-Host "  • $_" -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "Step 2: Starting DisplayShadersPowerToy..." -ForegroundColor Cyan
Write-Host "----------------------------------------------"
Write-Host "ACTION REQUIRED:" -ForegroundColor Yellow
Write-Host "1. Enable 'Shader Injection' toggle" -ForegroundColor White
Write-Host "2. Click 'Apply Settings'" -ForegroundColor White
Write-Host "3. Wait for monitoring to start..." -ForegroundColor White
Write-Host ""
Write-Host "Press ENTER when ready to continue test..." -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "Step 3: Opening test applications..." -ForegroundColor Cyan
Write-Host "----------------------------------------------"

# Open test apps with delays
Write-Host "Opening Notepad..." -ForegroundColor White
Start-Process notepad
Start-Sleep -Seconds 3

Write-Host "Checking for auto-injection..." -ForegroundColor Yellow
$notepadHooked = Get-Process notepad -ErrorAction SilentlyContinue | Where-Object {
    try { $_.Modules.ModuleName -contains "DisplayShaderHook.dll" } catch { $false }
}
if ($notepadHooked) {
    Write-Host "  ? Notepad HOOKED automatically!" -ForegroundColor Green
} else {
    Write-Host "  ? Notepad not hooked yet (wait 2-3 sec)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "Opening Calculator..." -ForegroundColor White
Start-Process calc
Start-Sleep -Seconds 3

Write-Host "Checking for auto-injection..." -ForegroundColor Yellow
$calcHooked = Get-Process CalculatorApp -ErrorAction SilentlyContinue | Where-Object {
    try { $_.Modules.ModuleName -contains "DisplayShaderHook.dll" } catch { $false }
}
if ($calcHooked) {
    Write-Host "  ? Calculator HOOKED automatically!" -ForegroundColor Green
} else {
    Write-Host "  ? Calculator not hooked yet (wait 2-3 sec)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "Opening Paint..." -ForegroundColor White
Start-Process mspaint
Start-Sleep -Seconds 3

Write-Host "Checking for auto-injection..." -ForegroundColor Yellow
$paintHooked = Get-Process mspaint -ErrorAction SilentlyContinue | Where-Object {
    try { $_.Modules.ModuleName -contains "DisplayShaderHook.dll" } catch { $false }
}
if ($paintHooked) {
    Write-Host "  ? Paint HOOKED automatically!" -ForegroundColor Green
} else {
    Write-Host "  ? Paint not hooked yet (wait 2-3 sec)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "Step 4: Final status" -ForegroundColor Cyan
Write-Host "----------------------------------------------"
$final = Get-HookedProcessCount
Write-Host "Total hooked processes: $final" -ForegroundColor Green
Write-Host "Change from initial: +$(($final - $initial))" -ForegroundColor Yellow
Write-Host ""

if ($final -gt $initial) {
    Write-Host "Hooked processes:" -ForegroundColor Cyan
    Get-HookedProcesses | ForEach-Object {
        Write-Host "  • $_" -ForegroundColor Green
    }
} else {
    Write-Host "WARNING: No new processes were hooked!" -ForegroundColor Red
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "  - Shader injection not enabled" -ForegroundColor White
    Write-Host "  - Apply not clicked" -ForegroundColor White
    Write-Host "  - DLL not loading correctly" -ForegroundColor White
}
Write-Host ""

Write-Host "Step 5: Real-time monitoring (30 seconds)" -ForegroundColor Cyan
Write-Host "----------------------------------------------"
Write-Host "Watching for changes..." -ForegroundColor Yellow
Write-Host ""

$startTime = Get-Date
while (((Get-Date) - $startTime).TotalSeconds -lt 30) {
    $count = Get-HookedProcessCount
    $elapsed = [int]((Get-Date) - $startTime).TotalSeconds
    
    Write-Host ("`rHooked processes: $count | Elapsed: $elapsed/30 sec") -NoNewline -ForegroundColor Green
    
    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host ""

Write-Host "=== TEST COMPLETE ===" -ForegroundColor Cyan
Write-Host ""

$finalCount = Get-HookedProcessCount
Write-Host "Final hooked process count: $finalCount" -ForegroundColor Green
Write-Host ""

if ($finalCount -gt $initial) {
    Write-Host "? SUCCESS!" -ForegroundColor Green
    Write-Host "  Universal continuous injection is working!" -ForegroundColor Green
    Write-Host "  New processes are being hooked automatically!" -ForegroundColor Green
} else {
    Write-Host "? PROBLEM DETECTED" -ForegroundColor Red
    Write-Host "  Injection may not be working correctly" -ForegroundColor Red
    Write-Host "  Check the app's status display" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Cleanup:" -ForegroundColor Cyan
Write-Host "Close the test apps? (Y/N)" -ForegroundColor Yellow
$cleanup = Read-Host

if ($cleanup -eq "Y" -or $cleanup -eq "y") {
    Get-Process notepad,CalculatorApp,mspaint -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "? Test apps closed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Check DisplayShadersPowerToy for:" -ForegroundColor Yellow
Write-Host "  • Status showing 'Monitoring ALL processes - N hooked'" -ForegroundColor White
Write-Host "  • Process list updating automatically" -ForegroundColor White
Write-Host "  • Count increasing when you open new GUI apps" -ForegroundColor White
Write-Host ""
