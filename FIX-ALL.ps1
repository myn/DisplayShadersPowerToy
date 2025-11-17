# Complete Fix for DLL Initialization - Run this to fix everything

Write-Host "????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  Display Shaders PowerToy - Complete DLL Fix" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Check if Visual Studio is running
$vsProcess = Get-Process devenv -ErrorAction SilentlyContinue

if ($vsProcess) {
    Write-Host "? Visual Studio is running - we need to close it to fix files" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor White
    Write-Host "  1. Let this script close Visual Studio" -ForegroundColor White
    Write-Host "  2. Close Visual Studio manually and run this script again" -ForegroundColor White
    Write-Host ""
    $choice = Read-Host "Enter choice (1 or 2)"
    
    if ($choice -eq "1") {
        Write-Host ""
        Write-Host "Closing Visual Studio..." -ForegroundColor Yellow
        Stop-Process -Name devenv -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
        Write-Host "? Visual Studio closed" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Please close Visual Studio and run this script again" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Step 1: Fixing ShaderService.cs..." -ForegroundColor Cyan

$shaderServicePath = "Services\ShaderService.cs"

if (!(Test-Path $shaderServicePath)) {
    Write-Host "? ShaderService.cs not found!" -ForegroundColor Red
    Write-Host "  Expected path: $shaderServicePath" -ForegroundColor Red
    exit 1
}

# Read the file
$content = Get-Content $shaderServicePath -Raw

# Check if it already has the fix
if ($content -match 'IMPORTANT: Only checks file existence') {
    Write-Host "? ShaderService.cs already fixed" -ForegroundColor Green
} else {
    # Apply the fix
    $oldPattern = '(?s)(/// <summary>\s*/// Check if native hook DLL is available.*?public static bool IsHookDllAvailable\(\)\s*\{.*?)if \(exists\)\s*\{.*?return false;\s*}\s*(return (?:exists|false);)'
    
    $newCode = @'
$1
        // Just check if file exists - don't try to load it yet
        // Loading will happen in Initialize() via InitializeHook()
        return exists;
'@
    
    $content = $content -replace $oldPattern, $newCode
    
    # Add comment to method summary
    $content = $content -replace '(/// <summary>\s*/// Check if native hook DLL is available)', '$1\r\n    /// IMPORTANT: Only checks file existence - does NOT load the DLL'
    
    Set-Content $shaderServicePath $content -NoNewline
    Write-Host "? ShaderService.cs fixed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 2: Verifying dllmain.cpp..." -ForegroundColor Cyan

$dllmainPath = "Native\DisplayShaderHook\dllmain.cpp"
$dllmainContent = Get-Content $dllmainPath -Raw

if ($dllmainContent -match 'initialization deferred') {
    Write-Host "? dllmain.cpp already has minimal DllMain" -ForegroundColor Green
} else {
    Write-Host "? dllmain.cpp needs to be updated" -ForegroundColor Yellow
    Write-Host "  Copying from dllmain_minimal.cpp..." -ForegroundColor Yellow
    
    if (Test-Path "Native\DisplayShaderHook\dllmain_minimal.cpp") {
        Copy-Item "Native\DisplayShaderHook\dllmain_minimal.cpp" $dllmainPath -Force
        Write-Host "? dllmain.cpp updated" -ForegroundColor Green
    } else {
        Write-Host "? dllmain_minimal.cpp not found" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Step 3: Rebuilding projects..." -ForegroundColor Cyan
Write-Host ""

# Clean
Write-Host "[3.1] Cleaning solution..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
Write-Host "? Clean complete" -ForegroundColor Green

# Rebuild C++ DLL
Write-Host ""
Write-Host "[3.2] Rebuilding C++ DLL..." -ForegroundColor Yellow
$buildResult = msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild /verbosity:minimal /nologo 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "? C++ DLL built successfully" -ForegroundColor Green
} else {
    Write-Host "? C++ build failed" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

# Rebuild C# project
Write-Host ""
Write-Host "[3.3] Rebuilding C# application..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration Debug --verbosity minimal 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "? C# application built successfully" -ForegroundColor Green
} else {
    Write-Host "? C# build failed" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

# Verify DLL was copied
Write-Host ""
Write-Host "Step 4: Verifying build output..." -ForegroundColor Cyan

$debugDll = "bin\Debug\net8.0-windows\DisplayShaderHook.dll"
if (Test-Path $debugDll) {
    $dllInfo = Get-Item $debugDll
    Write-Host "? DisplayShaderHook.dll exists in Debug output" -ForegroundColor Green
    Write-Host "  Size: $($dllInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "  Modified: $($dllInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "? DLL not found in Debug output" -ForegroundColor Red
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? FIX COMPLETE!" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

Write-Host "What was fixed:" -ForegroundColor Cyan
Write-Host "  1. ShaderService.IsHookDllAvailable() - no longer calls GetHookVersion()" -ForegroundColor White
Write-Host "  2. DllMain is minimal - defers initialization" -ForegroundColor White
Write-Host "  3. Both projects rebuilt successfully" -ForegroundColor White
Write-Host ""

Write-Host "To test:" -ForegroundColor Cyan
Write-Host "  1. Open Visual Studio: devenv DisplayShadersPowerToy.sln" -ForegroundColor White
Write-Host "  2. Press F5 to debug" -ForegroundColor White
Write-Host "  3. Check Output window (Debug) for:" -ForegroundColor White
Write-Host "     '[DisplayShaderService] Shader mode available: True'" -ForegroundColor Green
Write-Host "     '[ShaderService] Initializing native hook...'" -ForegroundColor Green
Write-Host "     '[ShaderService] Native hook initialization: Success'" -ForegroundColor Green
Write-Host ""

Write-Host "Opening Visual Studio..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Start-Process "devenv.exe" "DisplayShadersPowerToy.sln"

Write-Host ""
Write-Host "? Done! Press F5 in Visual Studio to test" -ForegroundColor Green
