using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Tests for verifying ClearType settings are properly applied and persist without being reverted.
/// These tests validate the fix for the bug where enabling ClearType would apply momentarily
/// then be disabled by the NotifySystemOfChanges method.
/// </summary>
[Collection("RegistryTests")]
public class ClearTypeStatePersistenceTests
{
    private const string ClearTypeRegistryPath = @"Control Panel\Desktop";
    private const string FontSmoothingKey = "FontSmoothing";
    private const string FontSmoothingTypeKey = "FontSmoothingType";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfoW")]
    private static extern bool SystemParametersInfoGet(uint uiAction, uint uiParam, ref IntPtr pvParam, uint fWinIni);

    private const uint SPI_GETFONTSMOOTHING = 0x004A;
    private const uint SPI_SETFONTSMOOTHING = 0x004B;

    #region ClearType Toggle Persistence Tests

    /// <summary>
    /// Tests that enabling ClearType via settings results in ClearType being enabled in Windows
    /// </summary>
    [Fact]
    public void EnableClearType_ViaSettings_ClearTypeIsEnabled()
    {
        // Arrange
        var service = new DisplayShaderService();
        var settings = new DisplaySettings
        {
            EnableClearType = true,
            EnableShaderInjection = false, // Disable shader to isolate ClearType test
            ClearTypeLayout = SubpixelLayout.RgbStripe,
            ClearTypeIntensity = 1.0
        };

        try
        {
            // Act - Apply settings with ClearType enabled
            service.ApplyShaderSettings(settings);

            // Assert - Verify ClearType is actually enabled via Windows API
            IntPtr fontSmoothingEnabled = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref fontSmoothingEnabled, 0);
            
            Assert.NotEqual(IntPtr.Zero, fontSmoothingEnabled);
        }
        finally
        {
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that disabling ClearType via settings results in ClearType being disabled in Windows
    /// </summary>
    [Fact]
    public void DisableClearType_ViaSettings_ClearTypeIsDisabled()
    {
        // Arrange
        var service = new DisplayShaderService();
        var settings = new DisplaySettings
        {
            EnableClearType = false,
            EnableShaderInjection = false,
            ClearTypeLayout = SubpixelLayout.RgbStripe,
            ClearTypeIntensity = 1.0
        };

        try
        {
            // Act - Apply settings with ClearType disabled
            service.ApplyShaderSettings(settings);

            // Assert - Verify ClearType is actually disabled via Windows API
            IntPtr fontSmoothingEnabled = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref fontSmoothingEnabled, 0);
            
            Assert.Equal(IntPtr.Zero, fontSmoothingEnabled);
        }
        finally
        {
            // Restore ClearType for other tests
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that ClearType state persists after multiple enable/disable cycles
    /// This validates the fix for the bug where ClearType was being disabled after being enabled
    /// </summary>
    [Fact]
    public void ClearType_EnableDisableCycles_StatePersiststCorrectly()
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            // Test enable -> disable -> enable cycle
            for (int i = 0; i < 3; i++)
            {
                // Enable ClearType
                var enableSettings = new DisplaySettings
                {
                    EnableClearType = true,
                    EnableShaderInjection = false,
                    ClearTypeLayout = SubpixelLayout.RgbStripe,
                    ClearTypeIntensity = 1.0
                };
                service.ApplyShaderSettings(enableSettings);

                // Verify ClearType is enabled
                IntPtr enabledState = IntPtr.Zero;
                SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref enabledState, 0);
                Assert.NotEqual(IntPtr.Zero, enabledState);

                // Disable ClearType
                var disableSettings = new DisplaySettings
                {
                    EnableClearType = false,
                    EnableShaderInjection = false,
                    ClearTypeLayout = SubpixelLayout.RgbStripe,
                    ClearTypeIntensity = 1.0
                };
                service.ApplyShaderSettings(disableSettings);

                // Verify ClearType is disabled
                IntPtr disabledState = IntPtr.Zero;
                SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref disabledState, 0);
                Assert.Equal(IntPtr.Zero, disabledState);
            }
        }
        finally
        {
            // Restore ClearType
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that enabling ClearType doesn't get immediately reverted
    /// This specifically tests the bug where NotifySystemOfChanges was disabling ClearType
    /// </summary>
    [Fact]
    public void EnableClearType_DoesNotGetImmediatelyReverted()
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            // First disable ClearType to establish baseline
            var disableSettings = new DisplaySettings
            {
                EnableClearType = false,
                EnableShaderInjection = false
            };
            service.ApplyShaderSettings(disableSettings);

            // Verify disabled
            IntPtr disabledState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref disabledState, 0);
            Assert.Equal(IntPtr.Zero, disabledState);

            // Now enable ClearType
            var enableSettings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = false,
                ClearTypeLayout = SubpixelLayout.WrgbStripe,
                ClearTypeIntensity = 0.85
            };
            service.ApplyShaderSettings(enableSettings);

            // Immediately verify ClearType is still enabled (not reverted)
            IntPtr enabledState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref enabledState, 0);
            
            // This was the bug - ClearType would be enabled then immediately disabled
            Assert.NotEqual(IntPtr.Zero, enabledState);

            // Verify again after a brief delay to ensure state persists
            Thread.Sleep(100);
            IntPtr persistedState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref persistedState, 0);
            Assert.NotEqual(IntPtr.Zero, persistedState);
        }
        finally
        {
            // Restore ClearType
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            service.Dispose();
        }
    }

    #endregion

    #region ClearType and Shader Injection Independence Tests

    /// <summary>
    /// Tests that enabling shader injection doesn't affect ClearType state
    /// </summary>
    [Fact]
    public void EnableShaderInjection_DoesNotAffectClearTypeState()
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            // First enable ClearType
            var clearTypeOnlySettings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = false,
                ClearTypeLayout = SubpixelLayout.RgbStripe,
                ClearTypeIntensity = 1.0
            };
            service.ApplyShaderSettings(clearTypeOnlySettings);

            // Verify ClearType is enabled
            IntPtr initialState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref initialState, 0);
            Assert.NotEqual(IntPtr.Zero, initialState);

            // Now enable shader injection as well
            var bothEnabledSettings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = true,
                ClearTypeLayout = SubpixelLayout.RgbStripe,
                ClearTypeIntensity = 1.0,
                ShaderLayout = SubpixelLayout.RgbStripe,
                ShaderIntensity = 1.0
            };
            service.ApplyShaderSettings(bothEnabledSettings);

            // Verify ClearType is still enabled
            IntPtr afterShaderState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref afterShaderState, 0);
            Assert.NotEqual(IntPtr.Zero, afterShaderState);
        }
        finally
        {
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that disabling shader injection doesn't affect ClearType state
    /// </summary>
    [Fact]
    public void DisableShaderInjection_DoesNotAffectClearTypeState()
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            // Start with both enabled
            var bothEnabledSettings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = true,
                ClearTypeLayout = SubpixelLayout.WrgbStripe,
                ClearTypeIntensity = 0.9
            };
            service.ApplyShaderSettings(bothEnabledSettings);

            // Verify ClearType is enabled
            IntPtr initialState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref initialState, 0);
            Assert.NotEqual(IntPtr.Zero, initialState);

            // Now disable shader injection only
            var clearTypeOnlySettings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = false,
                ClearTypeLayout = SubpixelLayout.WrgbStripe,
                ClearTypeIntensity = 0.9
            };
            service.ApplyShaderSettings(clearTypeOnlySettings);

            // Verify ClearType is still enabled
            IntPtr afterDisableState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref afterDisableState, 0);
            Assert.NotEqual(IntPtr.Zero, afterDisableState);
        }
        finally
        {
            service.Dispose();
        }
    }

    #endregion

    #region ClearType Layout Settings Tests

    /// <summary>
    /// Tests that all ClearType layouts can be applied without reverting
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe)]
    [InlineData(SubpixelLayout.WrgbStripe)]
    [InlineData(SubpixelLayout.RgbTriangular)]
    [InlineData(SubpixelLayout.Pentile)]
    public void ApplyClearTypeLayout_StateDoesNotRevert(SubpixelLayout layout)
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            var settings = new DisplaySettings
            {
                EnableClearType = true,
                EnableShaderInjection = false,
                ClearTypeLayout = layout,
                ClearTypeIntensity = 0.85
            };

            // Act
            service.ApplyShaderSettings(settings);

            // Assert - ClearType should be enabled (not reverted)
            IntPtr state = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref state, 0);
            Assert.NotEqual(IntPtr.Zero, state);
        }
        finally
        {
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that changing ClearType layout preserves enabled state
    /// </summary>
    [Fact]
    public void ChangeClearTypeLayout_PreservesEnabledState()
    {
        // Arrange
        var service = new DisplayShaderService();
        var layouts = new[] { SubpixelLayout.RgbStripe, SubpixelLayout.WrgbStripe, 
                              SubpixelLayout.RgbTriangular, SubpixelLayout.Pentile };

        try
        {
            foreach (var layout in layouts)
            {
                var settings = new DisplaySettings
                {
                    EnableClearType = true,
                    EnableShaderInjection = false,
                    ClearTypeLayout = layout,
                    ClearTypeIntensity = 1.0
                };

                // Act
                service.ApplyShaderSettings(settings);

                // Assert - ClearType should still be enabled after each layout change
                IntPtr state = IntPtr.Zero;
                SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref state, 0);
                Assert.NotEqual(IntPtr.Zero, state);
            }
        }
        finally
        {
            service.Dispose();
        }
    }

    #endregion
}

