using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Tests for verifying UI state management and synchronization.
/// These tests ensure that toggle states, display configurations, and settings
/// remain consistent across UI interactions and service updates.
/// </summary>
/// <remarks>
/// Tests that use the registry must run sequentially to avoid race conditions
/// </remarks>
[Collection("RegistryTests")]
public class StateManagementTests
{
    #region Toggle State Synchronization Tests

    /// <summary>
    /// Tests that enabling shader injection updates the settings correctly
    /// </summary>
    [Fact]
    public void EnableShaderInjection_UpdatesSettingsCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = false };

        // Act
        settings.EnableShaderInjection = true;

        // Assert
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests that disabling shader injection updates the settings correctly
    /// </summary>
    [Fact]
    public void DisableShaderInjection_UpdatesSettingsCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = true };

        // Act
        settings.EnableShaderInjection = false;

        // Assert
        Assert.False(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests that enabling ClearType updates the settings correctly
    /// </summary>
    [Fact]
    public void EnableClearType_UpdatesSettingsCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings { EnableClearType = false };

        // Act
        settings.EnableClearType = true;

        // Assert
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Tests that disabling ClearType updates the settings correctly
    /// </summary>
    [Fact]
    public void DisableClearType_UpdatesSettingsCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings { EnableClearType = true };

        // Act
        settings.EnableClearType = false;

        // Assert
        Assert.False(settings.EnableClearType);
    }

    /// <summary>
    /// Tests that shader injection and ClearType can be toggled independently
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ShaderAndClearType_IndependentToggling(bool shaderEnabled, bool clearTypeEnabled)
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act
        settings.EnableShaderInjection = shaderEnabled;
        settings.EnableClearType = clearTypeEnabled;

        // Assert
        Assert.Equal(shaderEnabled, settings.EnableShaderInjection);
        Assert.Equal(clearTypeEnabled, settings.EnableClearType);
    }

    /// <summary>
    /// Tests rapid toggle switching doesn't corrupt state
    /// </summary>
    [Fact]
    public void RapidToggleSwitching_MaintainsConsistentState()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act - simulate rapid toggle switching
        for (int i = 0; i < 100; i++)
        {
            settings.EnableShaderInjection = (i % 2) == 0;
            settings.EnableClearType = (i % 3) == 0;
        }

        // Assert - final state should be consistent with last assignments
        Assert.False(settings.EnableShaderInjection); // 99 % 2 = 1, so false
        Assert.True(settings.EnableClearType);        // 99 % 3 = 0, so true
    }

    #endregion

    #region Display Configuration State Tests

    /// <summary>
    /// Tests that changing display type updates settings for the correct monitor
    /// </summary>
    [Fact]
    public void ChangeDisplayType_UpdatesCorrectMonitor()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
    }

    /// <summary>
    /// Tests that changing display type doesn't affect other monitors
    /// </summary>
    [Fact]
    public void ChangeDisplayType_DoesNotAffectOtherMonitors()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitor1 = @"\\.\DISPLAY1";
        string monitor2 = @"\\.\DISPLAY2";
        
        settings.MonitorSettings[monitor1] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };
        settings.MonitorSettings[monitor2] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbTriangular };

        // Act - change only monitor 1
        settings.MonitorSettings[monitor1].ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert - monitor 2 should be unchanged
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitor1].ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.MonitorSettings[monitor2].ShaderLayout);
    }

    /// <summary>
    /// Tests that all display types can be selected without state corruption
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe)]
    [InlineData(SubpixelLayout.WrgbStripe)]
    [InlineData(SubpixelLayout.RgbTriangular)]
    [InlineData(SubpixelLayout.Pentile)]
    [InlineData(SubpixelLayout.None)]
    public void SelectDisplayType_StateRemainsConsistent(SubpixelLayout layout)
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        // Act
        settings.MonitorSettings[monitorId].ShaderLayout = layout;
        settings.ShaderLayout = layout; // Also set global for primary

        // Assert
        Assert.Equal(layout, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.Equal(layout, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests cycling through all display types in sequence
    /// </summary>
    [Fact]
    public void CycleThroughAllDisplayTypes_StateRemainsConsistent()
    {
        // Arrange
        var settings = new DisplaySettings();
        var layouts = Enum.GetValues<SubpixelLayout>();

        // Act & Assert - cycle through each layout
        foreach (var layout in layouts)
        {
            settings.ShaderLayout = layout;
            Assert.Equal(layout, settings.ShaderLayout);
        }

        // Final state should be the last layout
        Assert.Equal(SubpixelLayout.None, settings.ShaderLayout);
    }

    #endregion

    #region Intensity Slider State Tests

    /// <summary>
    /// Tests that intensity changes are applied correctly
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void ChangeIntensity_UpdatesSettingsCorrectly(double intensity)
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act
        settings.ShaderIntensity = intensity;

        // Assert
        Assert.Equal(intensity, settings.ShaderIntensity);
    }

    /// <summary>
    /// Tests that intensity changes for specific monitors work correctly
    /// </summary>
    [Fact]
    public void ChangeMonitorIntensity_UpdatesCorrectMonitor()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderIntensity = 1.0 };

        // Act
        settings.MonitorSettings[monitorId].ShaderIntensity = 0.5;

        // Assert
        Assert.Equal(0.5, settings.MonitorSettings[monitorId].ShaderIntensity);
    }

    /// <summary>
    /// Tests rapid intensity changes don't corrupt state
    /// </summary>
    [Fact]
    public void RapidIntensityChanges_MaintainsConsistentState()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act - simulate rapid slider movement
        for (double i = 0; i <= 1.0; i += 0.1)
        {
            settings.ShaderIntensity = i;
        }
        settings.ShaderIntensity = 0.7;

        // Assert
        Assert.Equal(0.7, settings.ShaderIntensity);
    }

    #endregion

    #region Combined State Change Tests

    /// <summary>
    /// Tests that changing multiple settings simultaneously maintains consistency
    /// </summary>
    [Fact]
    public void MultipleSimultaneousChanges_MaintainsConsistency()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act - change multiple settings at once
        settings.EnableShaderInjection = true;
        settings.EnableClearType = false;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderIntensity = 0.85;

        // Assert
        Assert.True(settings.EnableShaderInjection);
        Assert.False(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(0.85, settings.ShaderIntensity);
    }

    /// <summary>
    /// Tests toggling shader then changing display type maintains correct state
    /// </summary>
    [Fact]
    public void ToggleShader_ThenChangeDisplayType_StateRemainsConsistent()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - enable shader, then change display type
        settings.EnableShaderInjection = true;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests changing display type then toggling shader maintains correct state
    /// </summary>
    [Fact]
    public void ChangeDisplayType_ThenToggleShader_StateRemainsConsistent()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - change display type, then disable shader
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.EnableShaderInjection = false;

        // Assert - display type should be preserved even when shader is disabled
        Assert.False(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests enabling ClearType then disabling shader injection maintains ClearType state
    /// </summary>
    [Fact]
    public void EnableClearType_ThenDisableShader_ClearTypeRemains()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false
        };

        // Act
        settings.EnableClearType = true;
        settings.EnableShaderInjection = false;

        // Assert - ClearType should remain enabled independently
        Assert.False(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Tests switching between monitors preserves each monitor's settings
    /// </summary>
    [Fact]
    public void SwitchBetweenMonitors_PreservesSettings()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitor1 = @"\\.\DISPLAY1";
        string monitor2 = @"\\.\DISPLAY2";

        settings.MonitorSettings[monitor1] = new MonitorSettings 
        { 
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9 
        };
        settings.MonitorSettings[monitor2] = new MonitorSettings 
        { 
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.8 
        };

        // Act - simulate switching to monitor 2 and changing settings
        var currentMonitor = monitor2;
        settings.MonitorSettings[currentMonitor].ShaderLayout = SubpixelLayout.Pentile;

        // Switch back to monitor 1 (simulate UI)
        currentMonitor = monitor1;
        
        // Assert - monitor 1 settings should be unchanged
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitor1].ShaderLayout);
        Assert.Equal(0.9, settings.MonitorSettings[monitor1].ShaderIntensity);
        
        // Assert - monitor 2 should have new settings
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[monitor2].ShaderLayout);
        Assert.Equal(0.8, settings.MonitorSettings[monitor2].ShaderIntensity);
    }

    #endregion

    #region Settings Service State Tests

    /// <summary>
    /// Tests that saving and loading settings preserves toggle states
    /// </summary>
    [Fact]
    public void SaveAndLoadSettings_PreservesToggleStates()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Clear settings first to ensure clean state
        settingsService.ClearSettings();
        
        var originalSettings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            StartWithWindows = true,
            MinimizeToTray = true
        };

        // Act
        settingsService.SaveSettings(originalSettings);
        var loadedSettings = settingsService.LoadSettings();

        // Assert
        Assert.Equal(originalSettings.EnableShaderInjection, loadedSettings.EnableShaderInjection);
        Assert.Equal(originalSettings.EnableClearType, loadedSettings.EnableClearType);
        Assert.Equal(originalSettings.StartWithWindows, loadedSettings.StartWithWindows);
        Assert.Equal(originalSettings.MinimizeToTray, loadedSettings.MinimizeToTray);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that saving and loading settings preserves display configuration
    /// </summary>
    [Fact]
    public void SaveAndLoadSettings_PreservesDisplayConfiguration()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Clear settings first to ensure clean state
        settingsService.ClearSettings();
        
        var originalSettings = new DisplaySettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ClearTypeIntensity = 0.85
        };

        // Act
        settingsService.SaveSettings(originalSettings);
        var loadedSettings = settingsService.LoadSettings();

        // Assert
        Assert.Equal(originalSettings.ShaderLayout, loadedSettings.ShaderLayout);
        Assert.Equal(originalSettings.ShaderIntensity, loadedSettings.ShaderIntensity);
        Assert.Equal(originalSettings.ClearTypeLayout, loadedSettings.ClearTypeLayout);
        Assert.Equal(originalSettings.ClearTypeIntensity, loadedSettings.ClearTypeIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that saving and loading settings preserves per-monitor settings
    /// </summary>
    [Fact]
    public void SaveAndLoadSettings_PreservesMonitorSettings()
    {
        // Arrange
        var originalSettings = new DisplaySettings();
        originalSettings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };
        originalSettings.MonitorSettings[@"\\.\DISPLAY2"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.8
        };

        var settingsService = new SettingsService();

        // Act
        settingsService.SaveSettings(originalSettings);
        var loadedSettings = settingsService.LoadSettings();

        // Assert
        Assert.Equal(2, loadedSettings.MonitorSettings.Count);
        Assert.True(loadedSettings.MonitorSettings.ContainsKey(@"\\.\DISPLAY1"));
        Assert.True(loadedSettings.MonitorSettings.ContainsKey(@"\\.\DISPLAY2"));
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedSettings.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(0.9, loadedSettings.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);
        Assert.Equal(SubpixelLayout.RgbTriangular, loadedSettings.MonitorSettings[@"\\.\DISPLAY2"].ShaderLayout);
        Assert.Equal(0.8, loadedSettings.MonitorSettings[@"\\.\DISPLAY2"].ShaderIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that multiple save operations don't corrupt settings
    /// </summary>
    [Fact]
    public void MultipleSaveOperations_DoNotCorruptSettings()
    {
        // Arrange
        var settingsService = new SettingsService();
        var settings = new DisplaySettings();

        // Act - save multiple times with different values
        for (int i = 0; i < 10; i++)
        {
            settings.EnableShaderInjection = (i % 2) == 0;
            settings.ShaderLayout = (SubpixelLayout)(i % 5);
            settings.ShaderIntensity = (double)i / 10;
            settingsService.SaveSettings(settings);
        }

        // Final save
        settings.EnableShaderInjection = true;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderIntensity = 0.77;
        settingsService.SaveSettings(settings);

        // Load and verify
        var loadedSettings = settingsService.LoadSettings();

        // Assert
        Assert.True(loadedSettings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedSettings.ShaderLayout);
        Assert.Equal(0.77, loadedSettings.ShaderIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that settings work with empty monitor collection
    /// </summary>
    [Fact]
    public void EmptyMonitorCollection_HandledCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Assert
        Assert.NotNull(settings.MonitorSettings);
        Assert.Empty(settings.MonitorSettings);
    }

    /// <summary>
    /// Tests adding monitor settings to empty collection
    /// </summary>
    [Fact]
    public void AddMonitorToEmptyCollection_WorksCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings();
        Assert.Empty(settings.MonitorSettings);

        // Act
        settings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };

        // Assert
        Assert.Single(settings.MonitorSettings);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
    }

    /// <summary>
    /// Tests accessing non-existent monitor settings doesn't crash
    /// </summary>
    [Fact]
    public void AccessNonExistentMonitor_HandledGracefully()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act & Assert - should not throw
        bool exists = settings.MonitorSettings.TryGetValue(@"\\.\DISPLAY99", out var monitorSettings);
        Assert.False(exists);
        Assert.Null(monitorSettings);
    }

    /// <summary>
    /// Tests updating monitor settings that doesn't exist yet
    /// </summary>
    [Fact]
    public void UpdateNonExistentMonitor_CreatesNewEntry()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY3";

        // Act
        if (!settings.MonitorSettings.TryGetValue(monitorId, out var monitorSettings))
        {
            monitorSettings = new MonitorSettings();
            settings.MonitorSettings[monitorId] = monitorSettings;
        }
        monitorSettings.ShaderLayout = SubpixelLayout.Pentile;

        // Assert
        Assert.True(settings.MonitorSettings.ContainsKey(monitorId));
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[monitorId].ShaderLayout);
    }

    /// <summary>
    /// Tests intensity boundary values
    /// </summary>
    [Theory]
    [InlineData(-0.1)] // Below minimum (should still be set, validation is UI responsibility)
    [InlineData(0.0)]  // Minimum
    [InlineData(1.0)]  // Maximum
    [InlineData(1.5)]  // Above maximum (should still be set, validation is UI responsibility)
    public void IntensityBoundaryValues_HandledCorrectly(double intensity)
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act
        settings.ShaderIntensity = intensity;

        // Assert
        Assert.Equal(intensity, settings.ShaderIntensity);
    }

    #endregion

    #region State Transition Tests

    /// <summary>
    /// Tests the complete flow of: load -> modify -> save -> reload
    /// </summary>
    [Fact]
    public void FullStateTransitionCycle_MaintainsIntegrity()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Clear any previous state first
        settingsService.ClearSettings();
        
        // Initial state
        var settings = settingsService.LoadSettings();
        
        // Clear any existing monitor settings to start fresh
        settings.MonitorSettings.Clear();
        
        // Act - modify everything
        settings.EnableShaderInjection = true;
        settings.EnableClearType = false;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderIntensity = 0.85;
        settings.ClearTypeLayout = SubpixelLayout.WrgbStripe;
        settings.ClearTypeIntensity = 0.85;
        settings.StartWithWindows = true;
        settings.MinimizeToTray = true;
        
        settings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };
        
        // Save
        settingsService.SaveSettings(settings);
        
        // Reload in new instance
        var loadedSettings = settingsService.LoadSettings();

        // Assert all values preserved
        Assert.True(loadedSettings.EnableShaderInjection);
        Assert.False(loadedSettings.EnableClearType);
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedSettings.ShaderLayout);
        Assert.Equal(0.85, loadedSettings.ShaderIntensity);
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedSettings.ClearTypeLayout);
        Assert.Equal(0.85, loadedSettings.ClearTypeIntensity);
        Assert.True(loadedSettings.StartWithWindows);
        Assert.True(loadedSettings.MinimizeToTray);
        Assert.Single(loadedSettings.MonitorSettings);
        Assert.Equal(SubpixelLayout.WrgbStripe, loadedSettings.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that disabling and re-enabling shader preserves display type selection
    /// </summary>
    [Fact]
    public void DisableAndReenableShader_PreservesDisplayType()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };

        // Act - disable shader
        settings.EnableShaderInjection = false;
        
        // Assert - display type should be preserved
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(0.9, settings.ShaderIntensity);

        // Act - re-enable shader
        settings.EnableShaderInjection = true;

        // Assert - display type should still be preserved
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(0.9, settings.ShaderIntensity);
    }

    /// <summary>
    /// Tests that disabling and re-enabling ClearType preserves layout selection
    /// </summary>
    [Fact]
    public void DisableAndReenableClearType_PreservesLayout()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableClearType = true,
            ClearTypeLayout = SubpixelLayout.RgbTriangular,
            ClearTypeIntensity = 0.75
        };

        // Act - disable ClearType
        settings.EnableClearType = false;
        
        // Assert - layout should be preserved
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ClearTypeLayout);
        Assert.Equal(0.75, settings.ClearTypeIntensity);

        // Act - re-enable ClearType
        settings.EnableClearType = true;

        // Assert - layout should still be preserved
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ClearTypeLayout);
        Assert.Equal(0.75, settings.ClearTypeIntensity);
    }

    #endregion
}

