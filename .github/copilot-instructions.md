# GitHub Copilot Instructions for Display Shaders PowerToy

## Project Overview

Display Shaders PowerToy (also known as OLED Text Optimizer) is a Windows utility that fixes text rendering issues on modern OLED displays using **real-time DirectWrite shader injection** with custom HLSL pixel shaders.

The application consists of two main components:
- **C# WPF Application** (`DisplayShadersPowerToy.exe`) - Modern UI for configuration and process management
- **C++ Hook DLL** (`DisplayShaderHook.dll`) - DirectWrite API hooks and HLSL shader implementation

## Project Structure

```
DisplayShadersPowerToy/
├── Models/                      # C# data models
│   ├── DisplaySettings.cs       # Settings data model
│   └── SubpixelLayout.cs        # Enum for subpixel types
├── Services/                    # C# business logic
│   ├── DisplayShaderService.cs  # Shader injection management
│   ├── ShaderService.cs         # Shader configuration
│   ├── InjectionManager.cs      # Process injection
│   └── SettingsService.cs       # Settings persistence
├── Helpers/                     # C# utility classes
├── Native/DisplayShaderHook/    # C++ Hook DLL
│   ├── dllmain.cpp              # DLL entry point
│   ├── DirectWriteHook.cpp      # DirectWrite API hooks
│   ├── SubpixelShader.cpp       # HLSL shader implementation
│   └── ConfigLoader.cpp         # Configuration loading
├── SubpixelMasks/               # Subpixel mask images
├── docs/                        # Documentation
├── MainWindow.xaml              # Main UI definition
├── MainWindow.xaml.cs           # Main UI logic
├── App.xaml                     # Application definition
├── App.xaml.cs                  # Application startup logic
└── DisplayShadersPowerToy.csproj # C# project file
```

## Build Instructions

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 (with C++ and .NET workloads)
- Windows 10/11 SDK

### Building the C# Application Only (No Shaders)

```bash
dotnet restore
dotnet build
```

The app will run in ClearType fallback mode without the native DLL.

### Building with Native Shader Support

```bash
# Build C++ hook DLL first
msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64

# Build C# application
dotnet build -c Release
```

### Running

```bash
dotnet run
```

Or run the executable directly:
```
bin\Release\net8.0-windows\DisplayShadersPowerToy.exe
```

## Testing

### Manual Testing Checklist

- All subpixel layouts apply correctly
- Shader intensity slider works (0-100%)
- Enable/disable shader toggles correctly
- Start with Windows registers in registry
- Minimize to tray hides window
- System tray icon shows and works
- Settings persist after restart
- Status updates show correct process count

## Code Style Guidelines

### C# Conventions

- Use PascalCase for public members
- Use camelCase for private fields with underscore prefix (`_fieldName`)
- Use explicit access modifiers
- Use nullable reference types (project has `<Nullable>enable</Nullable>`)
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Follow .NET conventions
- Use async/await for I/O operations

### C++ Conventions

- Use PascalCase for classes and functions
- Use camelCase for variables
- Follow Microsoft C++ Core Guidelines
- Use modern C++20 features
- Use RAII for resource management
- Use smart pointers over raw pointers
- Check return values from API calls

### XAML Conventions

- Use `x:Name` for controls accessed in code-behind
- Group related UI elements in cards/borders
- Use consistent spacing and indentation
- Define reusable styles in `Window.Resources`

## Key Technical Details

### Supported Display Types

- **Standard LCD/LED** - Traditional RGB stripe
- **LG WOLED (WRGB)** - LG C/G/B series OLED displays
- **QD-OLED (RGB Triangular)** - Samsung/Alienware monitors
- **PenTile AMOLED** - Diamond pattern displays

### Architecture

The application uses shared memory for communication between C# and C++:
- Named memory: "DisplayShadersConfig"
- Size: 256 bytes
- Contains: Layout type, intensity, enabled flag

### Dependencies

**C# Application:**
- Hardcodet.NotifyIcon.Wpf (2.0.1) - System tray support

**C++ Hook DLL:**
- MinHook - API hooking library
- Windows SDK - DirectWrite and D3D11 headers

## Contributing

### Pull Request Requirements

1. Code builds without warnings
2. Tested on at least 2 applications
3. No memory leaks (for C++ changes)
4. Updated documentation if applicable
5. Follow existing code style
6. Update CHANGELOG.md for user-facing changes

### Process Filtering

When working with the injection system, note that:
- Only GUI applications are injected (MainWindowHandle != 0)
- System processes are blacklisted
- Security software is blacklisted
- Anti-cheat systems are blacklisted

## Important Notes

- The application does NOT modify Windows ClearType settings or system font rendering
- The app runs with user-level privileges (no admin rights required)
- Settings are stored in registry under `HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy\`
- The `DisplayShaderHook.dll` must be in the same directory as the executable for shader support
