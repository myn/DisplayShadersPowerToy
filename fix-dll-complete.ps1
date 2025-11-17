# Complete fix for DLL initialization failure

Write-Host "Applying comprehensive DLL initialization fix..." -ForegroundColor Cyan
Write-Host ""

# Step 1: Update dllmain.cpp with minimal version
Write-Host "[1/3] Updating dllmain.cpp with ultra-minimal version..." -ForegroundColor Yellow
Copy-Item "Native\DisplayShaderHook\dllmain_minimal.cpp" "Native\DisplayShaderHook\dllmain.cpp" -Force
Write-Host "  ? Removed ALL operations from DllMain except storing handle" -ForegroundColor Green

# Step 2: Update ShaderService.cs IsHookDllAvailable
Write-Host ""
Write-Host "[2/3] Fixing ShaderService.IsHookDllAvailable()..." -ForegroundColor Yellow

$shaderServicePath = "Services\ShaderService.cs"
$content = Get-Content $shaderServicePath -Raw

# Replace the IsHookDllAvailable method
$newMethod = @'
    /// <summary>
    /// Check if native hook DLL is available
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

# Find and replace the method
$pattern = '(?s)/// <summary>\s*/// Check if native hook DLL is available.*?return false;\s*}\s*}'
$content = $content -replace $pattern, $newMethod

Set-Content $shaderServicePath $content -NoNewline
Write-Host "  ? Removed GetHookVersion() call from detection" -ForegroundColor Green

# Step 3: Clean up temp files
Write-Host ""
Write-Host "[3/3] Cleaning up temporary files..." -ForegroundColor Yellow
$tempFiles = @(
    "Native\DisplayShaderHook\dllmain_fixed.cpp",
    "Services\ShaderService_fixed.cs",
    "ShaderService_detection_fix.txt"
)

foreach ($file in $tempFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force
        Write-Host "  ? Removed $file" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "? Fix applied successfully!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

Write-Host "Changes made:" -ForegroundColor Cyan
Write-Host "  1. DllMain now does NOTHING except store handle" -ForegroundColor White
Write-Host "  2. All logging removed from DllMain" -ForegroundColor White
Write-Host "  3. IsHookDllAvailable() only checks file existence" -ForegroundColor White
Write-Host "  4. No DLL loading until Initialize() is called" -ForegroundColor White
Write-Host ""

Write-Host "Now rebuild:" -ForegroundColor Cyan
Write-Host '  msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild /verbosity:minimal' -ForegroundColor White
Write-Host '  dotnet clean' -ForegroundColor White
Write-Host '  dotnet build --configuration Debug' -ForegroundColor White
Write-Host ""

Write-Host "Then press F5 and check for:" -ForegroundColor Yellow
Write-Host "  [DisplayShaderService] Shader mode available: True" -ForegroundColor Green
Write-Host "  [ShaderService] Initializing native hook..." -ForegroundColor Green
Write-Host "  [ShaderService] Native hook initialization: Success" -ForegroundColor Green
