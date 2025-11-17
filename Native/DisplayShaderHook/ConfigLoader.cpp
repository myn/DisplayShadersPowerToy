#include "ConfigLoader.h"

namespace DisplayShader {

    ConfigLoader& ConfigLoader::Instance() {
        static ConfigLoader instance;
        return instance;
    }

    bool ConfigLoader::Initialize() {
        LogDebug(L"Initializing ConfigLoader...");

        // Create or open shared memory
        m_sharedMemory = OpenFileMappingW(
            FILE_MAP_READ,
            FALSE,
            SHARED_MEMORY_NAME);

        if (!m_sharedMemory) {
            // If doesn't exist, create it (should normally be created by C# app)
            m_sharedMemory = CreateFileMappingW(
                INVALID_HANDLE_VALUE,
                nullptr,
                PAGE_READWRITE,
                0,
                sizeof(ShaderConfig),
                SHARED_MEMORY_NAME);

            if (!m_sharedMemory) {
                LogError(L"Failed to create shared memory: %d", GetLastError());
                return false;
            }

            LogDebug(L"Created shared memory");
        }
        else {
            LogDebug(L"Opened existing shared memory");
        }

        // Open config changed event
        m_configChangedEvent = OpenEventW(
            SYNCHRONIZE,
            FALSE,
            CONFIG_EVENT_NAME);

        if (!m_configChangedEvent) {
            // Create if doesn't exist
            m_configChangedEvent = CreateEventW(
                nullptr,
                FALSE, // Auto-reset
                FALSE, // Initial state: not signaled
                CONFIG_EVENT_NAME);

            if (!m_configChangedEvent) {
                LogError(L"Failed to create config event: %d", GetLastError());
                return false;
            }
        }

        LogDebug(L"ConfigLoader initialized successfully");
        return true;
    }

    void ConfigLoader::Shutdown() {
        LogDebug(L"Shutting down ConfigLoader...");

        m_watching = false;

        if (m_configChangedEvent) {
            CloseHandle(m_configChangedEvent);
            m_configChangedEvent = nullptr;
        }

        if (m_sharedMemory) {
            CloseHandle(m_sharedMemory);
            m_sharedMemory = nullptr;
        }

        LogDebug(L"ConfigLoader shutdown complete");
    }

    bool ConfigLoader::LoadConfig(ShaderConfig& outConfig) {
        if (!m_sharedMemory) {
            LogError(L"Shared memory not initialized");
            return false;
        }

        // Map view of file
        void* pBuf = MapViewOfFile(
            m_sharedMemory,
            FILE_MAP_READ,
            0,
            0,
            sizeof(ShaderConfig));

        if (!pBuf) {
            LogError(L"Failed to map view of file: %d", GetLastError());
            return false;
        }

        // Copy configuration
        memcpy(&outConfig, pBuf, sizeof(ShaderConfig));

        UnmapViewOfFile(pBuf);

        LogDebug(L"Loaded config: Layout=%d, Intensity=%.2f, Enabled=%d",
            static_cast<int>(outConfig.layout),
            outConfig.intensity,
            outConfig.enabled);

        return true;
    }

    void ConfigLoader::WatchForChanges(std::function<void(const ShaderConfig&)> callback) {
        if (!m_configChangedEvent) {
            LogError(L"Config event not initialized");
            return;
        }

        m_watching = true;
        LogDebug(L"Started watching for config changes...");

        while (m_watching) {
            DWORD result = WaitForSingleObject(m_configChangedEvent, 1000);

            if (result == WAIT_OBJECT_0) {
                LogDebug(L"Config changed, reloading...");

                ShaderConfig newConfig;
                if (LoadConfig(newConfig)) {
                    callback(newConfig);
                }
            }
            else if (result == WAIT_TIMEOUT) {
                // Normal, continue waiting
            }
            else {
                LogError(L"Wait failed: %d", GetLastError());
                break;
            }
        }

        LogDebug(L"Stopped watching for config changes");
    }

} // namespace DisplayShader
