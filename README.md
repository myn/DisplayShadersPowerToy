# OLED Text Optimizer

**Crystal-clear text rendering for modern OLED displays through real-time shader injection**

![OLED Text Optimizer](Screenshot%202025-11-17%20135100.png)

## What is This?

OLED Text Optimizer is a Windows utility that fixes text rendering issues on modern OLED displays using **real-time DirectWrite shader injection** with custom HLSL pixel shaders.

### The Problem

Modern OLED displays use different subpixel layouts than traditional LCDs:
- **LG WOLED** (WRGB) - Four subpixels with white in center
- **Samsung QD-OLED** (RGB Triangular) - Triangular arrangement
- **PenTile AMOLED** - Diamond pattern

Windows ClearType was designed for standard RGB stripe LCDs, causing **color fringing** and **blurry text** on OLED displays.

### The Solution

This application uses **real-time DirectWrite shader injection** to apply custom subpixel rendering optimized for your specific display type.

**How it works:**
1. Hooks into DirectWrite rendering pipeline
2. Applies custom HLSL pixel shaders
3. Monitors and optimizes new processes automatically
4. Provides pixel-perfect subpixel-aware rendering

## Features

### Real-Time Shader Injection
- **Automatic process injection** - Hooks into applications as they launch
- **Live monitoring** - Shows currently optimized applications
- **Instant apply** - Changes take effect immediately
- **Safe and reversible** - Clean unhooking on exit

### Display Support
- **Standard LCD/LED** - Traditional RGB stripe
- **LG WOLED (WRGB)** - LG C/G/B series OLED displays
- **QD-OLED (RGB Triangular)** - Samsung/Alienware monitors
- **PenTile AMOLED** - Diamond pattern displays
- **Multi-Monitor Support** - Configure different settings for each display independently

### Advanced Features
- **Adjustable intensity** - Fine-tune optimization strength (0-100%)
- **Auto-start** - Launch with Windows
- **System tray** - Run silently in background
- **Diagnostic logs** - Detailed troubleshooting information

## Screenshot

The interface shows real-time status and easy configuration:

![OLED Text Optimizer Interface](Screenshot%202025-11-17%20135100.png)

**Key Elements:**
- **Green status indicator** - Shows active optimization
- **Live process count** - "Optimizing 6 applications"
- **Display type selection** - Choose your monitor
- **Optimization strength slider** - Adjust intensity
- **Active process list** - See what's being optimized

## Quick Start

### Installation

