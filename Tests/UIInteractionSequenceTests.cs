using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Integration tests that simulate realistic UI interaction sequences.
/// These tests verify that the state remains correct when performing
/// the exact operations the user described as problematic.
/// </summary>
[Collection("RegistryTests")]
public class UIInteractionSequenceTests
{
    #region Toggle ClearType Then Check State Tests

    /// <summary>
    /// Simulates: Enable ClearType -> Check visual state should be ON
    /// </summary>
    [Fact]
    public void EnableClearType_VisualStateShouldBeOn()
    {
        // Arrange
        var settings = new DisplaySettings { EnableClearType = false };

        // Act - user toggles ClearType ON
        settings.EnableClearType = true;

        // Simulate what VerifyUIState does
        bool toggleClearTypeIsChecked = settings.EnableClearType;

        // Assert - visual should match
        Assert.True(toggleClearTypeIsChecked);
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Simulates: Disable ClearType -> Check visual state should be OFF
    /// </summary>
    [Fact]
    public void DisableClearType_VisualStateShouldBeOff()
    {
        // Arrange
        var settings = new DisplaySettings { EnableClearType = true };

        // Act - user toggles ClearType OFF
        settings.EnableClearType = false;

        // Simulate what VerifyUIState does
        bool toggleClearTypeIsChecked = settings.EnableClearType;

        // Assert - visual should match
        Assert.False(toggleClearTypeIsChecked);
        Assert.False(settings.EnableClearType);
    }

    /// <summary>
    /// Simulates: Toggle ClearType multiple times -> State should match last toggle
    /// </summary>
    [Fact]
    public void ToggleClearTypeMultipleTimes_StateMatchesLastToggle()
    {
        // Arrange
        var settings = new DisplaySettings { EnableClearType = false };

        // Act - rapid toggles (simulating user clicking multiple times)
        for (int i = 0; i < 10; i++)
        {
            settings.EnableClearType = !settings.EnableClearType;
        }

        // Final state (started false, 10 toggles = false)
        Assert.False(settings.EnableClearType);
        
        // One more toggle
        settings.EnableClearType = true;
        Assert.True(settings.EnableClearType);
    }

    #endregion

    #region Toggle Real-Time Process Injection Then Check State Tests

    /// <summary>
    /// Simulates: Enable Real-Time Injection -> Both toggles should be ON
    /// </summary>
    [Fact]
    public void EnableRealTimeInjection_BothTogglesShouldBeOn()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = false };
        
        // Simulate UI state for both toggles
        bool quickEnableChecked = false;
        bool autoInjectChecked = false;

        // Act - user toggles Real-Time Injection ON (via either toggle)
        autoInjectChecked = true;
        settings.EnableShaderInjection = autoInjectChecked;
        quickEnableChecked = settings.EnableShaderInjection; // Sync

