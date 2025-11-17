# Microsoft Detours Integration

## Installation Instructions

Since Microsoft Detours is a third-party library, we need to handle it properly.

### Option 1: NuGet Package (Recommended)

The easiest way is to use the NuGet package:

```bash
# In the Native/DisplayShaderHook directory
nuget install Detours -Version 4.0.1
```

Or add to packages.config:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Detours" version="4.0.1" targetFramework="native" />
</packages>
```

### Option 2: Build from Source

If you prefer to build from source:

```bash
git clone https://github.com/microsoft/Detours.git
cd Detours
nmake
```

Then copy the files:
- `lib.X64\detours.lib` ? `Native\DisplayShaderHook\lib\detours.lib`
- `include\detours.h` ? `Native\DisplayShaderHook\include\detours.h`

### Option 3: Simplified Hook (No Detours)

For development/testing without Detours, we provide a simplified hooking mechanism
that uses MinHook (MIT licensed, easier to integrate).

See `SimplifiedHook.cpp` for the MinHook-based implementation.

## Current Status

The project is configured to work with **Option 3** (MinHook) by default, as it:
- Doesn't require external dependencies
- MIT licensed (same as this project)
- Easier to build and distribute
- Sufficient for DirectWrite hooking

If you need full Detours support, switch to Option 1 or 2 and update the vcxproj file.
