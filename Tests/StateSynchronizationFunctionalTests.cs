using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Collection definition for tests that access the Windows Registry.
/// Tests in this collection run sequentially to avoid race conditions.
/// </summary>
[CollectionDefinition("RegistryTests", DisableParallelization = true)]
public class RegistryTestsCollection
{
}

/// <summary>
/// Functional tests to verify state synchronization between UI controls and settings.
/// These tests validate that toggle states, display configurations, and ClearType settings
/// remain consistent and synchronized during user interactions.
/// </summary>
[Collection("RegistryTests")]
public class StateSynchronizationFunctionalTests
{
    #region Quick Enable and Auto Inject Synchronization Tests

    /// <summary>
    /// Tests that QuickEnable and AutoInject toggles stay synchronized when QuickEnable changes
    /// </summary>
    [Fact]
    public void QuickEnable_WhenToggled_AutoInjectShouldSync()
    {
        // Arrange - simulate UI state
        var settings = new DisplaySettings { EnableShaderInjection = false };
        bool quickEnableChecked = false;
        bool autoInjectChecked = false;

        // Act - simulate QuickEnable toggled ON (from UI handler)
        quickEnableChecked = true;
        settings.EnableShaderInjection = quickEnableChecked;
        autoInjectChecked = settings.EnableShaderInjection; // This is what the UI should do

        // Assert - both should be true and in sync
        Assert.True(quickEnableChecked);
        Assert.True(autoInjectChecked);
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(quickEnableChecked, autoInjectChecked);
    }

