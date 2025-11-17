# Quick fix for C++ compilation errors

Write-Host "Fixing C++ compilation errors..." -ForegroundColor Cyan

# Fix 1: Add #include <functional> to ConfigLoader.h
$configLoaderH = "Native\DisplayShaderHook\ConfigLoader.h"
$content = Get-Content $configLoaderH -Raw
if ($content -notmatch "#include <functional>") {
    $content = $content.Replace("#include `"Common.h`"", "#include `"Common.h`"`n#include <functional>")
    Set-Content $configLoaderH $content -NoNewline
    Write-Host "  ? Fixed ConfigLoader.h - added #include <functional>" -ForegroundColor Green
}

# Fix 2: Fix MinHook.cpp include path
$minHookCpp = "Native\DisplayShaderHook\MinHook.cpp"
$content = Get-Content $minHookCpp -Raw
$content = $content.Replace('#include "../include/MinHook.h"', '#include "include/MinHook.h"')
Set-Content $minHookCpp $content -NoNewline
Write-Host "  ? Fixed MinHook.cpp - corrected include path" -ForegroundColor Green

Write-Host ""
Write-Host "Fixes applied! Rebuild now..." -ForegroundColor Green
