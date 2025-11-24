using DisplayShadersPowerToy.Models;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Service for managing display shader injection
/// Uses real DirectWrite/D3D hooks with custom HLSL shaders
/// </summary>
public class DisplayShaderService : IDisposable
{
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
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Hook DLL not available");
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
            return "Shader Mode: Not Available (DisplayShaderHook.dll not found)";
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
    /// </summary>
    public void ApplyShaderSettings(DisplaySettings settings)
    {
        System.Diagnostics.Debug.WriteLine($"[DisplayShaderService] ApplyShaderSettings called");
        System.Diagnostics.Debug.WriteLine($"  - Shader Injection: {settings.EnableShaderInjection}");
        
        if (!_shaderModeAvailable || _shaderService == null || _injectionManager == null)
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Shader mode not available");
            return;
        }

        if (settings.EnableShaderInjection)
        {
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Applying shader settings");
            
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
        else
        {
            // Update config to disabled state first so any currently hooked apps stop processing
            _shaderService.UpdateShaderConfig(settings);

            // Stop monitoring if shader injection disabled
            _injectionManager.StopContinuousMonitoring();
            System.Diagnostics.Debug.WriteLine("[DisplayShaderService] Shader injection disabled, stopped monitoring");
        }
    }

    /// <summary>
    /// Get current shader settings
    /// </summary>
    public DisplaySettings GetCurrentSettings()
    {
        var settings = new DisplaySettings();
        
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
}
