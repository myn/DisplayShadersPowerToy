# Comprehensive C++ Build Fixes

Write-Host "Applying comprehensive C++ fixes..." -ForegroundColor Cyan
Write-Host ""

# Fix 1: Add MH_ALL_HOOKS definition to MinHook.h
Write-Host "[Fix 1] Adding MH_ALL_HOOKS to MinHook.h..." -ForegroundColor Yellow
$minHookH = "Native\DisplayShaderHook\include\MinHook.h"
$content = Get-Content $minHookH -Raw
if ($content -notmatch "#define MH_ALL_HOOKS") {
    # Add after the enum definition
    $content = $content.Replace("} MH_STATUS;", @"
} MH_STATUS;

// Special value for all hooks
#define MH_ALL_HOOKS NULL
"@)
    Set-Content $minHookH $content -NoNewline
    Write-Host "  ? Added MH_ALL_HOOKS definition" -ForegroundColor Green
}

# Fix 2: Fix DirectWriteHook.cpp include path
Write-Host "[Fix 2] Fixing DirectWriteHook.cpp include path..." -ForegroundColor Yellow
$directWriteHookCpp = "Native\DisplayShaderHook\DirectWriteHook.cpp"
$content = Get-Content $directWriteHookCpp -Raw
$content = $content.Replace('#include "../include/MinHook.h"', '#include "include/MinHook.h"')
Set-Content $directWriteHookCpp $content -NoNewline
Write-Host "  ? Fixed include path" -ForegroundColor Green

# Fix 3: Fix dllmain.cpp switch statement scope issue
Write-Host "[Fix 3] Fixing dllmain.cpp switch statement..." -ForegroundColor Yellow
$dllMainCpp = "Native\DisplayShaderHook\dllmain.cpp"
$content = Get-Content $dllMainCpp -Raw

# Wrap the case block in curly braces to fix scope issue
$content = $content -replace '(case DLL_PROCESS_ATTACH:\s+)LogDebug', '$1{${NewLine}        LogDebug'
$content = $content -replace '(break;)(\s+case DLL_PROCESS_DETACH:)', '}${NewLine}$1$2'

# Actually, let's just rewrite the problematic section
$newDllMainContent = @'
#include "Common.h"
#include "DirectWriteHook.h"
#include "SubpixelShader.h"
#include "ConfigLoader.h"
#include <thread>

using namespace DisplayShader;

// Configuration watcher thread
std::thread g_configWatcherThread;
bool g_running = false;

// Config watcher function
void ConfigWatcherThreadFunc() {
    LogDebug(L"Config watcher thread started");

    ConfigLoader::Instance().WatchForChanges([](const ShaderConfig& config) {
        LogDebug(L"Config updated, applying changes...");
        DirectWriteHook::Instance().UpdateConfig(config);
    });

    LogDebug(L"Config watcher thread stopped");
}

// DLL entry point
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
    {
        LogDebug(L"DisplayShaderHook.dll loaded into process");

        // Disable thread library calls for performance
        DisableThreadLibraryCalls(hModule);

        // Initialize config loader
        if (!ConfigLoader::Instance().Initialize()) {
            LogError(L"Failed to initialize ConfigLoader");
            return FALSE;
        }

        // Load initial configuration
        ShaderConfig initialConfig;
        if (ConfigLoader::Instance().LoadConfig(initialConfig)) {
            LogDebug(L"Initial config loaded");

            // Initialize DirectWrite hook
            if (DirectWriteHook::Instance().Initialize()) {
                DirectWriteHook::Instance().UpdateConfig(initialConfig);

                // Start config watcher thread
                g_running = true;
                g_configWatcherThread = std::thread(ConfigWatcherThreadFunc);

                LogDebug(L"DisplayShaderHook initialized successfully");
            }
            else {
                LogError(L"Failed to initialize DirectWriteHook");
                return FALSE;
            }
        }
        else {
            LogDebug(L"No initial config found, using defaults");
        }

        break;
    }

    case DLL_PROCESS_DETACH:
    {
        LogDebug(L"DisplayShaderHook.dll unloading from process");

        // Stop config watcher
        g_running = false;
        if (g_configWatcherThread.joinable()) {
            g_configWatcherThread.join();
        }

        // Shutdown components
        DirectWriteHook::Instance().Shutdown();
        SubpixelShader::Instance().Shutdown();
        ConfigLoader::Instance().Shutdown();

        LogDebug(L"DisplayShaderHook shutdown complete");
        break;
    }

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }

    return TRUE;
}

// Exported functions for C# interop
extern "C" {
    /// <summary>
    /// Get hook version (for compatibility checking)
    /// </summary>
    DISPLAYSHADER_API int GetHookVersion() {
        return 1; // Version 1.0
    }

    /// <summary>
    /// Force config reload (can be called from C#)
    /// </summary>
    DISPLAYSHADER_API bool ReloadConfig() {
        ShaderConfig config;
        if (ConfigLoader::Instance().LoadConfig(config)) {
            DirectWriteHook::Instance().UpdateConfig(config);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if hook is active
    /// </summary>
    DISPLAYSHADER_API bool IsHookActive() {
        return DirectWriteHook::Instance().IsActive();
    }
}
'@

Set-Content $dllMainCpp $newDllMainContent -NoNewline
Write-Host "  ? Fixed switch statement scope issues" -ForegroundColor Green

Write-Host ""
Write-Host "All fixes applied successfully!" -ForegroundColor Green
Write-Host "Ready to rebuild..." -ForegroundColor Cyan
