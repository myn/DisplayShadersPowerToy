# DisplayShadersPowerToy - Production Build Script
# This script builds both C# and Native C++ projects and validates the output

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipNative,
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host " Display Shaders PowerToy - Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if running in correct directory
if (-not (Test-Path "DisplayShadersPowerToy.csproj")) {
    Write-Host "ERROR: Must run from project root directory" -ForegroundColor Red
    exit 1
}

# Clean if requested
if ($Clean) {
    Write-Host "[Clean] Removing build artifacts..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path "Native\DisplayShaderHook\x64") { Remove-Item -Recurse -Force "Native\DisplayShaderHook\x64" }
    Write-Host "  ? Clean complete" -ForegroundColor Green
    Write-Host ""
}

$BuildSuccess = $true
$NativeDllBuilt = $false

# Build Native C++ DLL
if (-not $SkipNative) {
    Write-Host "[1/3] Building Native C++ Hook DLL..." -ForegroundColor Cyan
    
    # Check if MSBuild is available
    $msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
        -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
        -prerelease | Select-Object -First 1
    
    if (-not $msbuild) {
        Write-Host "  ? MSBuild not found - Native C++ build skipped" -ForegroundColor Yellow
        Write-Host "    Install Visual Studio 2022 with C++ Desktop Development workload" -ForegroundColor Gray
        Write-Host "    The application will run in ClearType fallback mode" -ForegroundColor Gray
    } else {
        Write-Host "  Using MSBuild: $msbuild" -ForegroundColor Gray
        
        $vcxproj = "Native\DisplayShaderHook\DisplayShaderHook.vcxproj"
        
        try {
            & $msbuild $vcxproj `
                /p:Configuration=$Configuration `
                /p:Platform=x64 `
                /p:OutDir="..\..\bin\x64\$Configuration\" `
                /verbosity:minimal `
                /nologo
            
            if ($LASTEXITCODE -eq 0) {
                $dllPath = "bin\x64\$Configuration\DisplayShaderHook.dll"
                if (Test-Path $dllPath) {
                    Write-Host "  ? Native DLL built successfully" -ForegroundColor Green
                    Write-Host "    Location: $dllPath" -ForegroundColor Gray
                    $NativeDllBuilt = $true
                } else {
                    Write-Host "  ? DLL file not found at expected location" -ForegroundColor Red
                    $BuildSuccess = $false
                }
            } else {
                Write-Host "  ? Native build failed with exit code $LASTEXITCODE" -ForegroundColor Red
                $BuildSuccess = $false
            }
        } catch {
            Write-Host "  ? Native build failed: $($_.Exception.Message)" -ForegroundColor Red
            $BuildSuccess = $false
        }
    }
    Write-Host ""
} else {
    Write-Host "[1/3] Skipping Native C++ build (--SkipNative flag)" -ForegroundColor Yellow
    Write-Host ""
}

# Build C# Application
Write-Host "[2/3] Building C# Application..." -ForegroundColor Cyan

try {
    $output = dotnet build --configuration $Configuration --verbosity minimal 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? C# project built successfully" -ForegroundColor Green
        
        # Check output directory
        $exePath = "bin\$Configuration\net8.0-windows\DisplayShadersPowerToy.exe"
        if (Test-Path $exePath) {
            Write-Host "    Location: $exePath" -ForegroundColor Gray
            
            # Check if native DLL was copied
            $dllInOutput = "bin\$Configuration\net8.0-windows\DisplayShaderHook.dll"
            if (Test-Path $dllInOutput) {
                Write-Host "    ? Native DLL copied to output directory" -ForegroundColor Green
            } elseif ($NativeDllBuilt) {
                # Copy manually if not auto-copied
                Write-Host "    Copying Native DLL to output directory..." -ForegroundColor Yellow
                $sourceDll = "bin\x64\$Configuration\DisplayShaderHook.dll"
                if (Test-Path $sourceDll) {
                    Copy-Item $sourceDll $dllInOutput -Force
                    Write-Host "    ? Native DLL copied manually" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  ? Executable not found at expected location" -ForegroundColor Red
            $BuildSuccess = $false
        }
    } else {
        Write-Host "  ? C# build failed" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        $BuildSuccess = $false
    }
} catch {
    Write-Host "  ? C# build failed: $($_.Exception.Message)" -ForegroundColor Red
    $BuildSuccess = $false
}

Write-Host ""

# Run Tests if requested
if ($RunTests -and $BuildSuccess) {
    Write-Host "[3/3] Running Tests..." -ForegroundColor Cyan
    
    if (Test-Path ".\test-production.ps1") {
        & ".\test-production.ps1" -Configuration $Configuration
    } else {
        Write-Host "  ? Test script not found" -ForegroundColor Yellow
    }
} else {
    Write-Host "[3/3] Tests skipped" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host " Build Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

if ($BuildSuccess) {
    Write-Host "? Build completed successfully!" -ForegroundColor Green
    Write-Host ""
    
    if ($NativeDllBuilt) {
        Write-Host "Mode: Full Shader Mode" -ForegroundColor Green
        Write-Host "  - Native C++ hooks available" -ForegroundColor Gray
        Write-Host "  - Real DirectWrite shader support" -ForegroundColor Gray
    } else {
        Write-Host "Mode: ClearType Fallback Mode" -ForegroundColor Yellow
        Write-Host "  - Native DLL not available" -ForegroundColor Gray
        Write-Host "  - Using registry-based ClearType adjustments" -ForegroundColor Gray
        Write-Host "  - Build Native project for full shader support" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "To run the application:" -ForegroundColor Cyan
    Write-Host "  cd bin\$Configuration\net8.0-windows" -ForegroundColor White
    Write-Host "  .\DisplayShadersPowerToy.exe" -ForegroundColor White
    Write-Host ""
    
    exit 0
} else {
    Write-Host "? Build failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the errors above for details" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
