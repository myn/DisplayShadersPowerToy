using System.Runtime.InteropServices;
using Microsoft.Win32;
using DisplayShadersPowerToy.Models;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Service for managing ClearType settings AND actual display shaders
/// 
/// DUAL MODE OPERATION:
/// - ClearType Mode: Adjusts Windows ClearType registry settings (user-selectable)
/// - Shader Mode: Uses real DirectWrite/D3D hooks with custom shaders (user-selectable)
/// 
/// Both modes can be enabled/disabled independently by the user.
/// The shader mode requires DisplayShaderHook.dll to be present and injected into target processes.
/// 
/// See docs/TECHNICAL_LIMITATIONS.md and docs/DEVELOPER.md for details.
/// </summary>
public class DisplayShaderService : IDisposable
{
    private const string ClearTypeRegistryPath = @"Control Panel\Desktop";
    private const string FontSmoothingKey = "FontSmoothing";
    private const string FontSmoothingTypeKey = "FontSmoothingType";
    private const string FontSmoothingOrientationKey = "FontSmoothingOrientation";
    private const string FontSmoothingGammaKey = "FontSmoothingGamma";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfoW")]
    private static extern bool SystemParametersInfoGet(uint uiAction, uint uiParam, ref IntPtr pvParam, uint fWinIni);

    private const uint SPI_GETFONTSMOOTHING = 0x004A;
    private const uint SPI_SETFONTSMOOTHING = 0x004B;
    private const uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;
    private const uint SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;
    private const uint SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;

    private readonly ShaderService? _shaderService;
    private bool _shaderModeAvailable;
    private InjectionManager? _injectionManager;

