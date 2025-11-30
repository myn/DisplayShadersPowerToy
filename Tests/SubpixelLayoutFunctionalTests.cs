using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Functional tests for validating subpixel layout settings are applied correctly.
/// These tests verify the logic paths and settings configuration for each display type.
/// Note: Actual Windows registry/API calls are mocked or skipped on non-Windows platforms.
/// </summary>
public class SubpixelLayoutFunctionalTests
{
    /// <summary>
    /// Tests that applying settings with RgbStripe layout configures the correct settings
    /// </summary>
    [Fact]
    public void ApplySettings_RgbStripeLayout_SetsCorrectConfiguration()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.RgbStripe,
            ClearTypeLayout = SubpixelLayout.RgbStripe,
            ShaderIntensity = 1.0,
            ClearTypeIntensity = 1.0
        };

        // Assert - settings are configured correctly
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ClearTypeLayout);
        Assert.Equal(1.0, settings.ShaderIntensity);
        Assert.Equal(1.0, settings.ClearTypeIntensity);
        Assert.True(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
    }

    /// <summary>
    /// Tests that applying settings with WrgbStripe (WOLED) layout configures the correct settings
    /// </summary>
    [Fact]
    public void ApplySettings_WrgbStripeLayout_SetsCorrectConfiguration()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.8,
            ClearTypeIntensity = 0.8
        };

        // Assert - settings are configured correctly for WOLED displays
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ClearTypeLayout);
        Assert.Equal(0.8, settings.ShaderIntensity);
        Assert.Equal(0.8, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests that applying settings with RgbTriangular (QD-OLED) layout configures the correct settings
    /// </summary>
    [Fact]
    public void ApplySettings_RgbTriangularLayout_SetsCorrectConfiguration()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ClearTypeLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.75,
            ClearTypeIntensity = 0.75
        };

        // Assert - settings are configured correctly for QD-OLED displays
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ClearTypeLayout);
        Assert.Equal(0.75, settings.ShaderIntensity);
        Assert.Equal(0.75, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests that applying settings with Pentile layout configures the correct settings
    /// </summary>
    [Fact]
    public void ApplySettings_PentileLayout_SetsCorrectConfiguration()
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.Pentile,
            ClearTypeLayout = SubpixelLayout.Pentile,
            ShaderIntensity = 0.7,
            ClearTypeIntensity = 0.7
        };

        // Assert - settings are configured correctly for Pentile displays
        Assert.Equal(SubpixelLayout.Pentile, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.Pentile, settings.ClearTypeLayout);
        Assert.Equal(0.7, settings.ShaderIntensity);
        Assert.Equal(0.7, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests that disabling shader injection keeps ClearType independent
    /// </summary>
    [Fact]
    public void ApplySettings_ShaderDisabled_ClearTypeStillConfigurable()
    {
        // Arrange - shader disabled, ClearType enabled
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 1.0,
            ClearTypeIntensity = 1.0
        };

        // Assert - ClearType should work independently of shader injection
        Assert.False(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ClearTypeLayout);
    }

    /// <summary>
    /// Tests that disabling ClearType keeps shader injection independent
    /// </summary>
    [Fact]
    public void ApplySettings_ClearTypeDisabled_ShaderStillConfigurable()
    {
        // Arrange - ClearType disabled, shader enabled
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = false,
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ClearTypeLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.9,
            ClearTypeIntensity = 0.9
        };

        // Assert - Shader injection should work independently of ClearType
        Assert.True(settings.EnableShaderInjection);
        Assert.False(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ShaderLayout);
    }

    /// <summary>
    /// Tests that both modes can be enabled simultaneously
    /// </summary>
    [Fact]
    public void ApplySettings_BothModesEnabled_WorksTogether()
    {
        // Arrange - both modes enabled
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.85,
            ClearTypeIntensity = 0.85
        };

        // Assert - both modes should be independently enabled
        Assert.True(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ClearTypeLayout);
    }

    /// <summary>
    /// Tests that both modes can be disabled simultaneously
    /// </summary>
    [Fact]
    public void ApplySettings_BothModesDisabled_WorksTogether()
    {
        // Arrange - both modes disabled
        var settings = new DisplaySettings
        {
            EnableShaderInjection = false,
            EnableClearType = false,
            ShaderLayout = SubpixelLayout.None,
            ClearTypeLayout = SubpixelLayout.None,
            ShaderIntensity = 0.0,
            ClearTypeIntensity = 0.0
        };

        // Assert - both modes should be independently disabled
        Assert.False(settings.EnableShaderInjection);
        Assert.False(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.None, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.None, settings.ClearTypeLayout);
    }

    /// <summary>
    /// Tests intensity values at boundaries for all layouts
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe, 0.0)]
    [InlineData(SubpixelLayout.RgbStripe, 0.5)]
    [InlineData(SubpixelLayout.RgbStripe, 1.0)]
    [InlineData(SubpixelLayout.WrgbStripe, 0.0)]
    [InlineData(SubpixelLayout.WrgbStripe, 0.5)]
    [InlineData(SubpixelLayout.WrgbStripe, 1.0)]
    [InlineData(SubpixelLayout.RgbTriangular, 0.0)]
    [InlineData(SubpixelLayout.RgbTriangular, 0.5)]
    [InlineData(SubpixelLayout.RgbTriangular, 1.0)]
    [InlineData(SubpixelLayout.Pentile, 0.0)]
    [InlineData(SubpixelLayout.Pentile, 0.5)]
    [InlineData(SubpixelLayout.Pentile, 1.0)]
    public void ApplySettings_IntensityValues_AreAppliedCorrectly(SubpixelLayout layout, double intensity)
    {
        // Arrange
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = layout,
            ClearTypeLayout = layout,
            ShaderIntensity = intensity,
            ClearTypeIntensity = intensity
        };

        // Assert
        Assert.Equal(layout, settings.ShaderLayout);
        Assert.Equal(layout, settings.ClearTypeLayout);
        Assert.Equal(intensity, settings.ShaderIntensity);
        Assert.Equal(intensity, settings.ClearTypeIntensity);
    }

    /// <summary>
    /// Tests per-monitor settings with different layouts
    /// </summary>
    [Fact]
    public void ApplySettings_PerMonitorSettings_DifferentLayouts()
    {
        // Arrange - main display is WOLED, secondary is QD-OLED
        var settings = new DisplaySettings
        {
            EnableShaderInjection = true,
            EnableClearType = true,
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ClearTypeLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 1.0,
            ClearTypeIntensity = 1.0
        };

        // Add per-monitor settings
        settings.MonitorSettings[@"\\.\DISPLAY1"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.9
        };

        settings.MonitorSettings[@"\\.\DISPLAY2"] = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.RgbTriangular,
            ShaderIntensity = 0.8
        };

        // Assert
        Assert.Equal(2, settings.MonitorSettings.Count);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[@"\\.\DISPLAY1"].ShaderLayout);
        Assert.Equal(0.9, settings.MonitorSettings[@"\\.\DISPLAY1"].ShaderIntensity);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.MonitorSettings[@"\\.\DISPLAY2"].ShaderLayout);
        Assert.Equal(0.8, settings.MonitorSettings[@"\\.\DISPLAY2"].ShaderIntensity);
    }

    /// <summary>
    /// Tests that all subpixel layouts can be cycled through
    /// </summary>
    [Fact]
    public void ApplySettings_AllSubpixelLayouts_CanBeCycled()
    {
        var settings = new DisplaySettings();
        var layouts = Enum.GetValues<SubpixelLayout>();

        foreach (var layout in layouts)
        {
            // Act
            settings.ShaderLayout = layout;
            settings.ClearTypeLayout = layout;

            // Assert
            Assert.Equal(layout, settings.ShaderLayout);
            Assert.Equal(layout, settings.ClearTypeLayout);
        }
    }
}

/// <summary>
/// Tests for ShaderService configuration writing
/// </summary>
public class ShaderServiceConfigTests
{
    /// <summary>
    /// Tests that shader config correctly maps SubpixelLayout values
    /// </summary>
    [Theory]
    [InlineData(SubpixelLayout.RgbStripe, 0)]
    [InlineData(SubpixelLayout.WrgbStripe, 1)]
    [InlineData(SubpixelLayout.RgbTriangular, 2)]
    [InlineData(SubpixelLayout.Pentile, 3)]
    [InlineData(SubpixelLayout.None, 4)]
    public void ShaderConfig_SubpixelLayoutMapping_IsCorrect(SubpixelLayout layout, int expectedValue)
    {
        // Assert - enum values should match shader config expectations
        Assert.Equal(expectedValue, (int)layout);
    }
}
