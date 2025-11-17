# Fix ShaderService.cs - Remove GetHookVersion() from detection

Write-Host "Fixing ShaderService.cs..." -ForegroundColor Cyan
Write-Host ""
Write-Host "? This requires closing Visual Studio" -ForegroundColor Yellow
Write-Host ""

$vsRunning = Get-Process devenv -ErrorAction SilentlyContinue
if ($vsRunning) {
    Write-Host "Visual Studio is currently running (PID: $($vsRunning.Id))" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Close Visual Studio now? (Y/N)"
    
    if ($response -eq 'Y' -or $response -eq 'y') {
        Write-Host "Closing Visual Studio..." -ForegroundColor Yellow
        Stop-Process -Id $vsRunning.Id -Force
        Start-Sleep -Seconds 2
        Write-Host "? Visual Studio closed" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Please close Visual Studio manually, then run this script again" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host ""
Write-Host "Reading ShaderService.cs..." -ForegroundColor Yellow

$filePath = "Services\ShaderService.cs"
$content = Get-Content $filePath -Raw

# Replace the IsHookDllAvailable method
$newMethod = @'
    /// <summary>
    /// Check if native hook DLL is available
    /// IMPORTANT: Only checks file existence - does NOT load the DLL
    /// </summary>
    public static bool IsHookDllAvailable()
    {
        string dllPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DisplayShaderHook.dll");

        bool exists = File.Exists(dllPath);
        
        System.Diagnostics.Debug.WriteLine($"[ShaderService] Checking for DLL: {dllPath}");
        System.Diagnostics.Debug.WriteLine($"[ShaderService] DLL exists: {exists}");
        
        // Just check if file exists - don't try to load it yet
        // Loading will happen in Initialize() via InitializeHook()
        return exists;
    }
'@

# Pattern to match the old method (with all the try-catch blocks)
$pattern = '(?s)/// <summary>\s*/// Check if native hook DLL is available.*?}\s*return false;\s*}'

if ($content -match $pattern) {
    $content = $content -replace $pattern, $newMethod
    
    Set-Content $filePath $content -NoNewline
    
    Write-Host "? ShaderService.cs updated successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Changes made:" -ForegroundColor Cyan
    Write-Host "  - Removed GetHookVersion() call from IsHookDllAvailable()" -ForegroundColor White
    Write-Host "  - Now only checks if DLL file exists" -ForegroundColor White
    Write-Host "  - DLL will be loaded only during Initialize()" -ForegroundColor White
} else {
    Write-Host "? Method already appears to be fixed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Now rebuild the project:" -ForegroundColor Cyan
Write-Host "  dotnet clean" -ForegroundColor White
Write-Host "  dotnet build --configuration Debug" -ForegroundColor White
Write-Host ""
Write-Host "Then open Visual Studio and press F5 to test" -ForegroundColor Yellow