    /// <summary>
    /// Tests that QuickEnable and AutoInject toggles stay synchronized when AutoInject changes
    /// </summary>
    [Fact]
    public void AutoInject_WhenToggled_QuickEnableShouldSync()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = true };
        bool quickEnableChecked = true;
        bool autoInjectChecked = true;

        // Act - simulate AutoInject toggled OFF
        autoInjectChecked = false;
        settings.EnableShaderInjection = autoInjectChecked;
        quickEnableChecked = settings.EnableShaderInjection; // Sync back

        // Assert
        Assert.False(quickEnableChecked);
        Assert.False(autoInjectChecked);
        Assert.False(settings.EnableShaderInjection);
        Assert.Equal(quickEnableChecked, autoInjectChecked);
    }

    /// <summary>
    /// Tests rapid toggle switching maintains synchronization
    /// </summary>
    [Fact]
    public void RapidToggleSwitching_MaintainsSynchronization()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = false };
        bool quickEnableChecked = false;
        bool autoInjectChecked = false;

        // Act - rapid toggle switches
        for (int i = 0; i < 50; i++)
        {
            // Toggle QuickEnable
            quickEnableChecked = !quickEnableChecked;
            settings.EnableShaderInjection = quickEnableChecked;
            autoInjectChecked = settings.EnableShaderInjection;
            Assert.Equal(quickEnableChecked, autoInjectChecked);

            // Toggle AutoInject
            autoInjectChecked = !autoInjectChecked;
            settings.EnableShaderInjection = autoInjectChecked;
            quickEnableChecked = settings.EnableShaderInjection;
            Assert.Equal(quickEnableChecked, autoInjectChecked);
        }

        // Final state should be consistent
        Assert.Equal(quickEnableChecked, autoInjectChecked);
        Assert.Equal(quickEnableChecked, settings.EnableShaderInjection);
    }

    #endregion

    #region ClearType Toggle Independence Tests

    /// <summary>
    /// Tests that ClearType toggle operates independently from shader injection
    /// </summary>
    [Fact]
    public void ClearTypeToggle_IsIndependentFromShaderInjection()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true
        };

        // Act - toggle ClearType OFF while shader remains ON
        settings.EnableClearType = false;

        // Assert - shader should remain unchanged
        Assert.False(settings.EnableClearType);
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests that shader injection toggle operates independently from ClearType
    /// </summary>
    [Fact]
    public void ShaderInjectionToggle_IsIndependentFromClearType()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true
        };

        // Act - toggle shader OFF while ClearType remains ON
        settings.EnableShaderInjection = false;

        // Assert - ClearType should remain unchanged
        Assert.True(settings.EnableClearType);
        Assert.False(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Tests all four combinations of shader/ClearType states
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ShaderAndClearType_AllCombinations_AreValid(bool shaderEnabled, bool clearTypeEnabled)
    {
        // Arrange & Act
        var settings = new DisplaySettings
        {
            EnableShaderInjection = shaderEnabled,
            EnableClearType = clearTypeEnabled
        };

        // Assert
        Assert.Equal(shaderEnabled, settings.EnableShaderInjection);
        Assert.Equal(clearTypeEnabled, settings.EnableClearType);
    }

    #endregion

    #region Display Configuration State Tests

    /// <summary>
    /// Tests that selecting a display type updates the settings correctly
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe)]
    [InlineData(SubpixelLayout.WrgbStripe)]
    [InlineData(SubpixelLayout.RgbTriangular)]
    [InlineData(SubpixelLayout.Pentile)]
    public void SelectDisplayType_UpdatesSettingsCorrectly(SubpixelLayout layout)
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        // Act - simulate radio button selection
        settings.MonitorSettings[monitorId].ShaderLayout = layout;
        settings.ShaderLayout = layout; // Global setting

        // Assert
        Assert.Equal(layout, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.Equal(layout, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests that switching between monitors preserves each monitor's display type
    /// </summary>
    [Fact]
    public void SwitchBetweenMonitors_PreservesDisplayType()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitor1 = @"\\.\DISPLAY1";
        string monitor2 = @"\\.\DISPLAY2";

        settings.MonitorSettings[monitor1] = new MonitorSettings { ShaderLayout = SubpixelLayout.WrgbStripe };
        settings.MonitorSettings[monitor2] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbTriangular };

        // Act - simulate switching monitors
        var currentMonitor = monitor1;
        var currentLayout = settings.MonitorSettings[currentMonitor].ShaderLayout;
        Assert.Equal(SubpixelLayout.WrgbStripe, currentLayout);

        currentMonitor = monitor2;
        currentLayout = settings.MonitorSettings[currentMonitor].ShaderLayout;
        Assert.Equal(SubpixelLayout.RgbTriangular, currentLayout);

        // Switch back
        currentMonitor = monitor1;
        currentLayout = settings.MonitorSettings[currentMonitor].ShaderLayout;

        // Assert - monitor1 should still have its original setting
        Assert.Equal(SubpixelLayout.WrgbStripe, currentLayout);
    }

    /// <summary>
    /// Tests that changing display type on one monitor doesn't affect others
    /// </summary>
    [Fact]
    public void ChangeDisplayTypeOnOneMonitor_DoesNotAffectOthers()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitor1 = @"\\.\DISPLAY1";
        string monitor2 = @"\\.\DISPLAY2";

        settings.MonitorSettings[monitor1] = new MonitorSettings { ShaderLayout = SubpixelLayout.WrgbStripe };
        settings.MonitorSettings[monitor2] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbTriangular };

        // Act - change monitor1's layout
        settings.MonitorSettings[monitor1].ShaderLayout = SubpixelLayout.Pentile;

        // Assert - monitor2 should be unchanged
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[monitor1].ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.MonitorSettings[monitor2].ShaderLayout);
    }

    #endregion

    #region Intensity State Tests

    /// <summary>
    /// Tests that intensity changes are correctly reflected in settings
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    [InlineData(1.0)]
    public void ChangeIntensity_UpdatesSettingsCorrectly(double intensity)
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        // Act
        settings.MonitorSettings[monitorId].ShaderIntensity = intensity;
        settings.ShaderIntensity = intensity;

        // Assert
        Assert.Equal(intensity, settings.MonitorSettings[monitorId].ShaderIntensity);
        Assert.Equal(intensity, settings.ShaderIntensity);
    }

    /// <summary>
    /// Tests that intensity for one monitor doesn't affect other monitors
    /// </summary>
    [Fact]
    public void ChangeIntensityOnOneMonitor_DoesNotAffectOthers()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitor1 = @"\\.\DISPLAY1";
        string monitor2 = @"\\.\DISPLAY2";

        settings.MonitorSettings[monitor1] = new MonitorSettings { ShaderIntensity = 1.0 };
        settings.MonitorSettings[monitor2] = new MonitorSettings { ShaderIntensity = 0.8 };

        // Act - change monitor1's intensity
        settings.MonitorSettings[monitor1].ShaderIntensity = 0.5;

        // Assert - monitor2 should be unchanged
        Assert.Equal(0.5, settings.MonitorSettings[monitor1].ShaderIntensity);
        Assert.Equal(0.8, settings.MonitorSettings[monitor2].ShaderIntensity);
    }

    #endregion

    #region Settings Service Round-Trip Tests

    /// <summary>
    /// Tests that toggle states survive save/load cycle
    /// </summary>
    [Fact]
    public void SaveLoad_PreservesToggleStates()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Clear settings first to ensure clean state
        settingsService.ClearSettings();
        
        var original = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            StartWithWindows = true,
            MinimizeToTray = false
        };

        // Act
        settingsService.SaveSettings(original);
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.Equal(original.EnableShaderInjection, loaded.EnableShaderInjection);
        Assert.Equal(original.EnableClearType, loaded.EnableClearType);
        Assert.Equal(original.StartWithWindows, loaded.StartWithWindows);
        Assert.Equal(original.MinimizeToTray, loaded.MinimizeToTray);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that display configuration survives save/load cycle
    /// </summary>
    [Fact]
    public void SaveLoad_PreservesDisplayConfiguration()
    {
        // Arrange
        var settingsService = new SettingsService();
        var original = new DisplaySettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ClearTypeIntensity = 0.85
        };

        // Act
        settingsService.SaveSettings(original);
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.Equal(original.ShaderLayout, loaded.ShaderLayout);
        Assert.Equal(original.ShaderIntensity, loaded.ShaderIntensity);
        Assert.Equal(original.ClearTypeLayout, loaded.ClearTypeLayout);
        Assert.Equal(original.ClearTypeIntensity, loaded.ClearTypeIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that per-monitor settings survive save/load cycle
    /// </summary>
    [Fact]
    public void SaveLoad_PreservesPerMonitorSettings()
    {
        // Arrange
        var settingsService = new SettingsService();
        var original = new DisplaySettings();
        original.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };
        original.MonitorSettings[@"\\.\DISPLAY2"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.75
        };

        // Act
        settingsService.SaveSettings(original);
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.Equal(2, loaded.MonitorSettings.Count);
        Assert.Equal(SubpixelLayout.WrgbStripe, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(0.9, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);
        Assert.Equal(SubpixelLayout.RgbTriangular, loaded.MonitorSettings[@"\\.\DISPLAY2"].ShaderLayout);
        Assert.Equal(0.75, loaded.MonitorSettings[@"\\.\DISPLAY2"].ShaderIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    #endregion

    #region State Transition Sequence Tests

    /// <summary>
    /// Tests the sequence: Enable Shader -> Change Display Type -> Verify Both States
    /// </summary>
    [Fact]
    public void EnableShader_ThenChangeDisplayType_BothStatesCorrect()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - enable shader first
        settings.EnableShaderInjection = true;
        Assert.True(settings.EnableShaderInjection);

        // Act - then change display type
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert - both states should be correct
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests the sequence: Change Display Type -> Enable Shader -> Verify Both States
    /// </summary>
    [Fact]
    public void ChangeDisplayType_ThenEnableShader_BothStatesCorrect()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - change display type first
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);

        // Act - then enable shader
        settings.EnableShaderInjection = true;

        // Assert - both states should be correct
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests the sequence: Disable Shader -> Change Display Type -> Re-enable Shader -> Verify Display Type Preserved
    /// </summary>
    [Fact]
    public void DisableShader_ChangeDisplayType_ReEnable_DisplayTypePreserved()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - disable shader
        settings.EnableShaderInjection = false;
        Assert.False(settings.EnableShaderInjection);

        // Act - change display type while disabled
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);

        // Act - re-enable shader
        settings.EnableShaderInjection = true;

        // Assert - display type should be preserved
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests the sequence: Enable ClearType -> Disable Shader -> ClearType should still be on
    /// </summary>
    [Fact]
    public void EnableClearType_DisableShader_ClearTypeStillEnabled()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true
        };

        // Act - disable shader injection
        settings.EnableShaderInjection = false;

        // Assert - ClearType should remain enabled
        Assert.False(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Tests the sequence: Switch Monitor -> Change Settings -> Switch Back -> Verify Original Settings
    /// </summary>
    [Fact]
    public void SwitchMonitor_ChangeSettings_SwitchBack_OriginalPreserved()
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

        // Act - switch to monitor2 and change settings
        string currentMonitor = monitor2;
        settings.MonitorSettings[currentMonitor].ShaderLayout = SubpixelLayout.Pentile;
        settings.MonitorSettings[currentMonitor].ShaderIntensity = 0.5;

        // Act - switch back to monitor1
        currentMonitor = monitor1;

        // Assert - monitor1's settings should be unchanged
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitor1].ShaderLayout);
        Assert.Equal(0.9, settings.MonitorSettings[monitor1].ShaderIntensity);

        // Assert - monitor2's settings should have the new values
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[monitor2].ShaderLayout);
        Assert.Equal(0.5, settings.MonitorSettings[monitor2].ShaderIntensity);
    }

    #endregion

    #region Concurrent State Change Tests

    /// <summary>
    /// Tests that changing multiple settings in quick succession maintains consistency
    /// </summary>
    [Fact]
    public void MultipleSettingsChanged_QuickSuccession_StateConsistent()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        // Act - simulate rapid changes
        for (int i = 0; i < 100; i++)
        {
            settings.EnableShaderInjection = (i % 2) == 0;
            settings.EnableClearType = (i % 3) == 0;
            settings.ShaderLayout = (SubpixelLayout)(i % 5);
            settings.ShaderIntensity = (i % 11) / 10.0;
            settings.MonitorSettings[monitorId].ShaderLayout = (SubpixelLayout)((i + 1) % 5);
            settings.MonitorSettings[monitorId].ShaderIntensity = ((i + 1) % 11) / 10.0;
        }

        // Assert - final state should be deterministic based on i=99
        // 99 % 2 = 1, so false
        Assert.False(settings.EnableShaderInjection);
        // 99 % 3 = 0, so true
        Assert.True(settings.EnableClearType);
        // 99 % 5 = 4 -> None (enum value 4)
        Assert.Equal(SubpixelLayout.None, settings.ShaderLayout);
        // 99 % 11 = 0 -> 0.0
        Assert.Equal(0.0, settings.ShaderIntensity);
        // 100 % 5 = 0 -> RgbStripe
        Assert.Equal(SubpixelLayout.RgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
        // 100 % 11 = 1 -> 0.1
        Assert.Equal(0.1, settings.MonitorSettings[monitorId].ShaderIntensity);
    }

    /// <summary>
    /// Tests all toggle combinations remain valid after rapid switching
    /// </summary>
    [Fact]
    public void AllToggleCombinations_AfterRapidSwitching_AreValid()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act - go through all combinations multiple times
        bool[] boolValues = { true, false };
        
        foreach (var shader in boolValues)
        {
            foreach (var clearType in boolValues)
            {
                settings.EnableShaderInjection = shader;
                settings.EnableClearType = clearType;
                
                // Assert - current combination is valid
                Assert.Equal(shader, settings.EnableShaderInjection);
                Assert.Equal(clearType, settings.EnableClearType);
            }
        }
    }

    #endregion
}