    public DisplayShaderService()
    {
        System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Initializing...");
        
        _shaderModeAvailable = ShaderService.IsHookDllAvailable();
        
        System.Diagnostics.Debug.WriteLine($"[DisplayShaderService] Shader mode available: {_shaderModeAvailable}");

        if (_shaderModeAvailable)
        {
            _shaderService = new ShaderService();
            
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Attempting to initialize ShaderService...");
            
            if (!_shaderService.Initialize())
            {
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService] ShaderService.Initialize() failed");
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService] This usually means:");
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService]   - Failed to create shared memory (might need admin rights)");
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService]   - Failed to create event object");
                
                _shaderModeAvailable = false;
                _shaderService?.Dispose();
                _shaderService = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService] ShaderService.Initialize() succeeded");
                
                // Initialize injection manager
                _injectionManager = new InjectionManager();
                System.Diagnostics.Debug.WriteLine("[DisplayShaderService] InjectionManager created");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Hook DLL not available - shader injection will not be available");
        }
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        _injectionManager?.Dispose();
        _shaderService?.Dispose();
    }

    /// <summary>
    /// Check if real shader mode is available
    /// </summary>
    public bool IsShaderModeAvailable() => _shaderModeAvailable;

    /// <summary>
    /// Get shader mode status description
    /// </summary>
    public string GetShaderModeStatus()
    {
        if (!_shaderModeAvailable)
        {
            return "Shader Mode: Not Available (ClearType can still be used)";
        }

        int version = ShaderService.GetHookVersion();
        int injectedCount = _injectionManager?.GetInjectedProcessCount() ?? 0;
        
        if (injectedCount > 0)
        {
            return $"Shader Mode: Active (Hook v{version}, {injectedCount} processes)";
        }
        
        return $"Shader Mode: Ready (Hook v{version}, not injecting)";
    }

    /// <summary>
    /// Enable shader injection into whitelisted processes
    /// </summary>
    public int EnableShaderInjection()
    {
        if (!_shaderModeAvailable || _injectionManager == null)
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Cannot enable injection - shader mode not available");
            return 0;
        }

        System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Enabling shader injection...");
        int count = _injectionManager.InjectIntoProcesses();
        System.Diagnostics.Debug.WriteLine($"[DisplayShaderService] Injected into {count} processes");
        return count;
    }

    /// <summary>
    /// Get number of processes with shader injection active
    /// </summary>
    public int GetInjectedProcessCount()
    {
        return _injectionManager?.GetInjectedProcessCount() ?? 0;
    }

    /// <summary>
    /// Get list of injected process names
    /// </summary>
    public List<string> GetInjectedProcessNames()
    {
        return _injectionManager?.GetInjectedProcessNames() ?? new List<string>();
    }

    /// <summary>
    /// Apply display shader settings based on subpixel layout
    /// Uses real shaders if available, also applies ClearType settings if enabled
    /// </summary>
    public void ApplyShaderSettings(DisplaySettings settings)
    {
        System.Diagnostics.Debug.WriteLine($"[DisplayShaderService] ApplyShaderSettings called");
        System.Diagnostics.Debug.WriteLine($"  - Shader Injection: {settings.EnableShaderInjection}");
        System.Diagnostics.Debug.WriteLine($"  - ClearType: {settings.EnableClearType}");
        
        // Apply shader injection if enabled and available
        if (settings.EnableShaderInjection && _shaderModeAvailable && _shaderService != null && _injectionManager != null)
        {
            _injectionManager.UpdateIgnoredProcesses(settings.IgnoredProcessNames);
            ApplyRealShaderSettings(settings);
        }
        else if (_injectionManager != null)
        {
            _injectionManager.UpdateIgnoredProcesses(settings.IgnoredProcessNames);
            // Update config to disabled state first so any currently hooked apps stop processing
            if (_shaderService != null)
            {
                _shaderService.UpdateShaderConfig(settings);
            }

            // Stop monitoring if shader injection disabled
            _injectionManager.StopContinuousMonitoring();
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Shader injection disabled, stopped monitoring");
        }
        
        // Apply ClearType settings if enabled
        if (settings.EnableClearType)
        {
            ApplyLegacyClearTypeSettings(settings);
        }
        else
        {
            // Disable ClearType if not enabled
            DisableClearType();
        }
    }

    /// <summary>
    /// Apply settings using real DirectWrite/D3D shaders (the proper way)
    /// </summary>
    private void ApplyRealShaderSettings(DisplaySettings settings)
    {
        if (_shaderService == null || _injectionManager == null) return;

        System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Applying REAL shader settings");
        
        _shaderService.UpdateShaderConfig(settings);
        
        // Start continuous monitoring - this will inject into ALL current and future GUI processes
        if (!_injectionManager.IsMonitoring)
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Starting continuous monitoring for ALL GUI processes...");
            _injectionManager.StartContinuousMonitoring();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Continuous monitoring already active");
        }
    }

    /// <summary>
    /// Apply ClearType registry settings (independent option, not a fallback)
    /// </summary>
    private void ApplyLegacyClearTypeSettings(DisplaySettings settings)
    {
        System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Applying ClearType settings");
        
        // Sync ClearType layout with shader layout if not explicitly set
        var clearTypeLayout = settings.ClearTypeLayout;
        var intensity = settings.ClearTypeIntensity;
        
        // If shader layout is set but ClearType layout follows default, sync them
        if (clearTypeLayout == SubpixelLayout.RgbStripe && settings.ShaderLayout != SubpixelLayout.RgbStripe)
        {
            clearTypeLayout = settings.ShaderLayout;
            intensity = settings.ShaderIntensity;
        }
        
        switch (clearTypeLayout)
        {
            case SubpixelLayout.RgbStripe:
                ApplyRgbStripeSettings(intensity);
                break;
            case SubpixelLayout.WrgbStripe:
                ApplyWrgbStripeSettings(intensity);
                break;
            case SubpixelLayout.RgbTriangular:
                ApplyRgbTriangularSettings(intensity);
                break;
            case SubpixelLayout.Pentile:
                ApplyPentileSettings(intensity);
                break;
            case SubpixelLayout.None:
                DisableClearType();
                break;
        }
        
        NotifySystemOfChanges();
    }

    /// <summary>
    /// Apply standard RGB stripe settings (most LCD/LED monitors)
    /// </summary>
    private void ApplyRgbStripeSettings(double intensity)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB orientation
        SetClearTypeContrast((uint)(1400 * intensity)); // Standard contrast
        
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            // Remove custom gamma for standard displays
            try
            {
                key.DeleteValue(FontSmoothingGammaKey, false);
            }
            catch
            {
                // Value might not exist, which is fine
            }
        }
    }

    /// <summary>
    /// Apply settings for WOLED displays (LG WRGB stripe)
    /// 
    /// NOTE: This is a WORKAROUND, not a proper fix.
    /// Windows ClearType only supports horizontal RGB or BGR layouts.
    /// WOLED has WRGB (White-Red-Green-Blue) which doesn't map perfectly.
    /// We reduce contrast to minimize color fringing on the white subpixel.
    /// </summary>
    private void ApplyWrgbStripeSettings(double intensity)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // Still RGB orientation (limitation: no RBG mode exists)
        SetClearTypeContrast((uint)(800 * intensity)); // Lower contrast to reduce color fringing
        
        // Additional registry settings for WOLED
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1200 * intensity), RegistryValueKind.DWord);
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
    private void ApplyRgbTriangularSettings(double intensity)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB orientation
        // For triangular subpixels, we need even more conservative settings
        SetClearTypeContrast((uint)(600 * intensity));
        
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1000 * intensity), RegistryValueKind.DWord);
        }
    }

    /// <summary>
    /// Apply settings for PenTile displays
    /// </summary>
    private void ApplyPentileSettings(double intensity)
    {
        SetClearTypeEnabled(true);
        SetClearTypeOrientation(1); // RGB
        // PenTile benefits from reduced ClearType
        SetClearTypeContrast((uint)(700 * intensity));
        
        using var key = Registry.CurrentUser.CreateSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            key.SetValue(FontSmoothingGammaKey, (int)(1100 * intensity), RegistryValueKind.DWord);
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
    /// Notify system to refresh display settings without changing the current font smoothing state.
    /// Uses SPI_GETFONTSMOOTHING to query then re-set the current value with SPIF_SENDCHANGE.
    /// </summary>
    private void NotifySystemOfChanges()
    {
        // Query current font smoothing state
        IntPtr currentState = IntPtr.Zero;
        SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref currentState, 0);
        
        // Re-apply current state with SENDCHANGE flag to notify other applications
        uint enabled = currentState != IntPtr.Zero ? 1u : 0u;
        SystemParametersInfo(SPI_SETFONTSMOOTHING, enabled, IntPtr.Zero, SPIF_SENDCHANGE);
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
    /// Get current ClearType AND shader settings
    /// </summary>
    public DisplaySettings GetCurrentSettings()
    {
        var settings = new DisplaySettings();

        // Read ClearType settings from registry
        using var key = Registry.CurrentUser.OpenSubKey(ClearTypeRegistryPath);
        if (key != null)
        {
            var fontSmoothing = key.GetValue(FontSmoothingKey)?.ToString();
            var fontSmoothingType = key.GetValue(FontSmoothingTypeKey);

            bool clearTypeEnabled = fontSmoothing == "2" && fontSmoothingType?.ToString() == "2";
            
            settings.EnableClearType = clearTypeEnabled;
            
            // Try to detect current layout based on settings
            var contrast = key.GetValue(FontSmoothingGammaKey);
            if (contrast != null)
            {
                int contrastValue = Convert.ToInt32(contrast);
                SubpixelLayout detectedLayout;
                
                if (contrastValue <= 600)
                    detectedLayout = SubpixelLayout.RgbTriangular;
                else if (contrastValue <= 800)
                    detectedLayout = SubpixelLayout.WrgbStripe;
                else if (contrastValue <= 1000)
                    detectedLayout = SubpixelLayout.Pentile;
                else
                    detectedLayout = SubpixelLayout.RgbStripe;
                
                settings.ClearTypeLayout = detectedLayout;
            }
            else
            {
                settings.ClearTypeLayout = SubpixelLayout.RgbStripe;
            }
        }
        
        // Read shader settings from shared memory (if available)
        if (_shaderModeAvailable && _shaderService != null)
        {
            var shaderSettings = _shaderService.ReadCurrentConfig();
            settings.EnableShaderInjection = shaderSettings.EnableShaderInjection;
            settings.ShaderLayout = shaderSettings.ShaderLayout;
            settings.ShaderIntensity = shaderSettings.ShaderIntensity;
        }
        else
        {
            // Shader mode not available, use defaults
            settings.EnableShaderInjection = false;
            settings.ShaderLayout = SubpixelLayout.RgbStripe;
            settings.ShaderIntensity = 1.0;
        }

        return settings;
    }

    /// <summary>
    /// DEVELOPER/CLEANUP ONLY: Force eject DLLs from all processes
    /// WARNING: Will likely cause crashes in hooked applications!
    /// Only use when you need to rebuild/delete DLL files during development.
    /// </summary>
    public void ForceEjectAllDlls()
    {
        if (!_shaderModeAvailable || _injectionManager == null)
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Cannot force-eject - no injection manager");
            return;
        }

        System.Diagnostics.Debug.WriteLine("[DisplayShaderService] ⚠ WARNING: Force-ejecting all DLLs");
        _injectionManager.ForceEjectAllDlls();
    }
}
