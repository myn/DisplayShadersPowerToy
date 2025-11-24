namespace DisplayShadersPowerToy.Models;

public class DisplaySettings
{
    // Shader Injection Settings
    public bool EnableShaderInjection { get; set; } = true;
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
    
    // Application Settings
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
}