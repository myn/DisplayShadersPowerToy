# Complete System Build Script
# Builds both Native DLL and C# application

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipNative,
    
    [Parameter(Mandatory=$false)]
    [switch]$Test
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Display Shaders PowerToy - Full Build     " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host ""

$ErrorActionPreference = "Stop"
$buildSuccess = $true

# Step 1: Build Native DLL (if MSBuild available and not skipped)
if (-not $SkipNative) {
    Write-Host "[Step 1] Building Native DLL..." -ForegroundColor Yellow
    
    $nativeProject = "Native\DisplayShaderHook\DisplayShaderHook.vcxproj"
    
    if (Test-Path $nativeProject) {
        # Try to find MSBuild
        $msbuild = $null
        
        # Try VS 2022
        $msbuildPaths = @(
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
        )
        
        foreach ($path in $msbuildPaths) {
            if (Test-Path $path) {
                $msbuild = $path
                break
            }
        }
        
        if ($msbuild) {
            Write-Host "  Found MSBuild: $msbuild" -ForegroundColor Gray
            Write-Host "  Building..." -ForegroundColor Gray
            
            try {
                & $msbuild $nativeProject /p:Configuration=$Configuration /p:Platform=x64 /v:minimal /nologo
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  ? Native DLL built successfully" -ForegroundColor Green
                    
                    # Copy DLL to C# output directory
                    $dllSource = "bin\x64\$Configuration\DisplayShaderHook.dll"
                    $dllDest = "bin\$Configuration\net8.0-windows\DisplayShaderHook.dll"
                    
                    if (Test-Path $dllSource) {
                        $destDir = Split-Path $dllDest -Parent
                        if (-not (Test-Path $destDir)) {
                            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                        }
                        
                        Copy-Item $dllSource $dllDest -Force
                        Write-Host "  ? DLL copied to C# output: $dllDest" -ForegroundColor Green
                    } else {
                        Write-Host "  ? DLL not found at: $dllSource" -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "  ? Native DLL build failed" -ForegroundColor Red
                    $buildSuccess = $false
                }
            } catch {
                Write-Host "  ? Native build error: $($_.Exception.Message)" -ForegroundColor Red
                $buildSuccess = $false
            }
        } else {
            Write-Host "  ? MSBuild not found - skipping Native DLL build" -ForegroundColor Yellow
            Write-Host "    Install Visual Studio 2022 with C++ workload to build Native DLL" -ForegroundColor Gray
            Write-Host "    Shader mode will not be available without the DLL" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ? Native project not found: $nativeProject" -ForegroundColor Yellow
    }
} else {
    Write-Host "[Step 1] Skipping Native DLL build (--SkipNative specified)" -ForegroundColor Yellow
}

Write-Host ""

# Step 2: Build C# Application
Write-Host "[Step 2] Building C# Application..." -ForegroundColor Yellow

try {
    dotnet build -c $Configuration --nologo --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? C# application built successfully" -ForegroundColor Green
    } else {
        Write-Host "  ? C# build failed" -ForegroundColor Red
        $buildSuccess = $false
    }
} catch {
    Write-Host "  ? C# build error: $($_.Exception.Message)" -ForegroundColor Red
    $buildSuccess = $false
}

Write-Host ""

# Step 3: Verify output
Write-Host "[Step 3] Verifying output..." -ForegroundColor Yellow

$outputDir = "bin\$Configuration\net8.0-windows"
$exePath = Join-Path $outputDir "DisplayShadersPowerToy.exe"
$dllPath = Join-Path $outputDir "DisplayShaderHook.dll"

if (Test-Path $exePath) {
    Write-Host "  ? Main executable: $exePath" -ForegroundColor Green
    $exeInfo = Get-Item $exePath
    Write-Host "    Size: $($exeInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "    Modified: $($exeInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "  ? Main executable not found!" -ForegroundColor Red
    $buildSuccess = $false
}

if (Test-Path $dllPath) {
    Write-Host "  ? Shader DLL: $dllPath" -ForegroundColor Green
    $dllInfo = Get-Item $dllPath
    Write-Host "    Size: $($dllInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "    Modified: $($dllInfo.LastWriteTime)" -ForegroundColor Gray
    
    try {
        $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath)
        if ($version.FileMajorPart -gt 0) {
            Write-Host "    Version: $($version.FileMajorPart).$($version.FileMinorPart).$($version.FileBuildPart)" -ForegroundColor Gray
        }
    } catch {}
} else {
    Write-Host "  ? Shader DLL not found (shader mode will not be available)" -ForegroundColor Yellow
    Write-Host "    Run with Visual Studio to build Native project" -ForegroundColor Gray
}

Write-Host ""

# Step 4: Run tests (if requested)
if ($Test -and $buildSuccess) {
    Write-Host "[Step 4] Running tests..." -ForegroundColor Yellow
    
    if (Test-Path "test-complete-system.ps1") {
        & .\test-complete-system.ps1
    } else {
        Write-Host "  ? Test script not found" -ForegroundColor Yellow
    }
} elseif ($Test) {
    Write-Host "[Step 4] Skipping tests (build failed)" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Build Summary                              " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if ($buildSuccess) {
    Write-Host ""
    Write-Host "? BUILD SUCCESSFUL" -ForegroundColor Green
    Write-Host ""
    
    if (Test-Path $dllPath) {
        Write-Host "Shader Mode: AVAILABLE ?" -ForegroundColor Green
        Write-Host "  • Native DLL present" -ForegroundColor Gray
        Write-Host "  • Injection enabled" -ForegroundColor Gray
        Write-Host "  • Full functionality" -ForegroundColor Gray
    } else {
        Write-Host "Shader Mode: NOT AVAILABLE ?" -ForegroundColor Yellow
        Write-Host "  • Native DLL missing" -ForegroundColor Gray
        Write-Host "  • ClearType mode only" -ForegroundColor Gray
        Write-Host "  • Build Native project to enable" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "To run:" -ForegroundColor White
    Write-Host "  $exePath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To test:" -ForegroundColor White
    Write-Host "  .\test-complete-system.ps1" -ForegroundColor Cyan
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "? BUILD FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check error messages above" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

# Build info file
$buildInfo = @"
Build Information
=================
Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Configuration: $Configuration
Platform: x64 / .NET 8

Components:
- C# Application: $(if (Test-Path $exePath) { "?" } else { "?" })
- Native DLL: $(if (Test-Path $dllPath) { "?" } else { "?" })

Shader Mode: $(if (Test-Path $dllPath) { "AVAILABLE" } else { "NOT AVAILABLE" })

Output Directory: $outputDir
"@

$buildInfo | Out-File "BUILD_INFO.txt" -Encoding UTF8
Write-Host "Build info saved to BUILD_INFO.txt" -ForegroundColor Gray
Write-Host ""
