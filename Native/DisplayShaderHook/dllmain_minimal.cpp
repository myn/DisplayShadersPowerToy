#include "Common.h"
#include "DirectWriteHook.h"
#include "SubpixelShader.h"
#include "ConfigLoader.h"
#include <thread>
#include <atomic>
#include <mutex>

using namespace DisplayShader;

// Configuration watcher thread
std::thread g_configWatcherThread;
std::atomic<bool> g_running(false);
std::atomic<bool> g_initialized(false);
std::mutex g_initMutex;

// Module handle
HMODULE g_hModule = nullptr;

// Config watcher function
void ConfigWatcherThreadFunc() {
    LogDebug(L"Config watcher thread started");

    ConfigLoader::Instance().WatchForChanges([](const ShaderConfig& config) {
        LogDebug(L"Config updated, applying changes...");
        DirectWriteHook::Instance().UpdateConfig(config);
    });

    LogDebug(L"Config watcher thread stopped");
}

// Lazy initialization - called from exported functions
bool EnsureInitialized() {
    // Quick check without lock
    if (g_initialized.load(std::memory_order_acquire)) {
        return true;
    }

    // Lock for thread-safe initialization
    std::lock_guard<std::mutex> lock(g_initMutex);

    // Double-check after acquiring lock
    if (g_initialized.load(std::memory_order_acquire)) {
        return true;
    }

    LogDebug(L"DisplayShaderHook lazy initialization starting...");

    try {
        // Initialize config loader
        if (!ConfigLoader::Instance().Initialize()) {
            LogError(L"Failed to initialize ConfigLoader");
            return false;
        }

        // Load initial configuration
        ShaderConfig initialConfig;
        if (ConfigLoader::Instance().LoadConfig(initialConfig)) {
            LogDebug(L"Initial config loaded");

            // Initialize DirectWrite hook
            if (DirectWriteHook::Instance().Initialize()) {
                DirectWriteHook::Instance().UpdateConfig(initialConfig);

                // Start config watcher thread
                g_running.store(true, std::memory_order_release);
                g_configWatcherThread = std::thread(ConfigWatcherThreadFunc);

                LogDebug(L"DisplayShaderHook initialized successfully");
            }
            else {
                LogError(L"Failed to initialize DirectWriteHook");
                return false;
            }
        }
        else {
            LogDebug(L"No initial config found, using defaults");
        }

        g_initialized.store(true, std::memory_order_release);
        return true;
    }
    catch (...) {
        LogError(L"Exception during initialization");
        return false;
    }
}

// DLL entry point - ABSOLUTE MINIMUM
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        // Absolute minimum - just store handle
        g_hModule = hModule;
        DisableThreadLibraryCalls(hModule);
        break;

    case DLL_PROCESS_DETACH:
        // Stop config watcher if running
        g_running.store(false, std::memory_order_release);
        if (g_configWatcherThread.joinable()) {
            g_configWatcherThread.join();
        }

        // Shutdown components if initialized
        if (g_initialized.load(std::memory_order_acquire)) {
            try {
                DirectWriteHook::Instance().Shutdown();
                SubpixelShader::Instance().Shutdown();
                ConfigLoader::Instance().Shutdown();
            }
            catch (...) {
                // Ignore exceptions during shutdown
            }
        }
        break;

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
    /// Absolutely minimal - no initialization, no dependencies
    /// </summary>
    DISPLAYSHADER_API int GetHookVersion() {
        return 1; // Version 1.0
    }

    /// <summary>
    /// Initialize the hook system (must be called before other functions)
    /// </summary>
    DISPLAYSHADER_API bool InitializeHook() {
        try {
            return EnsureInitialized();
        }
        catch (...) {
            return false;
        }
    }

    /// <summary>
    /// Force config reload
    /// </summary>
    DISPLAYSHADER_API bool ReloadConfig() {
        try {
            if (!EnsureInitialized()) {
                return false;
            }

            ShaderConfig config;
            if (ConfigLoader::Instance().LoadConfig(config)) {
                DirectWriteHook::Instance().UpdateConfig(config);
                return true;
            }
            return false;
        }
        catch (...) {
            return false;
        }
    }

    /// <summary>
    /// Check if hook is active
    /// </summary>
    DISPLAYSHADER_API bool IsHookActive() {
        try {
            if (!g_initialized.load(std::memory_order_acquire)) {
                return false;
            }
            return DirectWriteHook::Instance().IsActive();
        }
        catch (...) {
            return false;
        }
    }
}
