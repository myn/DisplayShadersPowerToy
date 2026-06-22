using System.Collections.Generic;

namespace DisplayShadersPowerToy.Models;

public class DisplaySettings
{
    // Global Settings
    public bool EnableShaderInjection { get; set; } = true;
    
    // ClearType (Registry) Settings - Optional fallback/complement to shader injection
    public bool EnableClearType { get; set; } = true;
    public SubpixelLayout ClearTypeLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ClearTypeIntensity { get; set; } = 1.0;
    
    // Per-Monitor Settings
    // Key: Monitor Device ID (e.g., "\\.\DISPLAY1"), Value: Settings for that monitor
    public Dictionary<string, MonitorSettings> MonitorSettings { get; set; } = new Dictionary<string, MonitorSettings>();

    // Legacy/Fallback Settings (used if no specific monitor setting found)
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
    
    // Application Settings
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;

    // Process names to skip during DLL injection. Names are compared without ".exe".
    public List<string> IgnoredProcessNames { get; set; } = new List<string>
    {
        "java",
        "javaw",
        "RuneLite",
        "JagexLauncher"
    };
}

public class MonitorSettings
{
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
}
