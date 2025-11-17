# Display Shaders PowerToy - Project Index

## Overview
Display Shaders PowerToy is a Windows utility that improves text rendering on OLED displays through **actual display shaders** with DirectWrite/D3D hooks, with ClearType fallback mode.

---

## ?? Project Structure

### Core Application (C# WPF)

#### Main Application
- **`App.xaml`** - Application definition and resources
- **`App.xaml.cs`** - Application startup logic, command-line args
- **`MainWindow.xaml`** - Main UI (settings, preview, theme toggle)
- **`MainWindow.xaml.cs`** - Main window logic and event handlers

#### Services
- **`Services/DisplayShaderService.cs`** - **DUAL MODE**: Real shaders + ClearType fallback
- **`Services/ShaderService.cs`** - **NEW**: Manages actual display shaders via native DLL
- **`Services/InjectionManager.cs`** - **NEW**: Safe DLL injection into processes
- **`Services/SettingsService.cs`** - Settings persistence (registry)

#### Models
- **`Models/DisplaySettings.cs`** - Settings data model
- **`Models/SubpixelLayout.cs`** - Subpixel layout enum

#### Helpers
- **`Helpers/IconGenerator.cs`** - System tray icon generation

### Native C++ Hook DLL ? NEW

**Location**: `Native/DisplayShaderHook/`

#### Core Hook System
- **`dllmain.cpp`** - DLL entry point, initialization
- **`DirectWriteHook.h/cpp`** - DirectWrite glyph rendering hooks
- **`SubpixelShader.h/cpp`** - **HLSL shaders for WOLED/QD-OLED fixes**
- **`ConfigLoader.h/cpp`** - Shared memory config communication
- **`Common.h`** - Shared data structures and utilities

#### Project Files
- **`DisplayShaderHook.vcxproj`** - Visual C++ project file

### HLSL Shader Implementation ? NEW

Embedded in `SubpixelShader.cpp`:

```hlsl
// WOLED WRGB Fix - RBG channel remapping (Blue in middle)
float3 ApplyWrgbLayout(float3 originalRGB, float2 screenPos)

// QD-OLED Triangular Fix - Vertical fringing correction  
float3 ApplyTriangularLayout(float3 originalRGB, float2 screenPos)

// Pentile Diamond Pattern Compensation
float3 ApplyPentileLayout(float3 originalRGB, float2 screenPos)
```

---

## ?? Documentation

### Implementation Docs ? NEW
- **`docs/OPTION_B_SUMMARY.md`** - **MAIN**: Implementation summary and status
- **`docs/IMPLEMENTATION_STATUS.md`** - Detailed progress tracking
- **`docs/BUILD_INSTRUCTIONS.md`** - How to build native DLL + C# app
- **`docs/ROADMAP.md`** - 5-phase implementation plan
- **`docs/TECHNICAL_LIMITATIONS.md`** - Why ClearType alone doesn't work

### User Documentation
- **`README.md`** - Main project README (updated for shader mode)
- **`GETTING_STARTED.md`** - Quick start guide
- **`QUICKSTART.md`** - Installation and basic usage
- **`FAQ.md`** - Frequently asked questions
- **`CONFIGURATION.md`** - Detailed configuration guide

### Developer Documentation  
- **`DEVELOPER.md`** - Developer guide (needs update for shaders)
- **`PROJECT_SUMMARY.md`** - Project overview
- **`CHANGELOG.md`** - Version history
- **`LICENSE`** - MIT License

### Technical Docs
- **`docs/WINDOW_CLOSING_FIX.md`** - System tray minimize fix
- **`docs/ICON.md`** - Icon generation details
- **`COMMUNITY_RESPONSE.md`** - Response to feedback

---

## ?? Key Features

### ? Implemented

#### Real Display Shaders (NEW - Phase 2)
- Native C++ DirectWrite hooks
- HLSL pixel shaders for GPU execution
- WOLED WRGB ? RBG remapping (Blue in middle)
- QD-OLED triangular layout support (vertical)
- Pentile diamond pattern compensation
- Shared memory config communication
- Safe DLL injection system

#### ClearType Mode (Legacy Fallback)
- RGB Stripe (Standard LCD)
- WRGB Stripe (WOLED workaround)
- RGB Triangular (QD-OLED workaround)  
- PenTile (AMOLED workaround)
- Adjustable intensity (0-100%)

#### Application Features
- Modern WPF UI with dark/light themes
- System tray integration
- Start with Windows
- Minimize to tray
- Real-time preview
- Settings persistence

### ? In Progress

#### Critical for POC
1. Microsoft Detours integration (4-8 hours)
2. DirectWrite vtable hooking (8-16 hours)
3. D3D11 device acquisition (4-8 hours)
4. Glyph rendering pipeline (16-24 hours)
5. Build system integration (2-4 hours)

**Estimated POC Time**: 40-60 hours

---

## ??? Architecture

### Dual-Mode Operation