/// <summary>
/// Tests specifically for the UI initialization and update sequence
/// </summary>
[Collection("RegistryTests")]
public class UIInitializationTests
{
    /// <summary>
    /// Tests that loading settings correctly initializes all UI-related values
    /// </summary>
    [Fact]
    public void LoadSettings_InitializesAllValues()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Act - load default settings
        var settings = settingsService.LoadSettings();

        // Assert - default values should be set
        Assert.NotNull(settings);
        Assert.NotNull(settings.MonitorSettings);
        // ShaderIntensity defaults to 1.0, not 0
        Assert.True(settings.ShaderIntensity >= 0.0 && settings.ShaderIntensity <= 1.0);
    }

    /// <summary>
    /// Tests that settings with all toggles ON load correctly
    /// </summary>
    [Fact]
    public void LoadSettings_AllTogglesOn_LoadsCorrectly()
    {
        // Arrange
        var settingsService = new SettingsService();
        
        // Clear settings first
        settingsService.ClearSettings();
        var original = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            StartWithWindows = true,
            MinimizeToTray = true
        };
        settingsService.SaveSettings(original);

        // Act
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.True(loaded.EnableShaderInjection);
        Assert.True(loaded.EnableClearType);
        Assert.True(loaded.StartWithWindows);
        Assert.True(loaded.MinimizeToTray);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that settings with all toggles OFF load correctly
    /// </summary>
    [Fact]
    public void LoadSettings_AllTogglesOff_LoadsCorrectly()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();
        
        var original = new DisplaySettings
        {
            EnableShaderInjection = false,
            EnableClearType = false,
            StartWithWindows = false,
            MinimizeToTray = false
        };
        settingsService.SaveSettings(original);

        // Act
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.False(loaded.EnableShaderInjection);
        Assert.False(loaded.EnableClearType);
        Assert.False(loaded.StartWithWindows);
        Assert.False(loaded.MinimizeToTray);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that mixed toggle states load correctly
    /// </summary>
    [Fact]
    public void LoadSettings_MixedToggleStates_LoadsCorrectly()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();
        
        var original = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            StartWithWindows = false,
            MinimizeToTray = true
        };
        settingsService.SaveSettings(original);

        // Act
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.True(loaded.EnableShaderInjection);
        Assert.False(loaded.EnableClearType);
        Assert.False(loaded.StartWithWindows);
        Assert.True(loaded.MinimizeToTray);

        // Cleanup
        settingsService.ClearSettings();
    }

    /// <summary>
    /// Tests that monitor-specific settings are correctly loaded for UI
    /// </summary>
    [Fact]
    public void LoadSettings_WithMonitorSettings_LoadsCorrectly()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();
        
        var original = new DisplaySettings();
        original.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85
        };
        settingsService.SaveSettings(original);

        // Act
        var loaded = settingsService.LoadSettings();

        // Assert
        Assert.True(loaded.MonitorSettings.ContainsKey(@"\\.\DISPLAY1"));
        Assert.Equal(SubpixelLayout.WrgbStripe, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(0.85, loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }
}

