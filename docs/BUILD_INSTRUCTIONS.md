# Building the Native Shader Hook

## Overview

The Display Shaders PowerToy now includes **actual shader support** through a native C++ DLL that hooks into DirectWrite text rendering.

This replaces the legacy ClearType-only mode with real subpixel-aware rendering.

## Prerequisites

### For C# Development
- .NET 8.0 SDK
- Visual Studio 2022 (any edition)

### For Native C++ Hook Development
- Visual Studio 2022 with C++ Desktop Development workload
- Windows SDK 10.0.19041.0 or later
- Microsoft Detours library (see below)

## Project Structure

```
DisplayShadersPowerToy/
??? Services/
?   ??? DisplayShaderService.cs   (Dual-mode: Shader + ClearType)
?   ??? ShaderService.cs          (Manages real shaders)
?   ??? InjectionManager.cs       (DLL injection)
?   ??? SettingsService.cs        (Existing)
??? Native/
?   ??? DisplayShaderHook/
?       ??? DirectWriteHook.cpp   (Hook DirectWrite)
?       ??? SubpixelShader.cpp    (HLSL shaders)
?       ??? ConfigLoader.cpp      (Shared memory)
?       ??? dllmain.cpp            (DLL entry point)
??? SubpixelMasks/
    ??? woled_wrgb.png            (WOLED mask - to be created)
    ??? qdoled_triangular.png     (QD-OLED mask - to be created)
    ??? pentile.png               (Pentile mask - to be created)
```

## Building the Solution

### Option 1: C# Only (No Shaders)

If you only want to work on the C# application:

```bash
dotnet build
dotnet run
```

The app will run in **ClearType fallback mode** (legacy behavior).

### Option 2: Full Build with Native Shaders

#### Step 1: Install Microsoft Detours

Microsoft Detours is required for hooking DirectWrite APIs.

**Download**:
- https://github.com/microsoft/Detours

**Install**:
```bash
# Clone Detours
git clone https://github.com/microsoft/Detours.git
cd Detours

# Build
nmake

# Copy to project
copy lib.X64\detours.lib <ProjectRoot>\Native\DisplayShaderHook\lib\
copy include\detours.h <ProjectRoot>\Native\DisplayShaderHook\include\
```

#### Step 2: Add Native Project to Solution

1. Open `DisplayShadersPowerToy.sln` in Visual Studio 2022
2. Right-click solution ? Add ? Existing Project
3. Select `Native\DisplayShaderHook\DisplayShaderHook.vcxproj`
4. Right-click C# project ? Build Dependencies ? Project Dependencies
5. Check `DisplayShaderHook` as a dependency

#### Step 3: Configure Build

Update `DisplayShaderHook.vcxproj` to link Detours:

```xml
<ItemDefinitionGroup>
  <Link>
    <AdditionalLibraryDirectories>$(ProjectDir)lib\;%(AdditionalLibraryDirectories)</AdditionalLibraryDirectories>
    <AdditionalDependencies>detours.lib;d3d11.lib;dxgi.lib;d2d1.lib;dwrite.lib;%(AdditionalDependencies)</AdditionalDependencies>
  </Link>
</ItemDefinitionGroup>
```

#### Step 4: Build

```bash
# Build everything
msbuild DisplayShadersPowerToy.sln /p:Configuration=Release /p:Platform=x64

# Or use Visual Studio
# Build ? Build Solution (Ctrl+Shift+B)
```