/// <summary>
/// Tests for verifying Start With Windows registry persistence
/// </summary>
[Collection("RegistryTests")]
public class StartWithWindowsPersistenceTests
{
    private const string StartupRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "DisplayShadersPowerToy";

    /// <summary>
    /// Tests that enabling Start With Windows creates the registry entry
    /// </summary>
    [Fact]
    public void EnableStartWithWindows_CreatesRegistryEntry()
    {
        // Arrange
        var settingsService = new SettingsService();

        try
        {
            // First ensure it's disabled
            settingsService.SetStartWithWindows(false);
            Assert.False(settingsService.IsStartWithWindowsEnabled());

            // Act - enable start with windows
            settingsService.SetStartWithWindows(true);

            // Assert - registry entry should exist
            Assert.True(settingsService.IsStartWithWindowsEnabled());

            // Verify by reading registry directly
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath);
            var value = key?.GetValue(AppName);
            Assert.NotNull(value);
        }
        finally
        {
            // Cleanup
            settingsService.SetStartWithWindows(false);
        }
    }

    /// <summary>
    /// Tests that disabling Start With Windows removes the registry entry
    /// </summary>
    [Fact]
    public void DisableStartWithWindows_RemovesRegistryEntry()
    {
        // Arrange
        var settingsService = new SettingsService();

        try
        {
            // First enable it
            settingsService.SetStartWithWindows(true);
            Assert.True(settingsService.IsStartWithWindowsEnabled());

            // Act - disable start with windows
            settingsService.SetStartWithWindows(false);

            // Assert - registry entry should not exist
            Assert.False(settingsService.IsStartWithWindowsEnabled());

            // Verify by reading registry directly
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath);
            var value = key?.GetValue(AppName);
            Assert.Null(value);
        }
        finally
        {
            // Ensure cleanup
            settingsService.SetStartWithWindows(false);
        }
    }

    /// <summary>
    /// Tests that multiple enable/disable cycles work correctly
    /// </summary>
    [Fact]
    public void StartWithWindows_EnableDisableCycles_WorkCorrectly()
    {
        // Arrange
        var settingsService = new SettingsService();

        try
        {
            for (int i = 0; i < 3; i++)
            {
                // Enable
                settingsService.SetStartWithWindows(true);
                Assert.True(settingsService.IsStartWithWindowsEnabled());

                // Disable
                settingsService.SetStartWithWindows(false);
                Assert.False(settingsService.IsStartWithWindowsEnabled());
            }
        }
        finally
        {
            settingsService.SetStartWithWindows(false);
        }
    }

    /// <summary>
    /// Tests that saved settings and actual registry state remain synchronized
    /// </summary>
    [Fact]
    public void SavedSettings_AndRegistryState_AreInSync()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        try
        {
            // Test sync when enabling
            var settings = new DisplaySettings { StartWithWindows = true };
            settingsService.SaveSettings(settings);
            settingsService.SetStartWithWindows(true);

            var loaded = settingsService.LoadSettings();
            bool registryState = settingsService.IsStartWithWindowsEnabled();

            Assert.True(loaded.StartWithWindows);
            Assert.True(registryState);
            Assert.Equal(loaded.StartWithWindows, registryState);

            // Test sync when disabling
            settings.StartWithWindows = false;
            settingsService.SaveSettings(settings);
            settingsService.SetStartWithWindows(false);

            loaded = settingsService.LoadSettings();
            registryState = settingsService.IsStartWithWindowsEnabled();

            Assert.False(loaded.StartWithWindows);
            Assert.False(registryState);
            Assert.Equal(loaded.StartWithWindows, registryState);
        }
        finally
        {
            settingsService.SetStartWithWindows(false);
            settingsService.ClearSettings();
        }
    }
}

