using System.Runtime.InteropServices;
using Microsoft.Win32;
using DisplayShadersPowerToy.Models;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Service for managing ClearType settings
/// 
/// IMPORTANT: This service does NOT implement actual display shaders.
/// It only modifies Windows ClearType registry settings to provide
/// workarounds for OLED displays. Due to Windows API limitations:
/// 
/// - Cannot implement RBG orientation for WOLED (only RGB/BGR supported)
/// - Cannot fix vertical fringing on QD-OLED (ClearType is horizontal-only)
/// - Cannot use custom subpixel masks
/// - Only adjusts contrast/gamma values
/// 
/// See docs/TECHNICAL_LIMITATIONS.md for details.
/// </summary>
public class DisplayShaderService
{
    private const string ClearTypeRegistryPath = @"Control Panel\Desktop";
    private const string FontSmoothingKey = "FontSmoothing";
    private const string FontSmoothingTypeKey = "FontSmoothingType";
    private const string FontSmoothingOrientationKey = "FontSmoothingOrientation";
    private const string FontSmoothingGammaKey = "FontSmoothingGamma";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    private const uint SPI_SETFONTSMOOTHING = 0x004B;
    private const uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;
    private const uint SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;
    private const uint SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;

    /// <summary>
    /// Apply display shader settings based on subpixel layout
    /// </summary>
    public void ApplyShaderSettings(DisplaySettings settings)
    {
        if (!settings.EnableShader)
        {
            // Disable ClearType completely
            DisableClearType();
            return;
        }

        switch (settings.SubpixelLayout)
        {
            case SubpixelLayout.RgbStripe:
                ApplyRgbStripeSettings(settings);
                break;
            case SubpixelLayout.WrgbStripe:
                ApplyWrgbStripeSettings(settings);
                break;
            case SubpixelLayout.RgbTriangular:
                ApplyRgbTriangularSettings(settings);
                break;
            case SubpixelLayout.Pentile:
                ApplyPentileSettings(settings);
                break;
            case SubpixelLayout.None:
                DisableClearType();
                break;
        }

        // Notify system of changes
        NotifySystemOfChanges();
    }

    /// <summary>
    /// Apply settings for standard RGB stripe displays (default ClearType)
    /// </summary>
    private void ApplyRgbStripeSettings(DisplaySettings settings)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB
        SetClearTypeContrast((uint)(1400 * settings.ShaderIntensity));
    }

    /// <summary>
    /// Apply settings for WOLED WRGB stripe displays
    /// 
    /// LIMITATION: This is a workaround, not a real fix. Windows ClearType
    /// does not support RBG orientation (where Blue is in the middle).
    /// We can only reduce contrast to minimize color fringing.
    /// A proper fix would require actual display shaders.
    /// </summary>
    private void ApplyWrgbStripeSettings(DisplaySettings settings)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // Still RGB orientation (limitation: no RBG mode exists)
        SetClearTypeContrast((uint)(800 * settings.ShaderIntensity)); // Lower contrast to reduce color fringing
        
        // Additional registry settings for WOLED
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1200 * settings.ShaderIntensity), RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Apply settings for QD-OLED RGB triangular displays
    /// 
    /// LIMITATION: This cannot fix the vertical green/purple fringing problem.
    /// Windows ClearType only handles horizontal subpixel arrangements.
    /// Triangular layouts with green on top and red/blue on bottom require
    /// actual display shaders to fix properly.
    /// </summary>
    private void ApplyRgbTriangularSettings(DisplaySettings settings)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB orientation
        // For triangular subpixels, we need even more conservative settings
        SetClearTypeContrast((uint)(600 * settings.ShaderIntensity));
        
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1000 * settings.ShaderIntensity), RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Apply settings for PenTile displays
    /// </summary>
    private void ApplyPentileSettings(DisplaySettings settings)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB
        // PenTile benefits from reduced ClearType
        SetClearTypeContrast((uint)(700 * settings.ShaderIntensity));
        
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1100 * settings.ShaderIntensity), RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Disable ClearType completely
    /// </summary>
    private void DisableClearType()
    {
        SetClearTypeEnabled(false);
    }

    /// <summary>
    /// Enable or disable ClearType
    /// </summary>
    private void SetClearTypeEnabled(bool enabled)
    {
        uint value = enabled ? 2u : 0u; // 2 = ClearType, 0 = Off
        SystemParametersInfo(SPI_SETFONTSMOOTHING, enabled ? 1u : 0u, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        SystemParametersInfo(SPI_SETFONTSMOOTHINGTYPE, 0, (IntPtr)value, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingKey, enabled ? "2" : "0", RegistryValueKind.String);
            key.SetValue(FontSmoothingTypeKey, value, RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Set ClearType orientation (1 = RGB, 0 = BGR)
    /// </summary>
    private void SetClearTypeOrientation(uint orientation)
    {
        SystemParametersInfo(SPI_SETFONTSMOOTHINGORIENTATION, 0, (IntPtr)orientation, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingOrientationKey, orientation, RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Set ClearType contrast level
    /// </summary>
    private void SetClearTypeContrast(uint contrast)
    {
        SystemParametersInfo(SPI_SETFONTSMOOTHINGCONTRAST, 0, (IntPtr)contrast, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }

    /// <summary>
    /// Notify system to refresh display settings
    /// </summary>
    private void NotifySystemOfChanges()
    {
        SystemParametersInfo(SPI_SETFONTSMOOTHING, 0, IntPtr.Zero, SPIF_SENDCHANGE);
    }

    /// <summary>
    /// Restore Windows default ClearType settings (OEM defaults)
    /// </summary>
    public void RestoreWindowsDefaults()
    {
        // Windows 10/11 default ClearType settings:
        // - ClearType enabled (FontSmoothing = 2)
        // - RGB orientation (FE_FONTSMOOTHINGORIENTATION = 1)
        // - Standard contrast (1400)
        // - No custom gamma setting
        
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB
        SetClearTypeContrast(1400); // Windows default contrast
        
        // Remove custom gamma setting if it exists
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            try
            {
                key.DeleteValue(FontSmoothingGammaKey, false);
            }
            catch
            {
                // Value might not exist, which is fine
            }
        }
        
        // Notify system of changes
        NotifySystemOfChanges();
    }

    /// <summary>
    /// Get current ClearType settings
    /// </summary>
    public DisplaySettings GetCurrentSettings()
    {
        var settings = new DisplaySettings();

        using var key = Registry.CurrentUser.OpenSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            var fontSmoothing = key.GetValue(FontSmoothingKey)?.ToString();
            var fontSmoothingType = key.GetValue(FontSmoothingTypeKey);

            settings.EnableShader = fontSmoothing == "2" && fontSmoothingType?.ToString() == "2";
            
            // Try to detect current layout based on settings
            var contrast = key.GetValue(FontSmoothingGammaKey);
            if (contrast != null)
            {
                int contrastValue = Convert.ToInt32(contrast);
                if (contrastValue <= 600)
                    settings.SubpixelLayout = SubpixelLayout.RgbTriangular;
                else if (contrastValue <= 800)
                    settings.SubpixelLayout = SubpixelLayout.WrgbStripe;
                else if (contrastValue <= 1000)
                    settings.SubpixelLayout = SubpixelLayout.Pentile;
                else
                    settings.SubpixelLayout = SubpixelLayout.RgbStripe;
            }
        }

        return settings;
    }
}
