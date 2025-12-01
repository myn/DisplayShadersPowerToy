# Developer Guide - Display Shaders PowerToy

## Project Overview

Display Shaders PowerToy is a .NET 8 WPF application that fixes text rendering issues on OLED displays using real-time DirectWrite shader injection.

### Architecture

```
DisplayShadersPowerToy/
?? Models/
?  ?? DisplaySettings.cs      # Settings data model
?  ?? SubpixelLayout.cs       # Enum for subpixel types
?? Services/
?  ?? DisplayShaderService.cs # Shader injection management
?  ?? ShaderService.cs        # Shader configuration
?  ?? InjectionManager.cs     # Process injection
?  ?? SettingsService.cs      # Settings persistence
?? Native/DisplayShaderHook/  # C++ Hook DLL
?  ?? dllmain.cpp            # DLL entry point
?  ?? DirectWriteHook.cpp    # DirectWrite API hooks
?  ?? SubpixelShader.cpp     # HLSL shader implementation
?  ?? ConfigLoader.cpp       # Configuration loading
?? MainWindow.xaml           # Main UI
?? MainWindow.xaml.cs        # Main UI logic
?? App.xaml                  # Application definition
?? App.xaml.cs               # Application startup logic
```

## Building the Project

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (with C++ and .NET workloads)
- Windows 10/11 SDK

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build C++ hook DLL
msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release

# Build C# application
dotnet build -c Release

# Run the application
dotnet run

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
```

### Dependencies

**C# Application:**
- **Hardcodet.NotifyIcon.Wpf** (2.0.1) - System tray icon support
- **.NET 8.0 Windows Desktop Runtime** - WPF support

**C++ Hook DLL:**
- **MinHook** - API hooking library
- **Windows SDK** - DirectWrite and D3D11 headers

## Code Structure

### Models

#### DisplaySettings.cs
Stores user preferences:
```csharp
public class DisplaySettings
{
    public bool EnableShaderInjection { get; set; } = true;
    public SubpixelLayout ShaderLayout { get; set; } = SubpixelLayout.RgbStripe;
    public double ShaderIntensity { get; set; } = 1.0;
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
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
    Pentile         // AMOLED
}
```

### Services

#### DisplayShaderService.cs
**Purpose:** Manages shader injection lifecycle

**Key Methods:**
- `ApplyShaderSettings(DisplaySettings)` - Apply shader configuration
- `EnableShaderInjection()` - Start injection
- `GetInjectedProcessCount()` - Count hooked processes
- `GetInjectedProcessNames()` - List hooked processes
- `IsShaderModeAvailable()` - Check if DLL is available

**Shader Pipeline:**
```
DisplayShaderService
    ?
ShaderService (config management)
    ?
InjectionManager (process hooking)
    ?
DisplayShaderHook.dll (DirectWrite hooks)
    ?
