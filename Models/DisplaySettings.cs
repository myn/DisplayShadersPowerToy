namespace DisplayShadersPowerToy.Models;

public class DisplaySettings
{
    public SubpixelLayout SubpixelLayout { get; set; } = SubpixelLayout.RgbStripe;
    public bool EnableShader { get; set; } = true;
    public double ShaderIntensity { get; set; } = 1.0;
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
}