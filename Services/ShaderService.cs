using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DisplayShadersPowerToy.Models;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Service for managing DisplayShaderHook.dll integration
/// Handles DLL detection, initialization, and configuration updates via shared memory
/// </summary>
public class ShaderService : IDisposable
{
    private const string HookDllName = "DisplayShaderHook.dll";
    private const string ConfigFileName = "shader_config.ini";
    
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Check if DisplayShaderHook.dll is available
    /// </summary>
    public static bool IsHookDllAvailable()
    {
        try
        {
            string dllPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                HookDllName);

            bool exists = File.Exists(dllPath);
            
            Debug.WriteLine($"[ShaderService] Hook DLL check: {dllPath} - {(exists ? "Found" : "Not found")}");
            return exists;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Error checking for hook DLL: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get version of DisplayShaderHook.dll
    /// </summary>
    public static int GetHookVersion()
    {
        try
        {
            if (!IsHookDllAvailable())
            {
                return 0;
            }

            string dllPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                HookDllName);

            var versionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            return versionInfo.FileMajorPart;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Error getting hook version: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Initialize the shader service
    /// </summary>
    public bool Initialize()
    {
        if (_initialized)
        {
            Debug.WriteLine("[ShaderService] Already initialized");
            return true;
        }

        try
        {
            if (!IsHookDllAvailable())
            {
                Debug.WriteLine("[ShaderService] Hook DLL not available");
                return false;
            }

            Debug.WriteLine("[ShaderService] Initializing shader service (file-based config)");

            // Just verify we can write to the config file location
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                ConfigFileName);

            try
            {
                // Test write access
                File.WriteAllText(configPath, "# DisplayShader Config\r\n");
                Debug.WriteLine($"[ShaderService] Config file location: {configPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShaderService] Cannot write config file: {ex.Message}");
                return false;
            }
            
            _initialized = true;
            Debug.WriteLine("[ShaderService] Shader service initialized successfully (no admin required)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Initialization failed: {ex.Message}");
            _initialized = false;
            return false;
        }
    }

    /// <summary>
    /// Update shader configuration and notify all hooked processes
    /// </summary>
    public void UpdateShaderConfig(DisplaySettings settings)
    {
        if (!_initialized)
        {
            Debug.WriteLine("[ShaderService] Cannot update config - not initialized");
            return;
        }

        try
        {
            Debug.WriteLine($"[ShaderService] Updating shader config:");
            Debug.WriteLine($"  - Subpixel Layout: {settings.ShaderLayout}");
            Debug.WriteLine($"  - Enabled: {settings.EnableShaderInjection}");
            Debug.WriteLine($"  - Intensity: {settings.ShaderIntensity:F2}");

            // Write configuration to INI file
            WriteConfigFile(settings);

            Debug.WriteLine("[ShaderService] Config file updated (hooked processes will reload automatically)");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Failed to update shader config: {ex.Message}");
        }
    }

    /// <summary>
    /// Write configuration file for DisplayShaderHook.dll to read
    /// The DLL's FileSystemWatcher will detect changes and reload automatically
    /// </summary>
    private void WriteConfigFile(DisplaySettings settings)
    {
        try
        {
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                ConfigFileName);

            using var writer = new StreamWriter(configPath, false);
            
            // Write in INI format that matches what ConfigLoader expects
            writer.WriteLine("# DisplayShader Configuration");
            writer.WriteLine($"# Auto-generated: {DateTime.Now}");
            writer.WriteLine();
            writer.WriteLine("[Shader]");
            writer.WriteLine($"Enabled={settings.EnableShaderInjection}");
            writer.WriteLine($"Layout={settings.ShaderLayout}");
            writer.WriteLine($"Intensity={settings.ShaderIntensity:F4}");
            writer.WriteLine();
            writer.WriteLine("# Layout values:");
            writer.WriteLine("# 0 = RgbStripe (Standard LCD)");
            writer.WriteLine("# 1 = WrgbStripe (WOLED)");
            writer.WriteLine("# 2 = RgbTriangular (QD-OLED)");
            writer.WriteLine("# 3 = Pentile");
            writer.WriteLine("# 4 = None (Disabled)");

            Debug.WriteLine($"[ShaderService] Configuration written to: {configPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Failed to write config file: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Read current configuration from file
    /// </summary>
    public DisplaySettings ReadCurrentConfig()
    {
        var settings = new DisplaySettings();

        if (!_initialized)
        {
            return settings;
        }

        try
        {
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                ConfigFileName);

            if (!File.Exists(configPath))
            {
                return settings;
            }

            var lines = File.ReadAllLines(configPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "Enabled":
                        settings.EnableShaderInjection = bool.Parse(value);
                        break;
                    case "Layout":
                        settings.ShaderLayout = Enum.Parse<SubpixelLayout>(value);
                        break;
                    case "Intensity":
                        settings.ShaderIntensity = double.Parse(value);
                        break;
                }
            }

            return settings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Failed to read config file: {ex.Message}");
            return settings;
        }
    }

    /// <summary>
    /// Dispose of shader service resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            Debug.WriteLine("[ShaderService] Disposing shader service");
            
            _initialized = false;
            _disposed = true;

            Debug.WriteLine("[ShaderService] Shader service disposed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShaderService] Error during disposal: {ex.Message}");
        }
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~ShaderService()
    {
        Dispose();
    }
}