Custom HLSL Shaders
```

#### InjectionManager.cs
**Purpose:** Inject DLL into GUI processes

**Key Methods:**
- `StartContinuousMonitoring()` - Auto-inject into new processes
- `StopContinuousMonitoring()` - Stop monitoring
- `InjectIntoProcesses()` - Inject into all eligible processes
- `GetInjectedProcessCount()` - Count injected processes

**Process Filtering:**
- Only injects into GUI applications (MainWindowHandle != 0)
- Blacklists system processes, security software, anti-cheat
- Skips Session 0 processes (system services)

#### ShaderService.cs
**Purpose:** Manage shader configuration via shared memory

**Key Methods:**
- `Initialize()` - Create shared memory
- `UpdateShaderConfig(DisplaySettings)` - Write configuration
- `ReadCurrentConfig()` - Read configuration
- `IsHookDllAvailable()` - Check if DLL exists

**Shared Memory:**
- Named: "DisplayShadersConfig"
- Size: 256 bytes
- Contains: Layout type, intensity, enabled flag

### UI Components

#### MainWindow.xaml
**Sections:**
1. Header - Title and description
2. Status Card - Live process count and enable toggle
3. Display Configuration - Radio buttons for layout selection
4. Intensity Slider - Adjustable optimization strength
5. Advanced Options - Startup and tray settings
6. Diagnostic Tools - Log viewing
7. Active Processes - List of optimized applications

**Modern Design:**
- Card-based layout
- Toggle switches
- Real-time status updates
- Green/gray color scheme

#### MainWindow.xaml.cs
**Event Handlers:**
- `DisplayType_Changed` - Update shader layout
- `Intensity_Changed` - Update shader intensity
- `QuickEnable_Changed` - Toggle shader injection
- `AutoInject_Changed` - Toggle auto-injection
- `Window_Closing` - Handle minimize to tray
- `Window_StateChanged` - Hide when minimized

**Status Updates:**
- Timer updates every 2 seconds
- Shows process count
- Displays active/waiting/disabled state

## Native Hook DLL

### DirectWrite Hooking

**dllmain.cpp:**
```cpp
BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID reserved)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hModule);
        if (!InitializeHooks())
        {
            return FALSE;
        }
    }
    else if (reason == DLL_PROCESS_DETACH)
    {
        ShutdownHooks();
    }
    return TRUE;
}
```

**DirectWriteHook.cpp:**
- Hooks `IDWriteTextRenderer::DrawGlyphRun`
- Applies custom pixel shader
- Reads layout from shared memory
- Adjusts RGB values based on subpixel type

### HLSL Shaders

**SubpixelShader.cpp:**
```cpp
// Shader for WOLED (WRGB Stripe)
float4 ApplyWOLEDShader(float4 color, float2 texCoord)
{
    // Remap RGB to RBG (Blue in middle for WRGB)
    float r = color.r;
    float g = color.g;
    float b = color.b;
    
    // Apply subpixel-aware rendering
    return float4(r, b, g, color.a);
}
```

**Supported Shaders:**
- RGB Stripe (standard)
- WRGB Stripe (WOLED)
- RGB Triangular (QD-OLED)
- PenTile (AMOLED)

## Registry Integration

### Application Settings Only

The app stores settings in:
```
HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy\
?? EnableShaderInjection (DWORD) 0 | 1
?? ShaderLayout (DWORD)           0-3
?? ShaderIntensity (String)       0.0-1.0
?? StartWithWindows (DWORD)       0 | 1
?? MinimizeToTray (DWORD)         0 | 1
```

**Note:** This app does NOT modify Windows font rendering settings or ClearType registry keys.

## Contributing

### Setting Up Development Environment

1. **Clone the repository**
```bash
git clone https://github.com/myn/DisplayShadersPowerToy.git
cd DisplayShadersPowerToy
```

2. **Open in Visual Studio 2022**
- File ? Open ? Project/Solution
- Select `DisplayShadersPowerToy.sln`

3. **Build C++ Hook DLL first**
```bash
msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release
```

4. **Build C# application**
```bash
dotnet build -c Release
```

5. **Run**
- Press F5 to debug
- Or `dotnet run` from command line

### Code Style Guidelines

**C# Conventions:**
- Use PascalCase for public members
- Use camelCase for private fields with underscore prefix (`_field`)
- Use explicit access modifiers
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose

**C++ Conventions:**
- Use PascalCase for classes and functions
- Use camelCase for variables
- Use RAII for resource management
- Check return values from API calls

**XAML Conventions:**
- Use x:Name for controls accessed in code-behind
- Group related UI elements in cards/borders
- Use consistent spacing and indentation
- Define reusable styles in Window.Resources

### Adding New Features

#### Example: Adding a new subpixel layout

1. **Update SubpixelLayout.cs**
```csharp
public enum SubpixelLayout
{
    RgbStripe,
    WrgbStripe,
    RgbTriangular,
    Pentile,
    NewLayoutType  // Add new layout
}
```

2. **Add shader in SubpixelShader.cpp**
```cpp
float4 ApplyNewLayoutShader(float4 color, float2 texCoord, float intensity)
{
    // Custom shader logic
    return color;
}
```

3. **Update DirectWriteHook.cpp**
```cpp
case SubpixelLayout::NewLayoutType:
    finalColor = ApplyNewLayoutShader(color, texCoord, intensity);
    break;
```

4. **Add UI in MainWindow.xaml**
```xml
<RadioButton x:Name="rbNewLayout"
             Content="New Layout Type"
             GroupName="DisplayType"
             Checked="DisplayType_Changed">
    <RadioButton.Content>
        <StackPanel>
            <TextBlock Text="New Layout" FontWeight="SemiBold"/>
            <TextBlock Text="Description" 
                      FontSize="11" 
                      Foreground="{StaticResource TextSecondary}"/>
        </StackPanel>
    </RadioButton.Content>