1. **Download** the latest release from [Releases](https://github.com/myn/DisplayShadersPowerToy/releases)
2. **Extract** the files to a folder (e.g., `C:\Program Files\OLED Text Optimizer\`)
3. **Run** `DisplayShadersPowerToy.exe`

**Important:** The `DisplayShaderHook.dll` file must be present in the same directory as the executable.

### First-Time Setup

1. **Select your display type:**
   - LG OLED monitor? **LG WOLED (WRGB)**
   - Samsung QD-OLED? **QD-OLED (RGB Triangular)**
   - Standard LCD? **Standard LCD / LED Monitor**

2. **Adjust optimization strength** (100% recommended to start)

3. **Toggle ON** the master switch (top-right)

4. **Watch the magic happen!** 
   - Status shows "Optimizing X applications"
   - Active processes list appears
   - Text rendering improves immediately

### Multi-Monitor Configuration

For setups with multiple displays (e.g., an OLED main monitor and LCD secondary monitors):

1. **Select a monitor** from the visual layout at the top of the window.
2. **Configure settings** for the selected monitor:
   - Set **Display Type** (e.g., WOLED for main, Standard LCD for secondary)
   - Adjust **Intensity**
3. **Repeat** for other monitors.
4. Use the **Identify** button to show numbers on your screens if you're unsure which is which.

### Recommended Settings

| Display Type | Setting | Intensity |
|-------------|---------|-----------|
| **LG WOLED** | LG WOLED (WRGB) | 85-100% |
| **Samsung QD-OLED** | QD-OLED (RGB Triangular) | 75-90% |
| **Standard LCD** | Standard LCD / LED | 100% |

## How It Works

### Architecture

```
+---------------------------------------------+
|          OLED Text Optimizer                |
+---------------------------------------------+
|                                             |
|  Real-Time Shader Injection                 |
|    - Hook DirectWrite API                   |
|    - Apply HLSL pixel shaders               |
|    - Monitor new processes                  |
|    - Auto-inject on launch                  |
|                                             |
|  Optimized for:                             |
|    - WOLED (WRGB stripe)                    |
|    - QD-OLED (RGB triangular)               |
|    - PenTile (diamond pattern)              |
|                                             |
+---------------------------------------------+
```

### What Gets Optimized?

**Automatically optimizes:**
- All GUI applications (Chrome, Firefox, Edge, etc.)
- Text editors (Notepad, VS Code, Visual Studio, etc.)
- Office apps (Word, Excel, PowerPoint, etc.)
- Communication apps (Slack, Teams, Discord, etc.)
- Any application using DirectWrite for text rendering

**Process filtering:**
- Skips system processes (safety)
- Skips security software
- Skips anti-cheat systems
- Only hooks GUI applications

## Status Indicators

### Main Status Card

```
? Optimizing 6 applications          [ENABLED]
  Real-time shader injection active
```

**Status Messages:**
- **"Optimizing X applications"** - Shader injection working
- **"Waiting for applications..."** - Ready, no apps yet
- **"Optimization disabled"** - Turned off
- **"Shader Mode: Not Available"** - DisplayShaderHook.dll not found

### Active Processes List

Shows live list of optimized applications:
```
chrome.exe (PID: 1234)
notepad.exe (PID: 5678)
devenv.exe (PID: 9012)
...
```

## Advanced Options

### Real-time Process Injection
- **Enabled** (default): Automatically hooks new applications
- **Disabled**: Manual mode only

### Launch at Startup
- **Enabled**: Starts with Windows
- **Disabled**: Manual launch required

### Minimize to System Tray
- **Enabled**: Hides to tray when minimized
- **Disabled**: Normal taskbar behavior

## Diagnostic Tools

### View Logs

Click **"View Logs"** to see detailed diagnostic information:
```
[22:45:01] [InjectionManager] Scanning 234 processes...
[22:45:02] [Injection] SUCCESS: chrome (PID: 1234)
[22:45:03] [Injection] SUCCESS: notepad (PID: 5678)
...
```

### Log File Location
```
%LOCALAPPDATA%\DisplayShadersPowerToy\Logs\diagnostic_YYYY-MM-DD.log
```

## Technical Details

### Components

**C# Application (`DisplayShadersPowerToy.exe`)**
- Modern WPF interface
- Process injection manager
- Settings management
- System tray integration

**C++ Hook DLL (`DisplayShaderHook.dll`)**
- DirectWrite API hooks
- HLSL pixel shader implementation
- Subpixel-aware rendering
- D3D11 device management

### Application Settings (Registry)

Settings are stored in:
```
HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy\
?? EnableShaderInjection
?? ShaderLayout
?? ShaderIntensity
?? StartWithWindows
?? MinimizeToTray
```

**Note:** This application does NOT modify Windows ClearType settings or system font rendering.

## Security & Safety

### What This App Does
- Injects display shaders into GUI processes
- Monitors running processes
- Stores user preferences in registry

### What This App Does NOT Do
- No admin rights required (user-level only)
- No kernel-level modifications
- No network communication
- No data collection or telemetry
- No modification of Windows ClearType settings

### Blacklisted Processes
The app **never** injects into:
- System processes (csrss, winlogon, etc.)
- Security processes (Windows Defender)
- Anti-cheat systems (EasyAntiCheat, BattlEye, etc.)
- Graphics drivers
- Its own process

## Building from Source

### Prerequisites
- Visual Studio 2022 (with C++ and .NET workloads)
- .NET 8.0 SDK
- Windows 10/11 SDK

### Build Steps

```powershell
# Clone the repository
git clone https://github.com/myn/DisplayShadersPowerToy.git
cd DisplayShadersPowerToy

# Build C++ hook DLL
msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release

# Build C# application
dotnet build -c Release

# Run
.\bin\Release\net8.0-windows\DisplayShadersPowerToy.exe
```

See [BUILD_INSTRUCTIONS.md](docs/BUILD_INSTRUCTIONS.md) for detailed build instructions.

## Troubleshooting

### "Shader Mode: Not Available"
- **Cause:** `DisplayShaderHook.dll` not found
- **Solution:** Ensure DLL is in same folder as executable
- **Check:** Look in diagnostic logs for DLL loading errors

### Text doesn't look different
1. Check that optimization is **ENABLED** (toggle in top-right)
2. Verify your **display type** is correct
3. Restart the application you want to optimize
4. Check **diagnostic logs** for injection errors

### Injection fails
- Some apps are protected (see logs)
- Try running as administrator (usually not needed)
- Check antivirus isn't blocking DLL

### App shows "Waiting for applications..."
- Shader injection is enabled but no apps hooked yet
- Launch a GUI application (browser, editor, etc.)
- Check diagnostic logs for injection attempts

## FAQ

**Q: Do I need administrator rights?**
A: No, the app runs with user-level privileges.

**Q: Will this work on my LG C2 OLED TV?**
A: Yes! Select "LG WOLED (WRGB)" for optimal results.

**Q: Can I use this with games?**
A: Yes, but anti-cheat systems may block injection. The app will skip those processes automatically.

**Q: What if DisplayShaderHook.dll is missing?**
A: The app will show "Shader Mode: Not Available" and no optimization will occur. You need both the EXE and DLL.

**Q: Does this work in virtual machines?**
A: Limited. VMs may use different rendering paths. Try it and check diagnostic logs.

**Q: Does this modify Windows settings?**
A: No. It only modifies application settings in the registry (under SOFTWARE\DisplayShadersPowerToy).

## Contributing

Contributions are welcome! Please see the [Development Guide](docs/DEVELOPER.md) for guidelines.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `.\test-complete-system.ps1`
5. Submit a pull request

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## Credits

- Inspired by [PowerToys Issue #25595](https://github.com/microsoft/PowerToys/issues/25595)
- Based on Blur Busters Display Shaders specification
- Built with love for the OLED community

## Documentation

- [Quick Start Guide](docs/QUICKSTART.md)
- [Build Instructions](docs/BUILD_INSTRUCTIONS.md)
- [Developer Guide](docs/DEVELOPER.md)
- [Implementation Status](docs/IMPLEMENTATION_STATUS.md)
- [Technical Limitations](docs/TECHNICAL_LIMITATIONS.md)

## Related Projects

- [PowerToys](https://github.com/microsoft/PowerToys) - Microsoft PowerToys
- [MacType](https://github.com/snowie2000/mactype) - Alternative font rendering
- [Better ClearType Tuner](https://github.com/bp2008/BetterClearTypeTuner) - ClearType configuration tool

---

**Made for OLED displays** | Version 2.0.0 | [Report Issues](https://github.com/myn/DisplayShadersPowerToy/issues)