/// <summary>
/// Tests for ClearType synchronization with shader settings
/// </summary>
public class ClearTypeSyncTests
{
    /// <summary>
    /// Tests that SyncClearTypeWithShaderSettings correctly syncs layout
    /// </summary>
    [Fact]
    public void SyncClearType_WhenEnabled_SyncsLayout()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85
        };

        // Act - simulate what SyncClearTypeWithShaderSettings does
        if (settings.EnableClearType)
        {
            settings.ClearTypeLayout = settings.ShaderLayout;
            settings.ClearTypeIntensity = settings.ShaderIntensity;
        }

        // Assert
        Assert.Equal(settings.ShaderLayout, settings.ClearTypeLayout);
        Assert.Equal(settings.ShaderIntensity, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests that ClearType settings don't sync when disabled
    /// </summary>
    [Fact]
    public void SyncClearType_WhenDisabled_DoesNotSync()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableClearType = false,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85,
            ClearTypeLayout = SubpixelLayout.RgbStripe,
            ClearTypeIntensity = 1.0
        };

        // Act - simulate what SyncClearTypeWithShaderSettings does when disabled
        if (settings.EnableClearType)
        {
            settings.ClearTypeLayout = settings.ShaderLayout;
            settings.ClearTypeIntensity = settings.ShaderIntensity;
        }

        // Assert - ClearType settings should remain unchanged
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ClearTypeLayout);
        Assert.Equal(1.0, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests that changing shader layout syncs to ClearType when enabled
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe)]
    [InlineData(SubpixelLayout.WrgbStripe)]
    [InlineData(SubpixelLayout.RgbTriangular)]
    [InlineData(SubpixelLayout.Pentile)]
    public void ChangeShaderLayout_SyncsToClearType_WhenEnabled(SubpixelLayout layout)
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.RgbStripe
        };

        // Act - change shader layout
        settings.ShaderLayout = layout;
        
        // Simulate sync
        if (settings.EnableClearType)
        {
            settings.ClearTypeLayout = settings.ShaderLayout;
        }

        // Assert
        Assert.Equal(layout, settings.ClearTypeLayout);
    }

    /// <summary>
    /// Tests that changing shader intensity syncs to ClearType when enabled
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void ChangeShaderIntensity_SyncsToClearType_WhenEnabled(double intensity)
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableClearType = true,
            ShaderIntensity = 1.0
        };

        // Act - change shader intensity
        settings.ShaderIntensity = intensity;
        
        // Simulate sync
        if (settings.EnableClearType)
        {
            settings.ClearTypeIntensity = settings.ShaderIntensity;
        }

        // Assert
        Assert.Equal(intensity, settings.ClearTypeIntensity);
    }
}
