namespace DisplayShadersPowerToy.Models;

public class DisplaySettings
{
    // Shader Injection (Hooking) Settings
    public bool EnableShaderInjection { get; set; } = true;
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
    
    // ClearType (Registry) Settings
    public bool EnableClearType { get; set; } = true;
    public SubpixelLayout ClearTypeLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ClearTypeIntensity { get; set; } = 1.0;
    
    // Legacy compatibility
    [Obsolete("Use EnableShaderInjection or EnableClearType instead")]
    public bool EnableShader
    {
        get => EnableShaderInjection || EnableClearType;
        set
        {
            EnableShaderInjection = value;
            EnableClearType = value;
        }
    }
    
    // Application Settings
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
}