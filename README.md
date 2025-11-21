# OLED Text Optimizer

**Crystal-clear text rendering for modern OLED displays**

![OLED Text Optimizer](Screenshot%202025-11-17%20135100.png)

## What is This?

OLED Text Optimizer is a Windows utility that fixes text rendering issues on modern OLED displays by using **real-time DirectWrite shader injection** and automatic **ClearType fallback optimization**.

### The Problem

Modern OLED displays use different subpixel layouts than traditional LCDs:
- **LG WOLED** (WRGB) - Four subpixels with white in center
- **Samsung QD-OLED** (RGB Triangular) - Triangular arrangement
- **PenTile AMOLED** - Diamond pattern

Windows ClearType was designed for standard RGB stripe LCDs, causing **color fringing** and **blurry text** on OLED displays.

### The Solution

This application provides **two optimization methods** that work together:

1. **Real-time Shader Injection** (Primary) - Injects DirectWrite hooks into running applications for pixel-perfect subpixel rendering
2. **ClearType Optimization** (Automatic Fallback) - Registry-based system-wide optimization when injection isn't available

## Features

### Real-Time Optimization
- **Automatic process injection** - Hooks into applications as they launch
- **Live monitoring** - Shows currently optimized applications
- **Instant apply** - Changes take effect immediately
- **Safe and reversible** - Clean unhooking on exit

### Display Support
- **Standard LCD/LED/JOLED** - Traditional RGB stripe (includes JOLED displays)
- **LG WOLED (WRGB)** - LG C/G/B series OLED displays
- **QD-OLED (RGB Triangular)** - Samsung/Alienware monitors
- **PenTile AMOLED** - Diamond pattern displays

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

1. **Download** the latest release from [Releases](https://github.com/yourusername/DisplayShadersPowerToy/releases)
2. **Extract** the files to a folder (e.g., `C:\Program Files\OLED Text Optimizer\`)
3. **Run** `DisplayShadersPowerToy.exe`

### First-Time Setup

1. **Select your display type:**
   - LG OLED monitor? **LG WOLED (WRGB)**
   - Samsung QD-OLED? **QD-OLED (RGB Triangular)**
   - JOLED monitor? **Standard LCD / LED / JOLED Monitor**
   - Standard LCD? **Standard LCD / LED / JOLED Monitor**

2. **Adjust optimization strength** (100% recommended to start)

3. **Toggle ON** the master switch (top-right)

4. **Watch the magic happen!** 
   - Status shows "Optimizing X applications"
   - Active processes list appears
   - Text rendering improves immediately

### Recommended Settings

| Display Type | Setting | Intensity |
|-------------|---------|-----------|
| **LG WOLED** | LG WOLED (WRGB) | 85-100% |
| **Samsung QD-OLED** | QD-OLED (RGB Triangular) | 75-90% |
| **JOLED** | Standard LCD / LED / JOLED | 100% |
| **Standard LCD** | Standard LCD / LED / JOLED | 100% |

## How It Works

### Dual-Mode Architecture

```
+---------------------------------------------+
|          OLED Text Optimizer                |
+---------------------------------------------+
|                                             |
|  [1] Real-Time Shader Injection             |
|      - Hook DirectWrite API                 |
|      - Apply HLSL pixel shaders             |
|      - Monitor new processes                |
|      - Auto-inject on launch                |
|                                             |
|  [2] ClearType Optimization (Fallback)      |
|      - Modify Windows registry              |
|      - System-wide settings                 |
|      - Always active as safety net          |
|                                             |
+---------------------------------------------+
```
### What Gets Optimized?

**Primary Method (Shader Injection):**
- All GUI applications (Chrome, Firefox, Edge, etc.)
- Text editors (Notepad, VS Code, Visual Studio, etc.)
- Office apps (Word, Excel, PowerPoint, etc.)
- Communication apps (Slack, Teams, Discord, etc.)
- Any app using DirectWrite for text rendering

**Fallback Method (ClearType):**
- Legacy applications
- System UI elements
- Apps that block injection
- Protected processes

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

### Modified Registry Keys

ClearType settings (automatic fallback):
```
HKEY_CURRENT_USER\Control Panel\Desktop\
?? FontSmoothing
?? FontSmoothingType
?? FontSmoothingOrientation
?? FontSmoothingGamma
```

Application settings:
```
HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy\
?? DisplayType
?? Intensity
?? EnableInjection
?? StartWithWindows
```

## Security & Safety

### What This App Does
- Injects display shaders into GUI processes
- Modifies user-level registry settings
- Monitors running processes

### What This App Does NOT Do
- No admin rights required (user-level only)
- No kernel-level modifications
- No network communication
- No data collection or telemetry

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

### Text doesn't look different
1. Check that optimization is **ENABLED** (toggle in top-right)
2. Verify your **display type** is correct
3. Restart the application you want to optimize
4. Check **diagnostic logs** for injection errors

### Injection fails
- Some apps are protected (see logs)
- Try running as administrator
- Check antivirus isn't blocking DLL

### App crashes or freezes
- Check logs: `%LOCALAPPDATA%\DisplayShadersPowerToy\Logs\`
- Report issue with log file attached
- Disable injection, use ClearType-only mode

## FAQ

**Q: Do I need administrator rights?**
A: No, the app runs with user-level privileges.

**Q: Will this work on my LG C2 OLED TV?**
A: Yes! Select "LG WOLED (WRGB)" for optimal results.

**Q: Can I use this with games?**
A: Yes, but anti-cheat systems may block injection. ClearType fallback will still work.

**Q: How do I uninstall?**
A: Just delete the application folder. Settings are stored in registry and can be reset by selecting "Standard LCD" and applying.

**Q: Does this work in virtual machines?**
A: Limited. VMs may use different rendering paths. Try it and check diagnostic logs.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

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

- [Quick Start Guide](QUICKSTART.md)
- [Build Instructions](docs/BUILD_INSTRUCTIONS.md)
- [Implementation Status](docs/IMPLEMENTATION_STATUS.md)
- [Technical Limitations](docs/TECHNICAL_LIMITATIONS.md)
- [Developer Guide](DEVELOPER.md)

## Related Projects

- [PowerToys](https://github.com/microsoft/PowerToys) - Microsoft PowerToys
- [MacType](https://github.com/snowie2000/mactype) - Alternative font rendering
- [Better ClearType Tuner](https://github.com/bp2008/BetterClearTypeTuner) - ClearType configuration tool

---

**Made for OLED displays** | Version 1.0.0 | [Report Issues](https://github.com/yourusername/DisplayShadersPowerToy/issues)
