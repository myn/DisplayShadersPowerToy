#include "ConfigLoader.h"
#include <sstream>
#include <fstream>

namespace DisplayShader {

    ConfigLoader& ConfigLoader::Instance() {
        static ConfigLoader instance;
        return instance;
    }

    bool ConfigLoader::Initialize() {
        LogDebug(L"Initializing ConfigLoader (file-based)...");

        // Get the DLL directory
        wchar_t dllPath[MAX_PATH];
        HMODULE hModule = nullptr;
        
        if (GetModuleHandleExW(
            GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (LPCWSTR)&ConfigLoader::Instance,
            &hModule))
        {
            GetModuleFileNameW(hModule, dllPath, MAX_PATH);
            
            // Get directory from full path
            std::wstring path(dllPath);
            size_t lastSlash = path.find_last_of(L"\\");
            if (lastSlash != std::wstring::npos) {
                m_configDirectory = path.substr(0, lastSlash);
            }
        }

        if (m_configDirectory.empty()) {
            LogError(L"Failed to determine DLL directory");
            return false;
        }

        m_configFilePath = m_configDirectory + L"\\shader_config.ini";
        
        LogDebug(L"Config file path: %s", m_configFilePath.c_str());
        LogDebug(L"ConfigLoader initialized successfully (no admin required)");
        
        return true;
    }

    void ConfigLoader::Shutdown() {
        LogDebug(L"Shutting down ConfigLoader...");
        
        m_watching = false;
        
        if (m_changeNotification != INVALID_HANDLE_VALUE) {
            FindCloseChangeNotification(m_changeNotification);
            m_changeNotification = INVALID_HANDLE_VALUE;
        }

        LogDebug(L"ConfigLoader shutdown complete");
    }

    bool ConfigLoader::LoadConfig(ShaderConfig& outConfig) {
        // Try to read from INI file
        std::ifstream file(m_configFilePath);
        if (!file.is_open()) {
            LogDebug(L"Config file not found, using defaults");
            return false;
        }

        std::string line;
        while (std::getline(file, line)) {
            // Skip comments and empty lines
            if (line.empty() || line[0] == '#' || line[0] == '[') {
                continue;
            }

            // Parse key=value
            size_t pos = line.find('=');
            if (pos == std::string::npos) {
                continue;
            }

            std::string key = line.substr(0, pos);
            std::string value = line.substr(pos + 1);

            // Trim whitespace
            key.erase(0, key.find_first_not_of(" \t\r\n"));
            key.erase(key.find_last_not_of(" \t\r\n") + 1);
            value.erase(0, value.find_first_not_of(" \t\r\n"));
            value.erase(value.find_last_not_of(" \t\r\n") + 1);

            if (key == "Enabled") {
                outConfig.enabled = (value == "True" || value == "true" || value == "1");
            }
            else if (key == "Layout") {
                outConfig.layout = static_cast<SubpixelLayout>(std::stoi(value));
            }
            else if (key == "Intensity") {
                outConfig.intensity = std::stof(value);
            }
        }

        file.close();

        LogDebug(L"Loaded config from file: Layout=%d, Intensity=%.2f, Enabled=%d",
            static_cast<int>(outConfig.layout),
            outConfig.intensity,
            outConfig.enabled);

        return true;
    }

    void ConfigLoader::WatchForChanges(std::function<void(const ShaderConfig&)> callback) {
        m_watching = true;
        LogDebug(L"Started watching for config file changes...");

        // Create change notification for the config directory
        m_changeNotification = FindFirstChangeNotificationW(
            m_configDirectory.c_str(),
            FALSE, // Don't watch subtree
            FILE_NOTIFY_CHANGE_LAST_WRITE | FILE_NOTIFY_CHANGE_FILE_NAME);

        if (m_changeNotification == INVALID_HANDLE_VALUE) {
            LogError(L"Failed to create change notification: %d", GetLastError());
            return;
        }

        while (m_watching) {
            DWORD result = WaitForSingleObject(m_changeNotification, 1000);

            if (result == WAIT_OBJECT_0) {
                // File changed, wait a moment for write to complete
                Sleep(50);
                
                LogDebug(L"Config file changed, reloading...");

                ShaderConfig newConfig;
                if (LoadConfig(newConfig)) {
                    callback(newConfig);
                }

                // Reset notification
                FindNextChangeNotification(m_changeNotification);
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