/// <summary>
/// Tests for verifying settings round-trip persistence (save and reload)
/// </summary>
[Collection("RegistryTests")]
public class SettingsRoundTripTests
{
    /// <summary>
    /// Tests that all toggle settings survive a save/load cycle
    /// </summary>
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, false, false)]
    public void AllToggleSettings_SurviveSaveLoadCycle(
        bool shaderEnabled, bool clearTypeEnabled, 
        bool startWithWindows, bool minimizeToTray)
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        try
        {
            var original = new DisplaySettings
            {
                EnableShaderInjection = shaderEnabled,
                EnableClearType = clearTypeEnabled,
                StartWithWindows = startWithWindows,
                MinimizeToTray = minimizeToTray
            };

            // Act
            settingsService.SaveSettings(original);
            var loaded = settingsService.LoadSettings();

            // Assert
            Assert.Equal(shaderEnabled, loaded.EnableShaderInjection);
            Assert.Equal(clearTypeEnabled, loaded.EnableClearType);
            Assert.Equal(startWithWindows, loaded.StartWithWindows);
            Assert.Equal(minimizeToTray, loaded.MinimizeToTray);
        }
        finally
        {
            settingsService.ClearSettings();
        }
    }

    /// <summary>
    /// Tests that layout and intensity settings survive a save/load cycle
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe, 1.0)]
    [InlineData(SubpixelLayout.WrgbStripe, 0.85)]
    [InlineData(SubpixelLayout.RgbTriangular, 0.75)]
    [InlineData(SubpixelLayout.Pentile, 0.7)]
    [InlineData(SubpixelLayout.None, 0.0)]
    public void LayoutAndIntensity_SurviveSaveLoadCycle(SubpixelLayout layout, double intensity)
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        try
        {
            var original = new DisplaySettings
            {
                ShaderLayout = layout,
                ShaderIntensity = intensity,
                ClearTypeLayout = layout,
                ClearTypeIntensity = intensity
            };

            // Act
            settingsService.SaveSettings(original);
            var loaded = settingsService.LoadSettings();

            // Assert
            Assert.Equal(layout, loaded.ShaderLayout);
            Assert.Equal(intensity, loaded.ShaderIntensity);
            Assert.Equal(layout, loaded.ClearTypeLayout);
            Assert.Equal(intensity, loaded.ClearTypeIntensity);
        }
        finally
        {
            settingsService.ClearSettings();
        }
    }

    /// <summary>
    /// Tests that per-monitor settings survive a save/load cycle
    /// </summary>
    [Fact]
    public void PerMonitorSettings_SurviveSaveLoadCycle()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        try
        {
            var original = new DisplaySettings();
            original.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
            {
                ShaderLayout = SubpixelLayout.WrgbStripe,
                ShaderIntensity = 0.9
            };
            original.MonitorSettings[@"\\.\DISPLAY2"] = new MonitorSettings
            {
                ShaderLayout = SubpixelLayout.RgbTriangular,
                ShaderIntensity = 0.8
            };
            original.MonitorSettings[@"\\.\DISPLAY3"] = new MonitorSettings
            {
                ShaderLayout = SubpixelLayout.Pentile,
                ShaderIntensity = 0.7
            };

            // Act
            settingsService.SaveSettings(original);
            var loaded = settingsService.LoadSettings();

            // Assert
            Assert.Equal(3, loaded.MonitorSettings.Count);
            
            Assert.True(loaded.MonitorSettings.ContainsKey(@"\\.\DISPLAY1"));
            Assert.Equal(SubpixelLayout.WrgbStripe, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
            Assert.Equal(0.9, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);

            Assert.True(loaded.MonitorSettings.ContainsKey(@"\\.\DISPLAY2"));
            Assert.Equal(SubpixelLayout.RgbTriangular, loaded.MonitorSettings[@"\\.\DISPLAY2"].ShaderLayout);
            Assert.Equal(0.8, loaded.MonitorSettings[@"\\.\DISPLAY2"].ShaderIntensity);

            Assert.True(loaded.MonitorSettings.ContainsKey(@"\\.\DISPLAY3"));
            Assert.Equal(SubpixelLayout.Pentile, loaded.MonitorSettings[@"\\.\DISPLAY3"].ShaderLayout);
            Assert.Equal(0.7, loaded.MonitorSettings[@"\\.\DISPLAY3"].ShaderIntensity);
        }
        finally
        {
            settingsService.ClearSettings();
        }
    }

    /// <summary>
    /// Tests that multiple sequential saves don't corrupt settings
    /// </summary>
    [Fact]
    public void MultipleSequentialSaves_DoNotCorruptSettings()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        try
        {
            var settings = new DisplaySettings();

            // Perform many sequential saves with different values
            for (int i = 0; i < 20; i++)
            {
                settings.EnableShaderInjection = (i % 2) == 0;
                settings.EnableClearType = (i % 3) == 0;
                settings.ShaderLayout = (SubpixelLayout)(i % 5);
                settings.ShaderIntensity = (i % 11) / 10.0;
                settingsService.SaveSettings(settings);
            }

            // Final values (i = 19)
            settings.EnableShaderInjection = true;  // final state
            settings.EnableClearType = false;       // final state
            settings.ShaderLayout = SubpixelLayout.WrgbStripe;
            settings.ShaderIntensity = 0.85;
            settingsService.SaveSettings(settings);

            // Act
            var loaded = settingsService.LoadSettings();

            // Assert - final saved state should be preserved
            Assert.True(loaded.EnableShaderInjection);
            Assert.False(loaded.EnableClearType);
            Assert.Equal(SubpixelLayout.WrgbStripe, loaded.ShaderLayout);
            Assert.Equal(0.85, loaded.ShaderIntensity);
        }
        finally
        {
            settingsService.ClearSettings();
        }
    }
}

