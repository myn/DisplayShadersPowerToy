# Developer Guide - Display Shaders PowerToy

## Project Overview

Display Shaders PowerToy is a .NET 8 WPF application that addresses text rendering issues on OLED displays with non-standard subpixel layouts.

### Architecture

```
DisplayShadersPowerToy/
??? Models/
?   ??? DisplaySettings.cs      # Settings data model
?   ??? SubpixelLayout.cs       # Enum for subpixel types
??? Services/
?   ??? DisplayShaderService.cs # Core ClearType manipulation
?   ??? SettingsService.cs      # Settings persistence
??? MainWindow.xaml             # Main UI
??? MainWindow.xaml.cs          # Main UI logic
??? App.xaml                    # Application definition
??? App.xaml.cs                 # Application startup logic
```

## Building the Project

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Windows 10/11

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run the application
dotnet run

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
```

### Dependencies

- **Hardcodet.NotifyIcon.Wpf** (2.0.1) - System tray icon support
- **.NET 8.0 Windows Desktop Runtime** - WPF and Windows Forms support

## Code Structure

### Models

#### DisplaySettings.cs
Stores user preferences:
```csharp
public class DisplaySettings
{
    public SubpixelLayout SubpixelLayout { get; set; }
    public bool EnableShader { get; set; }
    public double ShaderIntensity { get; set; }
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; }
}
```

#### SubpixelLayout.cs
Enum defining supported layouts:
```csharp
public enum SubpixelLayout
{
    RgbStripe,      // Standard LCD
    WrgbStripe,     // WOLED
    RgbTriangular,  // QD-OLED
    Pentile,        // AMOLED
    None            // Disabled
}
```

### Services

#### DisplayShaderService.cs
**Purpose:** Manages Windows ClearType settings

**Key Methods:**
- `ApplyShaderSettings(DisplaySettings)` - Apply settings to Windows
- `ApplyRgbStripeSettings()` - Standard LCD settings
- `ApplyWrgbStripeSettings()` - WOLED optimizations
- `ApplyRgbTriangularSettings()` - QD-OLED optimizations
- `ApplyPentileSettings()` - PenTile optimizations
- `DisableClearType()` - Turn off ClearType

**Windows APIs Used:**
- `SystemParametersInfo` - Apply system-wide font settings
- Registry keys in `HKCU\Control Panel\Desktop`

**ClearType Parameters:**
| Parameter | RGB Stripe | WRGB Stripe | RGB Triangular | PenTile |
|-----------|------------|-------------|----------------|---------|
| Contrast  | 1400       | 800         | 600            | 700     |
| Gamma     | Default    | 1200        | 1000           | 1100    |
| Orientation | RGB (1)  | RGB (1)     | RGB (1)        | RGB (1) |

#### SettingsService.cs
**Purpose:** Persist user settings in Windows Registry

**Key Methods:**
- `SaveSettings(DisplaySettings)` - Save to registry
- `LoadSettings()` - Load from registry
- `SetStartWithWindows(bool)` - Manage startup
- `IsStartWithWindowsEnabled()` - Check startup status

**Registry Locations:**
- Settings: `HKCU\SOFTWARE\DisplayShadersPowerToy`
- Startup: `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`

### UI Components

#### MainWindow.xaml
**Sections:**
1. Header - Title and description
2. Subpixel Layout - Radio buttons for selection
3. Shader Settings - Enable toggle and intensity slider
4. Application Settings - Startup and tray options
5. Action Buttons - Apply and Close

**Key UI Elements:**
- Radio buttons for mutually exclusive layout selection
- Slider with percentage display for intensity
- Checkboxes for boolean settings
- Styled GroupBox containers

#### MainWindow.xaml.cs
**Event Handlers:**
- `SubpixelLayout_Changed` - Update settings when layout selected
- `EnableShader_Changed` - Toggle shader on/off
- `ShaderIntensity_Changed` - Update intensity value
- `Apply_Click` - Save and apply all settings
- `Window_Closing` - Handle minimize to tray
- `Window_StateChanged` - Hide when minimized

**System Tray:**
- NotifyIcon with context menu
- Double-click to restore window
- Right-click menu for Open/Exit

## Windows API Integration

### SystemParametersInfo

Used to modify system-wide font rendering:

```csharp
[DllImport("user32.dll", SetLastError = true)]
private static extern bool SystemParametersInfo(
    uint uiAction,    // Action to perform
    uint uiParam,     // Parameter
    IntPtr pvParam,   // Additional data
    uint fWinIni      // Update flags
);
```

**Actions Used:**
- `SPI_SETFONTSMOOTHING` (0x004B) - Enable/disable ClearType
- `SPI_SETFONTSMOOTHINGTYPE` (0x200B) - Set anti-aliasing type
- `SPI_SETFONTSMOOTHINGORIENTATION` (0x2013) - RGB vs BGR
- `SPI_SETFONTSMOOTHINGCONTRAST` (0x200D) - Contrast level

**Flags:**
- `SPIF_UPDATEINIFILE` (0x01) - Write to registry
- `SPIF_SENDCHANGE` (0x02) - Broadcast WM_SETTINGCHANGE

### Registry Keys

**ClearType Settings:**
```
HKCU\Control Panel\Desktop\
??? FontSmoothing (String)          "0" | "2"
??? FontSmoothingType (DWORD)       0 | 2
??? FontSmoothingOrientation (DWORD) 0 | 1
??? FontSmoothingGamma (DWORD)      800-1400
```

**Application Settings:**
```
HKCU\SOFTWARE\DisplayShadersPowerToy\
??? SubpixelLayout (DWORD)    0-4
??? EnableShader (DWORD)      0 | 1
??? ShaderIntensity (String)  0.0-1.0
??? StartWithWindows (DWORD)  0 | 1
??? MinimizeToTray (DWORD)    0 | 1
```

## Contributing

### Setting Up Development Environment

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/DisplayShadersPowerToy.git
cd DisplayShadersPowerToy
```

