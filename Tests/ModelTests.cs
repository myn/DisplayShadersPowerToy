using DisplayShadersPowerToy.Models;

namespace DisplayShadersPowerToy.Tests;

/// <summary>
/// Tests for DisplaySettings model
/// </summary>
public class DisplaySettingsTests
{
    [Fact]
    public void DisplaySettings_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var settings = new DisplaySettings();

        // Assert
        Assert.True(settings.EnableShaderInjection);
        Assert.True(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ClearTypeLayout);
        Assert.Equal(1.0, settings.ShaderIntensity);
        Assert.Equal(1.0, settings.ClearTypeIntensity);
        Assert.False(settings.StartWithWindows);
        Assert.False(settings.MinimizeToTray);
        Assert.NotNull(settings.MonitorSettings);
        Assert.Empty(settings.MonitorSettings);
    }

    [Fact]
    public void DisplaySettings_CanSetAllProperties()
    {
        // Arrange
        var settings = new DisplaySettings();

        // Act
        settings.EnableShaderInjection = false;
        settings.EnableClearType = false;
        settings.ShaderLayout = SubpixelLayout.WrgbStripe;
        settings.ClearTypeLayout = SubpixelLayout.RgbTriangular;
        settings.ShaderIntensity = 0.5;
        settings.ClearTypeIntensity = 0.75;
        settings.StartWithWindows = true;
        settings.MinimizeToTray = true;

        // Assert
        Assert.False(settings.EnableShaderInjection);
        Assert.False(settings.EnableClearType);
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.ShaderLayout);
        Assert.Equal(SubpixelLayout.RgbTriangular, settings.ClearTypeLayout);
        Assert.Equal(0.5, settings.ShaderIntensity);
        Assert.Equal(0.75, settings.ClearTypeIntensity);
        Assert.True(settings.StartWithWindows);
        Assert.True(settings.MinimizeToTray);
    }

    [Fact]
    public void DisplaySettings_MonitorSettings_CanBeAddedAndRetrieved()
    {
        // Arrange
        var settings = new DisplaySettings();
        var monitorId = @"\\.\DISPLAY1";
        var monitorSettings = new MonitorSettings
        {
            ShaderLayout = SubpixelLayout.WrgbStripe,
            ShaderIntensity = 0.8
        };

        // Act
        settings.MonitorSettings[monitorId] = monitorSettings;

        // Assert
        Assert.True(settings.MonitorSettings.ContainsKey(monitorId));
        Assert.Equal(SubpixelLayout.WrgbStripe, settings.MonitorSettings[monitorId].ShaderLayout);
        Assert.Equal(0.8, settings.MonitorSettings[monitorId].ShaderIntensity);
    }
}

/// <summary>
/// Tests for MonitorSettings model
/// </summary>
public class MonitorSettingsTests
{
    [Fact]
    public void MonitorSettings_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var settings = new MonitorSettings();

        // Assert
        Assert.Equal(SubpixelLayout.RgbStripe, settings.ShaderLayout);
        Assert.Equal(1.0, settings.ShaderIntensity);
    }

    [Theory]
    [InlineData(SubpixelLayout.RgbStripe, 1.0)]
    [InlineData(SubpixelLayout.WrgbStripe, 0.5)]
    [InlineData(SubpixelLayout.RgbTriangular, 0.75)]
    [InlineData(SubpixelLayout.Pentile, 0.25)]
    [InlineData(SubpixelLayout.None, 0.0)]
    public void MonitorSettings_CanSetAllLayoutsAndIntensities(SubpixelLayout layout, double intensity)
    {
        // Arrange
        var settings = new MonitorSettings();

        // Act
        settings.ShaderLayout = layout;
        settings.ShaderIntensity = intensity;

        // Assert
        Assert.Equal(layout, settings.ShaderLayout);
        Assert.Equal(intensity, settings.ShaderIntensity);
    }
}

/// <summary>
/// Tests for SubpixelLayout enum
/// </summary>
public class SubpixelLayoutTests
{
    [Fact]
    public void SubpixelLayout_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)SubpixelLayout.RgbStripe);
        Assert.Equal(1, (int)SubpixelLayout.WrgbStripe);
        Assert.Equal(2, (int)SubpixelLayout.RgbTriangular);
        Assert.Equal(3, (int)SubpixelLayout.Pentile);
        Assert.Equal(4, (int)SubpixelLayout.None);
    }

    [Fact]
    public void SubpixelLayout_AllValuesAreDefined()
    {
        // Arrange
        var expectedLayouts = new[] 
        { 
            SubpixelLayout.RgbStripe, 
            SubpixelLayout.WrgbStripe, 
            SubpixelLayout.RgbTriangular, 
            SubpixelLayout.Pentile, 
            SubpixelLayout.None 
        };

        // Act
        var definedValues = Enum.GetValues<SubpixelLayout>();

        // Assert
        Assert.Equal(expectedLayouts.Length, definedValues.Length);
        foreach (var expected in expectedLayouts)
        {
            Assert.Contains(expected, definedValues);
        }
    }
}