/// <summary>
/// Tests for verifying the Reset to Windows Defaults functionality
/// </summary>
[Collection("RegistryTests")]
public class ResetToDefaultsTests
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfoW")]
    private static extern bool SystemParametersInfoGet(uint uiAction, uint uiParam, ref IntPtr pvParam, uint fWinIni);

    private const uint SPI_GETFONTSMOOTHING = 0x004A;
    private const uint SPI_SETFONTSMOOTHING = 0x004B;

    /// <summary>
    /// Tests that RestoreWindowsDefaults enables ClearType font smoothing
    /// </summary>
    [Fact]
    public void RestoreWindowsDefaults_EnablesClearType()
    {
        // Arrange
        var service = new DisplayShaderService();

        try
        {
            // First disable ClearType to have a non-default state
            var disabledSettings = new DisplaySettings
            {
                EnableClearType = false,
                EnableShaderInjection = false
            };
            service.ApplyShaderSettings(disabledSettings);

            // Verify ClearType is disabled
            IntPtr disabledState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref disabledState, 0);
            Assert.Equal(IntPtr.Zero, disabledState);

            // Act - restore Windows defaults
            service.RestoreWindowsDefaults();

            // Assert - ClearType should be enabled (Windows default)
            IntPtr enabledState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref enabledState, 0);
            Assert.NotEqual(IntPtr.Zero, enabledState);
        }
        finally
        {
            // Ensure ClearType is enabled for other tests
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that ClearSettings removes all saved settings from registry
    /// </summary>
    [Fact]
    public void ClearSettings_RemovesAllSavedSettings()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Save some non-default settings
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.75,
            StartWithWindows = true,
            MinimizeToTray = true
        };
        settings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.8
        };
        settingsService.SaveSettings(settings);

        // Verify settings were saved
        var loadedBefore = settingsService.LoadSettings();
        Assert.True(loadedBefore.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedBefore.ShaderLayout);

        // Act - clear all settings
        settingsService.ClearSettings();

        // Assert - settings should be reset to defaults
        var loadedAfter = settingsService.LoadSettings();
        
        // Default values from DisplaySettings class
        Assert.True(loadedAfter.EnableShaderInjection); // Default is true
        Assert.True(loadedAfter.EnableClearType);       // Default is true
        Assert.Equal(SubpixelLayout.RgbStripe, loadedAfter.ShaderLayout); // Default
        Assert.Equal(1.0, loadedAfter.ShaderIntensity); // Default
        Assert.False(loadedAfter.StartWithWindows);     // Default is false
        Assert.False(loadedAfter.MinimizeToTray);       // Default is false
        Assert.Empty(loadedAfter.MonitorSettings);      // Default is empty
    }

    /// <summary>
    /// Tests that SetStartWithWindows(false) removes the startup registry entry
    /// </summary>
    [Fact]
    public void ResetStartWithWindows_RemovesStartupEntry()
    {
        // Arrange
        var settingsService = new SettingsService();

        try
        {
            // First enable start with windows
            settingsService.SetStartWithWindows(true);
            Assert.True(settingsService.IsStartWithWindowsEnabled());

            // Act - disable (as part of reset)
            settingsService.SetStartWithWindows(false);

            // Assert - startup entry should be removed
            Assert.False(settingsService.IsStartWithWindowsEnabled());
        }
        finally
        {
            // Cleanup
            settingsService.SetStartWithWindows(false);
        }
    }

    /// <summary>
    /// Tests the full reset sequence: modified settings -> reset -> defaults restored
    /// </summary>
    [Fact]
    public void FullResetSequence_RestoresAllDefaults()
    {
        // Arrange
        var service = new DisplayShaderService();
        var settingsService = new SettingsService();

        try
        {
            // Apply non-default settings
            var modifiedSettings = new DisplaySettings
            {
                EnableShaderInjection = true,
                EnableClearType = false,
                ShaderLayout = SubpixelLayout.WrgbStripe,
                ShaderIntensity = 0.7,
                ClearTypeLayout = SubpixelLayout.WrgbStripe,
                ClearTypeIntensity = 0.7,
                StartWithWindows = true,
                MinimizeToTray = true
            };
            modifiedSettings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
            {
                ShaderLayout = SubpixelLayout.RgbTriangular,
                ShaderIntensity = 0.8
            };

            settingsService.SaveSettings(modifiedSettings);
            settingsService.SetStartWithWindows(true);
            service.ApplyShaderSettings(modifiedSettings);

            // Verify non-default state
            Assert.True(settingsService.IsStartWithWindowsEnabled());
            IntPtr clearTypeState = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref clearTypeState, 0);
            // ClearType is disabled in our settings
            Assert.Equal(IntPtr.Zero, clearTypeState);

            // Act - perform full reset sequence (simulating what ResetToDefaults_Click does)
            
            // 1. Disable shader injection
            modifiedSettings.EnableShaderInjection = false;
            modifiedSettings.EnableClearType = false;
            service.ApplyShaderSettings(modifiedSettings);

            // 2. Restore Windows ClearType defaults
            service.RestoreWindowsDefaults();

            // 3. Remove startup entry
            settingsService.SetStartWithWindows(false);

            // 4. Clear saved settings
            settingsService.ClearSettings();

            // Assert - all defaults restored
            
            // ClearType should be enabled (Windows default)
            IntPtr restoredClearType = IntPtr.Zero;
            SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref restoredClearType, 0);
            Assert.NotEqual(IntPtr.Zero, restoredClearType);

            // Startup entry should be removed
            Assert.False(settingsService.IsStartWithWindowsEnabled());

            // Saved settings should be defaults
            var loadedSettings = settingsService.LoadSettings();
            Assert.Equal(SubpixelLayout.RgbStripe, loadedSettings.ShaderLayout);
            Assert.Equal(1.0, loadedSettings.ShaderIntensity);
            Assert.Empty(loadedSettings.MonitorSettings);
        }
        finally
        {
            // Cleanup - ensure ClearType is enabled
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            settingsService.SetStartWithWindows(false);
            settingsService.ClearSettings();
            service.Dispose();
        }
    }

    /// <summary>
    /// Tests that reset can be performed multiple times without issues
    /// </summary>
    [Fact]
    public void MultipleResets_WorkCorrectly()
    {
        // Arrange
        var service = new DisplayShaderService();
        var settingsService = new SettingsService();

        try
        {
            for (int i = 0; i < 3; i++)
            {
                // Apply some settings
                var settings = new DisplaySettings
                {
                    EnableClearType = false,
                    EnableShaderInjection = false,
                    ShaderLayout = SubpixelLayout.WrgbStripe
                };
                settingsService.SaveSettings(settings);
                service.ApplyShaderSettings(settings);

                // Reset
                service.RestoreWindowsDefaults();
                settingsService.ClearSettings();

                // Verify defaults
                IntPtr clearTypeState = IntPtr.Zero;
                SystemParametersInfoGet(SPI_GETFONTSMOOTHING, 0, ref clearTypeState, 0);
                Assert.NotEqual(IntPtr.Zero, clearTypeState);

                var loaded = settingsService.LoadSettings();
                Assert.Equal(SubpixelLayout.RgbStripe, loaded.ShaderLayout);
            }
        }
        finally
        {
            SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, IntPtr.Zero, 0x01 | 0x02);
            settingsService.ClearSettings();
            service.Dispose();
        }
    }
}