/// <summary>
/// Tests specifically for UI toggle synchronization logic
/// These test the expected behavior of toggle interactions
/// </summary>
public class ToggleSynchronizationTests
{
    /// <summary>
    /// Simulates the QuickEnable toggle behavior - both toggles should sync
    /// </summary>
    [Fact]
    public void QuickEnableToggle_ShouldSyncWithAutoInject()
    {
        // Arrange - simulate two toggle states (QuickEnable and AutoInject)
        bool quickEnableChecked = false;
        bool autoInjectChecked = false;
        var settings = new DisplaySettings { EnableShaderInjection = false };

        // Act - simulate QuickEnable toggled ON
        quickEnableChecked = true;
        settings.EnableShaderInjection = quickEnableChecked;
        autoInjectChecked = settings.EnableShaderInjection; // Sync

        // Assert
        Assert.True(quickEnableChecked);
        Assert.True(autoInjectChecked);
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Simulates the AutoInject toggle behavior - both toggles should sync
    /// </summary>
    [Fact]
    public void AutoInjectToggle_ShouldSyncWithQuickEnable()
    {
        // Arrange - simulate two toggle states
        bool quickEnableChecked = true;
        bool autoInjectChecked = true;
        var settings = new DisplaySettings { EnableShaderInjection = true };

        // Act - simulate AutoInject toggled OFF
        autoInjectChecked = false;
        settings.EnableShaderInjection = autoInjectChecked;
        quickEnableChecked = settings.EnableShaderInjection; // Sync

        // Assert
        Assert.False(quickEnableChecked);
        Assert.False(autoInjectChecked);
        Assert.False(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests that ClearType toggle operates independently
    /// </summary>
    [Fact]
    public void ClearTypeToggle_OperatesIndependently()
    {
        // Arrange
        bool clearTypeChecked = false;
        bool shaderChecked = true;
        var settings = new DisplaySettings 
        { 
            EnableClearType = false,
            EnableShaderInjection = true 
        };

        // Act - toggle ClearType ON
        clearTypeChecked = true;
        settings.EnableClearType = clearTypeChecked;

        // Assert - shader state should be unchanged
        Assert.True(clearTypeChecked);
        Assert.True(shaderChecked);
        Assert.True(settings.EnableClearType);
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests that toggling shader OFF doesn't affect ClearType
    /// </summary>
    [Fact]
    public void ShaderToggleOff_DoesNotAffectClearType()
    {
        // Arrange
        var settings = new DisplaySettings 
        { 
            EnableClearType = true,
            EnableShaderInjection = true 
        };

        // Act - toggle shader OFF
        settings.EnableShaderInjection = false;

        // Assert - ClearType should remain ON
        Assert.False(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Tests that toggling ClearType OFF doesn't affect shader
    /// </summary>
    [Fact]
    public void ClearTypeToggleOff_DoesNotAffectShader()
    {
        // Arrange
        var settings = new DisplaySettings 
        { 
            EnableClearType = true,
            EnableShaderInjection = true 
        };

        // Act - toggle ClearType OFF
        settings.EnableClearType = false;

        // Assert - Shader should remain ON
        Assert.True(settings.EnableShaderInjection);
        Assert.False(settings.EnableClearType);
    }
}

/// <summary>
/// Tests for validating display configuration radio button behavior
/// </summary>
public class DisplayConfigurationTests
{
    /// <summary>
    /// Tests that only one display type can be selected at a time
    /// </summary>
    [Fact]
    public void OnlyOneDisplayType_CanBeSelected()
    {
        // Arrange
        var settings = new DisplaySettings();
        
        // Simulate radio button behavior - setting a layout should be exclusive
        var selectedLayout = SubpixelLayout.WrgbStripe;

        // Act
        settings.ShaderLayout = selectedLayout;

        // Assert - only one layout is set
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.NotEqual(SubpixelLayout.RgbStripe, settings.ShaderLayout);
        Assert.NotEqual(SubpixelLayout.RgbTriangular, settings.ShaderLayout);
        Assert.NotEqual(SubpixelLayout.Pentile, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests that selecting a new display type replaces the previous selection
    /// </summary>
    [Fact]
    public void SelectNewDisplayType_ReplacesPrevious()
    {
        // Arrange
        var settings = new DisplaySettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act - select a different layout
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests that display type selection is independent for each monitor
    /// </summary>
    [Fact]
    public void DisplayTypeSelection_IndependentPerMonitor()
    {
        // Arrange
        var settings = new DisplaySettings();
        settings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings { ShaderLayout = SubpixelLayout.WrgbStripe };
        settings.MonitorSettings[@"\\.\DISPLAY2"] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbTriangular };
        settings.MonitorSettings[@"\\.\DISPLAY3"] = new MonitorSettings { ShaderLayout = SubpixelLayout.Pentile };

        // Assert - each monitor can have different layout
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.MonitorSettings[@"\\.\DISPLAY2"].ShaderLayout);
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[@"\\.\DISPLAY3"].ShaderLayout);
    }

    /// <summary>
    /// Tests that changing display type on selected monitor updates correctly
    /// </summary>
    [Fact]
    public void ChangeDisplayType_OnSelectedMonitor_UpdatesCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings();
        string selectedMonitor = @"\\.\DISPLAY1";
        settings.MonitorSettings[selectedMonitor] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Simulate user selecting WOLED
        var newLayout = SubpixelLayout.WrgbStripe;

        // Act
        settings.MonitorSettings[selectedMonitor].ShaderLayout = newLayout;

        // Assert
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[selectedMonitor].ShaderLayout);
    }
}