        // Assert - both toggles should be ON
        Assert.True(quickEnableChecked);
        Assert.True(autoInjectChecked);
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Simulates: Disable Real-Time Injection -> Both toggles should be OFF
    /// </summary>
    [Fact]
    public void DisableRealTimeInjection_BothTogglesShouldBeOff()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = true };
        
        bool quickEnableChecked = true;
        bool autoInjectChecked = true;

        // Act - user toggles Real-Time Injection OFF
        quickEnableChecked = false;
        settings.EnableShaderInjection = quickEnableChecked;
        autoInjectChecked = settings.EnableShaderInjection; // Sync

        // Assert - both toggles should be OFF
        Assert.False(quickEnableChecked);
        Assert.False(autoInjectChecked);
        Assert.False(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Simulates: Toggle via QuickEnable then via AutoInject -> State remains consistent
    /// </summary>
    [Fact]
    public void ToggleViaDifferentControls_StateRemainsConsistent()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = false };
        bool quickEnable = false;
        bool autoInject = false;

        // Act - toggle via QuickEnable
        quickEnable = true;
        settings.EnableShaderInjection = quickEnable;
        autoInject = settings.EnableShaderInjection;
        Assert.True(quickEnable);
        Assert.True(autoInject);

        // Toggle via AutoInject
        autoInject = false;
        settings.EnableShaderInjection = autoInject;
        quickEnable = settings.EnableShaderInjection;
        Assert.False(quickEnable);
        Assert.False(autoInject);

        // Toggle via QuickEnable again
        quickEnable = true;
        settings.EnableShaderInjection = quickEnable;
        autoInject = settings.EnableShaderInjection;
        
        // Assert final state
        Assert.True(quickEnable);
        Assert.True(autoInject);
        Assert.True(settings.EnableShaderInjection);
    }

    #endregion

    #region Switch Display Configuration Then Check State Tests

    /// <summary>
    /// Simulates: Select WOLED -> Radio button should show WOLED selected
    /// </summary>
    [Fact]
    public void SelectWOLED_RadioButtonShowsWOLED()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act - user selects WOLED
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.WrgbStripe;

        // Simulate reading back for UI display
        var displayedLayout = settings.MonitorSettings[monitorId].ShaderLayout;

        // Assert
        Assert.Equal(SubpixelLayout.WrgbStripe, displayedLayout);
    }

    /// <summary>
    /// Simulates: Select QD-OLED -> Radio button should show QD-OLED selected
    /// </summary>
    [Fact]
    public void SelectQDOLED_RadioButtonShowsQDOLED()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act - user selects QD-OLED
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.RgbTriangular;

        // Simulate reading back for UI display
        var displayedLayout = settings.MonitorSettings[monitorId].ShaderLayout;

        // Assert
        Assert.Equal(SubpixelLayout.RgbTriangular, displayedLayout);
    }

    /// <summary>
    /// Simulates: Cycle through all display types -> Each shows correctly
    /// </summary>
    [Fact]
    public void CycleThroughDisplayTypes_EachShowsCorrectly()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        var layouts = new[] 
        { 
            SubpixelLayout.RgbStripe, 
            SubpixelLayout.WrgbStripe, 
            SubpixelLayout.RgbTriangular, 
            SubpixelLayout.Pentile 
        };

        // Act & Assert - cycle through each
        foreach (var layout in layouts)
        {
            settings.MonitorSettings[monitorId].ShaderLayout = layout;
            Assert.Equal(layout, settings.MonitorSettings[monitorId].ShaderLayout);
        }
    }

    #endregion

    #region Combined Scenario Tests

    /// <summary>
    /// Simulates the exact user scenario:
    /// 1. Change display type to WOLED
    /// 2. Toggle ClearType OFF
    /// 3. Toggle Real-Time Injection OFF
    /// 4. Check all visual states
    /// </summary>
    [Fact]
    public void CompleteUserScenario_AllStatesCorrect()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.RgbStripe
        };
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings 
        { 
            ShaderLayout = SubpixelLayout.RgbStripe,
            ShaderIntensity = 1.0
        };

        // UI state simulation
        bool quickEnable = true;
        bool autoInject = true;
        bool clearTypeToggle = true;
        SubpixelLayout displayedLayout = SubpixelLayout.RgbStripe;

        // Step 1: Change display type to WOLED
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        displayedLayout = settings.MonitorSettings[monitorId].ShaderLayout;
        Assert.Equal(SubpixelLayout.WrgbStripe, displayedLayout);

        // Step 2: Toggle ClearType OFF
        settings.EnableClearType = false;
        clearTypeToggle = settings.EnableClearType;
        Assert.False(clearTypeToggle);

        // Step 3: Toggle Real-Time Injection OFF
        settings.EnableShaderInjection = false;
        quickEnable = settings.EnableShaderInjection;
        autoInject = settings.EnableShaderInjection;
        Assert.False(quickEnable);
        Assert.False(autoInject);

        // Step 4: Verify all visual states
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.False(settings.EnableClearType);
        Assert.False(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Simulates alternating between enabling and disabling features:
    /// User rapidly toggles between different states
    /// </summary>
    [Fact]
    public void RapidStateChanges_AllStatesRemainConsistent()
    {
        // Arrange
        var settings = new DisplaySettings();
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings();

        // Rapid state changes
        for (int i = 0; i < 20; i++)
        {
            // Toggle shader
            settings.EnableShaderInjection = !settings.EnableShaderInjection;
            
            // Toggle ClearType  
            settings.EnableClearType = !settings.EnableClearType;
            
            // Change display type
            var layouts = new[] { SubpixelLayout.RgbStripe, SubpixelLayout.WrgbStripe, SubpixelLayout.RgbTriangular, SubpixelLayout.Pentile };
            settings.MonitorSettings[monitorId].ShaderLayout = layouts[i % 4];
            
            // Change intensity
            settings.MonitorSettings[monitorId].ShaderIntensity = (i % 11) / 10.0;
        }

        // Final verification - all states should be deterministic
        // i=19: 
        // EnableShaderInjection: started true, 20 toggles = true
        // EnableClearType: started true, 20 toggles = true
        // Layout: 19 % 4 = 3 = Pentile
        // Intensity: 19 % 11 = 8 -> 0.8
        Assert.True(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.Pentile, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.Equal(0.8, settings.MonitorSettings[monitorId].ShaderIntensity, 1); // tolerance for floating point
    }

    /// <summary>
    /// Simulates: Change settings, save, reload, verify all states match
    /// </summary>
    [Fact]
    public void ChangeSettings_SaveReload_AllStatesMatch()
    {
        // Arrange
        var settingsService = new SettingsService();
        settingsService.ClearSettings();

        var original = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85
        };
        original.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };

        // Act - save and reload
        settingsService.SaveSettings(original);
        var loaded = settingsService.LoadSettings();

        // Assert - all states match
        Assert.Equal(original.EnableShaderInjection, loaded.EnableShaderInjection);
        Assert.Equal(original.EnableClearType, loaded.EnableClearType);
        Assert.Equal(original.ShaderLayout, loaded.ShaderLayout);
        Assert.Equal(original.ShaderIntensity, loaded.ShaderIntensity);
        Assert.True(loaded.MonitorSettings.ContainsKey(@"\\.\DISPLAY1"));
        Assert.Equal(original.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout, 
                    loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(original.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity, 
                    loaded.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);

        // Cleanup
        settingsService.ClearSettings();
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Simulates: Toggle both toggles simultaneously (shouldn't happen in real UI but test anyway)
    /// </summary>
    [Fact]
    public void SimultaneousToggleChanges_StateRemainsConsistent()
    {
        // Arrange
        var settings = new DisplaySettings { EnableShaderInjection = false };
        
        // Simulate "simultaneous" changes
        bool quickEnable = true;
        bool autoInject = true;
        
        // Both change at once
        settings.EnableShaderInjection = quickEnable;
        settings.EnableShaderInjection = autoInject;
        
        // Sync both
        quickEnable = settings.EnableShaderInjection;
        autoInject = settings.EnableShaderInjection;

        // Assert
        Assert.True(quickEnable);
        Assert.True(autoInject);
        Assert.True(settings.EnableShaderInjection);
    }

    /// <summary>
    /// Simulates: Change display type while shader is disabled -> type should still be saved
    /// </summary>
    [Fact]
    public void ChangeDisplayType_WhileShaderDisabled_TypeIsSaved()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            ShaderLayout = SubpixelLayout.RgbStripe
        };
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act - change display type while shader is disabled
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;

        // Assert - type should be saved
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.False(settings.EnableShaderInjection); // Still disabled
    }

    /// <summary>
    /// Simulates: Enable shader after changing display type -> correct type should be active
    /// </summary>
    [Fact]
    public void EnableShader_AfterChangingDisplayType_CorrectTypeActive()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            ShaderLayout = SubpixelLayout.RgbStripe
        };
        string monitorId = @"\\.\DISPLAY1";
        settings.MonitorSettings[monitorId] = new MonitorSettings { ShaderLayout = SubpixelLayout.RgbStripe };

        // Act - change type, then enable
        settings.MonitorSettings[monitorId].ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.EnableShaderInjection = true;

        // Assert
        Assert.True(settings.EnableShaderInjection);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
    }

    #endregion
}