This produces:
- `bin\Release\net8.0-windows\DisplayShadersPowerToy.exe`
- `bin\x64\Release\DisplayShaderHook.dll` (copied to C# output)

## Running with Shader Support

### Check Shader Mode Status

Run the application and look for the status indicator (planned UI feature):

```
Shader Mode: Active (Hook v1)  ? Real shaders
Shader Mode: Not Available     ? ClearType fallback
```

### Manual DLL Injection Test

Test injection manually before running full app:

```powershell
# Start notepad
Start-Process notepad.exe

# Inject DLL (requires admin)
rundll32.exe DisplayShaderHook.dll,<EntryPoint>
```

Or use the C# injection manager:

```csharp
var injector = new InjectionManager();
injector.InjectIntoProcesses(); // Injects into whitelisted processes
```

## Development Workflow

### Iterating on HLSL Shaders

The HLSL shader code is embedded in `SubpixelShader.cpp`:

```cpp
const char* SUBPIXEL_SHADER_HLSL = R"(
    // Edit shader here
    float4 main(PSInput input) : SV_TARGET
    {
        // Your shader code
    }
)";
```

**To test changes**:
1. Edit `SubpixelShader.cpp`
2. Rebuild `DisplayShaderHook` project
3. Kill and restart target application (e.g., notepad)
4. DLL will be re-injected with new shader

### Debugging the Native DLL

**Attach Visual Studio debugger**:

1. Build `DisplayShaderHook` in Debug mode
2. Run target application (e.g., notepad.exe)
3. In Visual Studio: Debug ? Attach to Process
4. Select notepad.exe
5. Set breakpoints in `DirectWriteHook.cpp` or `SubpixelShader.cpp`

**Logging**:

Use `LogDebug()` and `LogError()` from `Common.h`:

```cpp
LogDebug(L"Hook installed successfully");
LogError(L"Failed to create shader: 0x%08X", hr);
```

View output in **DebugView** (https://learn.microsoft.com/en-us/sysinternals/downloads/debugview).

### Testing on Different Layouts

Create test masks in `SubpixelMasks/`:

**WOLED WRGB** (`woled_wrgb.png`):
- 4x1 pixel image
- Pixel 0: Black (white subpixel, ignored)
- Pixel 1: Red (255, 0, 0)
- Pixel 2: Blue (0, 0, 255) ? Blue in middle!
- Pixel 3: Green (0, 255, 0)

**QD-OLED Triangular** (`qdoled_triangular.png`):
- 2x2 pixel image
- Row 0: (Green/2, Green/2) ? Top row is green
- Row 1: (Red, Blue) ? Bottom row is R/B

Use image editors like GIMP or Photoshop to create precise pixel masks.

## Troubleshooting

### DLL Fails to Inject

**Error**: "Failed to inject into process"

**Causes**:
1. Application is running as admin, injector is not
2. Process is 32-bit (DLL is 64-bit only)
3. Antivirus blocking injection

**Solutions**:
- Run PowerToy as administrator
- Check target process architecture
- Add exception in antivirus

### Hooks Crash Target Application

**Error**: Target app crashes immediately after injection

**Causes**:
1. DirectWrite hook is malformed
2. Shader compilation failed
3. Memory access violation

**Solutions**:
- Attach debugger before injection
- Check crash dump (WER)
- Test with minimal hook (no shader)

### Shader Has No Effect

**Error**: DLL injects but text looks the same

**Causes**:
1. Hook not actually intercepting calls
2. Shader intensity set to 0%
3. Wrong process (not using DirectWrite)

**Solutions**:
- Verify `Hook_DrawGlyphRun` is called (add logging)
- Check shader config in shared memory
- Test with known DirectWrite app (VS Code, Edge)

## Performance Profiling

### Measure Hook Overhead

```cpp
// In Hook_DrawGlyphRun
auto start = std::chrono::high_resolution_clock::now();

// Call shader
SubpixelShader::Instance().RenderGlyphRun(...);

auto end = std::chrono::high_resolution_clock::now();
auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);

LogDebug(L"Shader took %lld ?s", duration.count());
```

**Target**: <100 ?s per glyph run

### GPU Profiling

Use **RenderDoc** or **PIX for Windows**:

1. Install PIX from Microsoft Store
2. Launch target app through PIX
3. Capture frame when rendering text
4. Inspect pixel shader execution time

## Security Considerations

### Code Signing

Before distribution, sign the DLL:

```bash
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com DisplayShaderHook.dll
```

### Antivirus Whitelisting

Submit to antivirus vendors:
- Microsoft Defender: https://www.microsoft.com/en-us/wdsi/filesubmission
- Symantec: https://submit.symantec.com/
- Others as needed

### Process Whitelist

Keep `InjectionManager._processWhitelist` restrictive:

```csharp
_processWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "notepad",      // ? Safe
    "code",         // ? Safe
    "chrome",       // ? Safe
    // "game.exe"   // ? Avoid games (anticheat)
};
```

## Contributing

### Code Style

**C++**:
- Follow Microsoft C++ Core Guidelines
- Use modern C++20 features
- RAII for resource management
- Smart pointers over raw pointers

**C#**:
- Follow .NET conventions
- Use nullable reference types
- Async/await for I/O
- XML documentation comments

### Pull Requests

Before submitting:
1. [ ] Code builds without warnings
2. [ ] Tested on at least 2 applications
3. [ ] No memory leaks (checked with ASAN or Valgrind)
4. [ ] Updated IMPLEMENTATION_STATUS.md
5. [ ] Added tests if applicable

## Resources

**Microsoft Documentation**:
- [DirectWrite API](https://learn.microsoft.com/en-us/windows/win32/directwrite/direct-write-portal)
- [Direct3D 11](https://learn.microsoft.com/en-us/windows/win32/direct3d11/atoc-dx-graphics-direct3d-11)
- [HLSL Reference](https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl)

**Tools**:
- [RenderDoc](https://renderdoc.org/) - Graphics debugging
- [PIX for Windows](https://devblogs.microsoft.com/pix/) - GPU profiling
- [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview) - Kernel debug output

**Libraries**:
- [Microsoft Detours](https://github.com/microsoft/Detours) - API hooking
- [stb_image](https://github.com/nothings/stb) - PNG loading

---

**Status**: Architecture complete, implementation ~20% done  
**Next**: Integrate Detours and implement actual hooks  
**See**: `docs/IMPLEMENTATION_STATUS.md` for detailed progress
