using Microsoft.Win32;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Service for managing application settings and startup behavior
/// </summary>
public class SettingsService
{
    private const string StartupRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "DisplayShadersPowerToy";
    private const string SettingsRegistryPath = @"SOFTWARE\DisplayShadersPowerToy";

    /// <summary>
    /// Enable or disable application startup with Windows
    /// </summary>
    public void SetStartWithWindows(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
            if (key != null)
            {
                if (enable)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting startup: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if application is set to start with Windows
    /// </summary>
    public bool IsStartWithWindowsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Save application settings to registry
    /// </summary>
    public void SaveSettings(Models.DisplaySettings settings)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsRegistryPath);
            if (key != null)
            {
                // Save shader injection settings
                key.SetValue("EnableShaderInjection", settings.EnableShaderInjection ? 1 : 0);
                key.SetValue("ShaderLayout", (int)settings.ShaderLayout);
                key.SetValue("ShaderIntensity", settings.ShaderIntensity);
                
                // Save ClearType settings
                key.SetValue("EnableClearType", settings.EnableClearType ? 1 : 0);
                key.SetValue("ClearTypeLayout", (int)settings.ClearTypeLayout);
                key.SetValue("ClearTypeIntensity", settings.ClearTypeIntensity);
                
                // Save application settings
                key.SetValue("StartWithWindows", settings.StartWithWindows ? 1 : 0);
                key.SetValue("MinimizeToTray", settings.MinimizeToTray ? 1 : 0);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Load application settings from registry
    /// </summary>
    public Models.DisplaySettings LoadSettings()
    {
        var settings = new Models.DisplaySettings();

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsRegistryPath);
            if (key != null)
            {
                // Load shader injection settings
                var enableShaderInjection = key.GetValue("EnableShaderInjection");
                if (enableShaderInjection != null)
                {
                    settings.EnableShaderInjection = Convert.ToInt32(enableShaderInjection) == 1;
                }

                var shaderLayout = key.GetValue("ShaderLayout");
                if (shaderLayout != null)
                {
                    settings.ShaderLayout = (Models.SubpixelLayout)Convert.ToInt32(shaderLayout);
                }

                var shaderIntensity = key.GetValue("ShaderIntensity");
                if (shaderIntensity != null)
                {
                    settings.ShaderIntensity = Convert.ToDouble(shaderIntensity);
                }

                // Load ClearType settings
                var enableClearType = key.GetValue("EnableClearType");
                if (enableClearType != null)
                {
                    settings.EnableClearType = Convert.ToInt32(enableClearType) == 1;
                }

                var clearTypeLayout = key.GetValue("ClearTypeLayout");
                if (clearTypeLayout != null)
                {
                    settings.ClearTypeLayout = (Models.SubpixelLayout)Convert.ToInt32(clearTypeLayout);
                }

                var clearTypeIntensity = key.GetValue("ClearTypeIntensity");
                if (clearTypeIntensity != null)
                {
                    settings.ClearTypeIntensity = Convert.ToDouble(clearTypeIntensity);
                }

                // Load application settings
                var startWithWindows = key.GetValue("StartWithWindows");
                if (startWithWindows != null)
                {
                    settings.StartWithWindows = Convert.ToInt32(startWithWindows) == 1;
                }

                var minimizeToTray = key.GetValue("MinimizeToTray");
                if (minimizeToTray != null)
                {
                    settings.MinimizeToTray = Convert.ToInt32(minimizeToTray) == 1;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }

        return settings;
    }

    /// <summary>
    /// Clear all saved settings
    /// </summary>
    public void ClearSettings()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKey(SettingsRegistryPath, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing settings: {ex.Message}");
        }
    }
}