</RadioButton>
```

5. **Update MainWindow.xaml.cs**
```csharp
private void DisplayType_Changed(object sender, RoutedEventArgs e)
{
    if (_isInitializing) return;

    if (rbNewLayout.IsChecked == true)
        _currentSettings.ShaderLayout = SubpixelLayout.NewLayoutType;
    
    ApplySettings();
}
```

### Testing

#### Manual Testing Checklist

**C# Application:**
- [ ] All subpixel layouts apply correctly
- [ ] Shader intensity slider works (0-100%)
- [ ] Enable/disable shader toggles correctly
- [ ] Start with Windows registers in registry
- [ ] Minimize to tray hides window
- [ ] System tray icon shows and works
- [ ] Double-click tray icon restores window
- [ ] Settings persist after restart
- [ ] Status updates show correct process count
- [ ] Active processes list displays

**C++ Hook DLL:**
- [ ] DLL loads into target processes
- [ ] DirectWrite hooks apply correctly
- [ ] Shaders execute without crashes
- [ ] Configuration reads from shared memory
- [ ] DLL unloads cleanly on exit

#### Test Displays

If possible, test on:
- Standard LCD monitor
- LG WOLED display
- Samsung QD-OLED display
- High DPI display (150%, 200% scaling)

### Debugging

**C# Application:**
```csharp
System.Diagnostics.Debug.WriteLine($"Applying {settings.ShaderLayout}");
```
View in Visual Studio: Debug ? Windows ? Output

**C++ Hook DLL:**
```cpp
OutputDebugStringA("Hook initialized\n");
```
View in DebugView or Visual Studio Output window

**Common Issues:**
- **DLL not loading:** Check that DisplayShaderHook.dll is in the same directory
- **Injection fails:** Some processes may block DLL injection (protected processes)
- **Shared memory error:** Ensure only one instance of app is running

### Performance Profiling

**C# Startup Performance:**
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// Code to measure
sw.Stop();
Debug.WriteLine($"Operation took {sw.ElapsedMilliseconds}ms");
```

**C++ Shader Performance:**
- Keep shader logic simple
- Avoid complex calculations in per-pixel code
- Profile with PIX or RenderDoc

### Building Release

**Complete Build:**
```bash
# Build C++ DLL
msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64

# Build C# app
dotnet publish -c Release -r win-x64 --self-contained

# Output in: bin\Release\net8.0-windows\win-x64\publish\
```

**Files needed for distribution:**
- DisplayShadersPowerToy.exe
- DisplayShaderHook.dll
- Runtime dependencies (if not self-contained)

## Architecture Decisions

### Why Shared Memory?

**Pros:**
- Fast communication between C# and C++
- No file I/O overhead
- Immediate configuration updates
- No race conditions with file locking

**Cons:**
- Requires careful synchronization
- Limited size (256 bytes)
- Must handle process crashes

### Why DLL Injection?

**Pros:**
- Access to DirectWrite rendering pipeline
- Can apply custom shaders
- Per-process optimization
- No system-wide changes

**Cons:**
- May be blocked by anti-virus
- Requires process elevation for some apps
- Anti-cheat systems may detect

### Why Not Kernel Driver?

**Pros of User-Mode:**
- No admin rights required
- Easier to develop and debug
- More compatible
- Safer (can't BSOD)

**Cons of User-Mode:**
- Cannot hook all processes
- Cannot modify display driver pipeline
- Limited to application-level

## Future Enhancements

### Planned Features

1. **Per-Monitor Configuration**
   - Detect connected displays via EDID
   - Store settings per monitor
   - Auto-switch when window moves

2. **Advanced Shader Options**
   - Custom gamma curves
   - Per-color channel adjustments
   - User-defined shader profiles

3. **UI Improvements**
   - Live preview of shader effect
   - Before/after comparison
   - Wizard for first-time setup

4. **Broader Compatibility**
   - Hook GDI+ rendering
   - Support Direct2D
   - Game mode detection

## Contributing Guidelines

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Make your changes** (C++ and/or C#)
4. **Test thoroughly** (both components)
5. **Update documentation**
6. **Submit a Pull Request**

**PR Requirements:**
- Describe what the change does
- Include testing steps
- Update CHANGELOG.md if user-facing
- Follow existing code style
- No breaking changes without discussion

## License

This project is licensed under the MIT License - see LICENSE file for details.

## Credits

- Inspired by PowerToys Issue #25595
- Based on Blur Busters Display Shaders specification
- MinHook library by Tsuda Kageyu
- Community feedback from OLED display users

## Resources

**DirectWrite:**
- [DirectWrite Documentation](https://docs.microsoft.com/en-us/windows/win32/directwrite/direct-write-portal)
- [Text Rendering](https://docs.microsoft.com/en-us/windows/win32/directwrite/text-rendering)

**HLSL Shaders:**
- [HLSL Reference](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl)
- [Shader Model 5](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-sm5)

**API Hooking:**
- [MinHook Documentation](https://github.com/TsudaKageyu/minhook)
- [DLL Injection Techniques](https://www.codeproject.com/Articles/4610/Three-Ways-to-Inject-Your-Code-into-Another-Proces)

**Display Technology:**
- [Blur Busters](https://blurbusters.com)
- [WOLED vs QD-OLED](https://tftcentral.co.uk)

**Development:**
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [WPF Tutorial](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [C++ Best Practices](https://isocpp.github.io/CppCoreGuidelines/CppCoreGuidelines)
