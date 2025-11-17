#pragma once

#include "Common.h"
#include <functional>
#include <string>

namespace DisplayShader {

    /// <summary>
    /// Loads shader configuration from INI file
    /// C# app writes config file, this DLL reads it
    /// Uses FindFirstChangeNotification to detect file changes
    /// NO ADMIN RIGHTS REQUIRED!
    /// </summary>
    class ConfigLoader {
    public:
        static ConfigLoader& Instance();

        /// <summary>
        /// Initialize file watcher for config
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Shutdown and cleanup
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Read current configuration from INI file
        /// </summary>
        bool LoadConfig(ShaderConfig& outConfig);

        /// <summary>
        /// Watch for config file changes (blocking call)
        /// </summary>
        void WatchForChanges(std::function<void(const ShaderConfig&)> callback);

    private:
        ConfigLoader() = default;
        ~ConfigLoader() = default;

        ConfigLoader(const ConfigLoader&) = delete;
        ConfigLoader& operator=(const ConfigLoader&) = delete;

        std::wstring m_configDirectory;
        std::wstring m_configFilePath;
        HANDLE m_changeNotification = INVALID_HANDLE_VALUE;
        bool m_watching = false;
    };

} // namespace DisplayShader