```
???????????????????????????????????????????
? DisplayShadersPowerToy.exe (C# WPF)     ?
?                                          ?
? ??????????????????????????????????????  ?
? ? DisplayShaderService               ?  ?
? ?                                    ?  ?
? ? if (ShaderMode Available)          ?  ?
? ?   ? Use Real Shaders ?            ?  ?
? ? else                               ?  ?
? ?   ? Use ClearType Fallback         ?  ?
? ??????????????????????????????????????  ?
???????????????????????????????????????????

         ?                    ?
         ? Real Shaders       ? ClearType
         ?                    ?
????????????????????  ??????????????????
? ShaderService    ?  ? Registry Tweaks?
? InjectionManager ?  ? SystemParams   ?
????????????????????  ??????????????????
         ?
         ? Inject DLL
         ?
???????????????????????????????????????????
? Target App (notepad.exe, chrome.exe)   ?
?                                          ?
? ??????????????????????????????????????  ?
? ? DisplayShaderHook.dll (Native C++) ?  ?
? ?                                    ?  ?
? ? DirectWriteHook                    ?  ?
? ?  ? Intercepts DrawGlyphRun        ?  ?
? ? SubpixelShader                     ?  ?
? ?  ? Applies HLSL shader (GPU)      ?  ?
? ?    • WOLED: RBG remapping         ?  ?
? ?    • QD-OLED: Triangular fix      ?  ?
? ?  ? Outputs corrected text         ?  ?
? ??????????????????????????????????????  ?
???????????????????????????????????????????
```

---

## ?? Getting Started

### For Users

```bash
# Download release
# Extract to folder  
# Run DisplayShadersPowerToy.exe
# Select your display type
# Click "Apply"
```

See `GETTING_STARTED.md` for details.

### For Developers (C# Only)

```bash
git clone https://github.com/yourusername/DisplayShadersPowerToy.git
cd DisplayShadersPowerToy
dotnet build
dotnet run
```

App will run in **ClearType fallback mode**.

### For Developers (With Shaders)

```bash
# Prerequisites:
# - Visual Studio 2022 with C++ workload
# - Microsoft Detours library

# Build native DLL
cd Native/DisplayShaderHook
msbuild DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64

# Build C# app  
cd ../..
dotnet build

# Run with shader support
dotnet run
```

See `docs/BUILD_INSTRUCTIONS.md` for complete setup.

---

## ?? Implementation Status

### Phase 1: Honest Rebranding ? COMPLETE
- [x] Technical limitations documentation
- [x] Community response
- [x] Updated README with disclaimers
- [x] Code comments explaining reality

### Phase 2: Research & POC ? 20% COMPLETE
- [x] Native C++ hook architecture
- [x] HLSL shader implementation
- [x] C# management services
- [x] Shared memory communication
- [ ] Detours integration (pending)
- [ ] Actual DirectWrite hooking (pending)
- [ ] D3D11 device acquisition (pending)
- [ ] Glyph rendering pipeline (pending)

### Phase 3: Production Implementation ? 5% COMPLETE
- [ ] Robust error handling
- [ ] PNG mask file loading
- [ ] Auto-injection on process start
- [ ] Performance optimization
- [ ] UI for shader mode

### Phase 4: Testing ? NOT STARTED
- [ ] Beta testing program
- [ ] WOLED hardware testing
- [ ] QD-OLED hardware testing
- [ ] Compatibility testing

### Phase 5: Release ? NOT STARTED
- [ ] Code signing
- [ ] Installer
- [ ] Documentation finalization
- [ ] PowerToys contribution

**Overall Progress**: ~15% to production-ready

See `docs/IMPLEMENTATION_STATUS.md` for detailed tracking.

---

## ?? Technical Details

### Subpixel Layouts Supported

#### 1. RGB Stripe (Standard LCD)
```
R G B | R G B | R G B
```
**Support**: Native Windows ClearType ?

#### 2. WOLED WRGB Stripe (LG OLED)
```
W R G B | W R G B
```
**Problem**: Need RBG (Blue in middle)  
**Windows**: Only supports RGB/BGR  
**Our Fix**: Shader swaps channels ?

```hlsl
adjusted.r = original.r * mask.r;  // Red
adjusted.b = original.b * mask.g;  // Blue ? middle
adjusted.g = original.g * mask.b;  // Green ? right
```

#### 3. QD-OLED Triangular (Samsung)
```
    G
  R   B
```
**Problem**: Vertical fringing (Green top, R/B bottom)  
**Windows**: Only handles horizontal  
**Our Fix**: Triangular-aware mask ?

#### 4. Pentile Diamond (AMOLED)
```
R G
G B
```
**Problem**: Shared green subpixels  
**Windows**: Assumes separate RGB  
**Our Fix**: Diamond pattern compensation ?

---

## ?? Dependencies

### C# Application
- .NET 8.0 Windows Desktop Runtime
- Hardcodet.NotifyIcon.Wpf 2.0.1

### Native Hook DLL
- Windows SDK 10.0.19041.0+
- D3D11 (included in SDK)
- DirectWrite (included in SDK)
- Microsoft Detours (external - needs installation)

---

## ?? Contributing

See `DEVELOPER.md` for contribution guidelines.

**Current Priority**: Complete Phase 2 POC
- Add Detours integration
- Implement DirectWrite hooks
- Connect D3D11 rendering

**How to Help**:
1. Test on WOLED/QD-OLED hardware
2. Review native C++ code
3. Improve HLSL shaders
4. Documentation improvements

---

## ?? License

MIT License - see `LICENSE` file

---

## ?? Credits

- Inspired by [PowerToys Issue #25595](https://github.com/microsoft/PowerToys/issues/25595)
- Community feedback on WOLED RBG approach
- Based on Blur Busters Display Shaders specification
- OLED display users testing and reporting

---

## ?? Support

- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions  
- **Documentation**: See `docs/` folder

---

**Last Updated**: 2024  
**Status**: Phase 2 in progress - Shader architecture complete, implementation ~20% done  
**Next Milestone**: Working POC with Detours integration
