#pragma once

#include "Common.h"
#include <functional>

namespace DisplayShader {

    /// <summary>
    /// Loads shader configuration from shared memory or file
    /// C# app writes config, this DLL reads it
    /// </summary>
    class ConfigLoader {
    public:
        static ConfigLoader& Instance();

        /// <summary>
        /// Initialize shared memory for config communication
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Shutdown and cleanup
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Read current configuration from shared memory
        /// </summary>
        bool LoadConfig(ShaderConfig& outConfig);

        /// <summary>
        /// Watch for config changes (blocking call)
        /// </summary>
        void WatchForChanges(std::function<void(const ShaderConfig&)> callback);

    private:
        ConfigLoader() = default;
        ~ConfigLoader() = default;

        ConfigLoader(const ConfigLoader&) = delete;
        ConfigLoader& operator=(const ConfigLoader&) = delete;

        HANDLE m_sharedMemory = nullptr;
        HANDLE m_configChangedEvent = nullptr;
        bool m_watching = false;

        static constexpr const wchar_t* SHARED_MEMORY_NAME = L"Global\\DisplayShaderConfig";
        static constexpr const wchar_t* CONFIG_EVENT_NAME = L"Global\\DisplayShaderConfigChanged";
    };

} // namespace DisplayShader