2. **Open in Visual Studio 2022**
- File ? Open ? Project/Solution
- Select `DisplayShadersPowerToy.csproj`

3. **Restore NuGet packages**
```bash
dotnet restore
```

4. **Build and run**
- Press F5 to debug
- Or `dotnet run` from command line

### Code Style Guidelines

**C# Conventions:**
- Use PascalCase for public members
- Use camelCase for private fields with underscore prefix (`_field`)
- Use explicit access modifiers
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose

**XAML Conventions:**
- Use x:Name for controls that are accessed in code-behind
- Group related UI elements in GroupBox
- Use consistent spacing and indentation
- Define reusable styles in Window.Resources

**Example:**
```csharp
/// <summary>
/// Applies shader settings to the display
/// </summary>
/// <param name="settings">Settings to apply</param>
public void ApplyShaderSettings(DisplaySettings settings)
{
    // Implementation
}
```

### Adding New Features

#### Example: Adding a new subpixel layout

1. **Update SubpixelLayout.cs**
```csharp
public enum SubpixelLayout
{
    // ...existing...
    NewLayoutType
}
```

2. **Add to DisplayShaderService.cs**
```csharp
case SubpixelLayout.NewLayoutType:
    ApplyNewLayoutSettings(settings);
    break;

private void ApplyNewLayoutSettings(DisplaySettings settings)
{
    SetClearTypeEnabled(true);
    SetClearTypeContrast((uint)(customValue * settings.ShaderIntensity));
    // Additional settings...
}
```

3. **Add UI in MainWindow.xaml**
```xml
<RadioButton x:Name="rbNewLayout"
             Content="New Layout Type"
             GroupName="SubpixelLayout"
             Checked="SubpixelLayout_Changed"/>
```

4. **Update MainWindow.xaml.cs**
```csharp
private void SubpixelLayout_Changed(object sender, RoutedEventArgs e)
{
    // ...existing cases...
    else if (rbNewLayout.IsChecked == true)
        _currentSettings.SubpixelLayout = SubpixelLayout.NewLayoutType;
}

private void InitializeUIFromSettings()
{
    // ...existing cases...
    case SubpixelLayout.NewLayoutType:
        rbNewLayout.IsChecked = true;
        break;
}
```

