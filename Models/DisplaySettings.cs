using System.Collections.Generic;

namespace DisplayShadersPowerToy.Models;

public class DisplaySettings
{
    // Global Settings
    public bool EnableShaderInjection { get; set; } = true;
    
    // Per-Monitor Settings
    // Key: Monitor Device ID (e.g., "\\.\DISPLAY1"), Value: Settings for that monitor
    public Dictionary<string, MonitorSettings> MonitorSettings { get; set; } = new Dictionary<string, MonitorSettings>();

    // Legacy/Fallback Settings (used if no specific monitor setting found)
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
    
    // Application Settings
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
}

public class MonitorSettings
{
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
}