### Testing

#### Manual Testing Checklist

- [ ] All subpixel layouts apply correctly
- [ ] Shader intensity slider works (0-100%)
- [ ] Enable/disable shader toggles correctly
- [ ] Start with Windows registers in registry
- [ ] Minimize to tray hides window
- [ ] System tray icon shows and works
- [ ] Double-click tray icon restores window
- [ ] Settings persist after restart
- [ ] Apply button saves settings
- [ ] Close button respects minimize to tray setting
- [ ] Text rendering changes are visible
- [ ] Application starts minimized with --minimized flag

#### Test Displays

If possible, test on:
- Standard LCD monitor
- LG WOLED display
- Samsung QD-OLED display
- High DPI display (150%, 200% scaling)

#### Registry Testing

Before release:
```bash
# Backup current settings
reg export "HKCU\Control Panel\Desktop" desktop_backup.reg

# Test application

# Restore if needed
reg import desktop_backup.reg
```

### Debugging

**Enable diagnostic output:**
```csharp
System.Diagnostics.Debug.WriteLine($"Applying {settings.SubpixelLayout}");
```

**View in Visual Studio:**
- Debug ? Windows ? Output
- Shows Debug.WriteLine messages

**Common issues:**
- **Registry access denied:** Run Visual Studio as Administrator
- **Settings not applying:** Check if another app is overriding ClearType
- **UI not updating:** Verify event handlers are connected in XAML

### Performance Profiling

**Startup Performance:**
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// Code to measure
sw.Stop();
Debug.WriteLine($"Operation took {sw.ElapsedMilliseconds}ms");
```

**Memory Usage:**
- Use Visual Studio Diagnostic Tools
- Target: < 50 MB working set
- No memory leaks on window open/close cycles

### Building Installer

**Using Advanced Installer or WiX:**
1. Create new installer project
2. Add output files from `bin\Release\net8.0-windows\publish`
3. Include .NET 8 Desktop Runtime prerequisite
4. Add registry entries for settings
5. Add shortcuts (Start Menu, Desktop optional)
6. Set application icon

**Or use ClickOnce:**
```bash
dotnet publish -c Release -r win-x64 \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  --self-contained
```

## Future Enhancements

### Planned Features

1. **Per-Monitor Configuration**
   - Detect connected displays
   - Store settings per display
   - Auto-switch when focus changes

2. **Display Auto-Detection**
   - Query monitor EDID data
   - Identify manufacturer and model
   - Auto-select subpixel layout

3. **Advanced Shader Options**
   - Custom gamma curves
   - Per-color channel adjustments
   - DirectX overlay for real-time shaders

4. **UI Improvements**
   - Preview pane with sample text
   - Before/after comparison
   - Wizard for first-time setup

5. **Settings Profiles**
   - Save multiple configurations
   - Quick-switch between profiles
   - Import/export settings

### Contributing Guidelines

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

**PR Requirements:**
- Describe what the change does
- Include testing steps
- Update CHANGELOG.md
- Follow existing code style
- No breaking changes without discussion

### Community

- **Issues:** Report bugs or request features on GitHub
- **Discussions:** Ask questions or share configurations
- **Pull Requests:** Contribute code improvements

## License

This project is licensed under the MIT License - see LICENSE file for details.

## Credits

- Inspired by PowerToys Issue #25595
- Based on Blur Busters Display Shaders specification
- Community feedback from OLED display users
- Windows ClearType research and documentation

## Resources

**Windows ClearType:**
- [ClearType Text Tuner](https://docs.microsoft.com/en-us/typography/cleartype/)
- [SystemParametersInfo API](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow)

**Display Technology:**
- [Blur Busters Display Motion Blur](https://blurbusters.com)
- [WOLED vs QD-OLED Comparison](https://tftcentral.co.uk)

**Development:**
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [WPF Tutorial](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
