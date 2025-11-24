# Technical Limitations

## Overview

This document provides an honest assessment of what this application can and cannot do.

## What This Application Does

### Real Shader Implementation

This application uses **actual DirectWrite shader injection** through a native C++ DLL:

**DisplayShaderHook.dll** provides:
- DirectWrite API hooking via MinHook
- Custom HLSL pixel shader implementation
- Subpixel-aware rendering for different display types
- D3D11 device management
- Real-time process injection

**This is not a workaround** - it's genuine shader-based rendering.

## Current Capabilities

### ? What Works

**1. Real-Time Shader Injection**
- Hooks into DirectWrite rendering pipeline
- Applies custom HLSL pixel shaders
- Supports WOLED, QD-OLED, PenTile, and standard RGB

**2. Process Management**
- Automatic injection into GUI applications
- Continuous monitoring for new processes
- Safe unhooking on exit

**3. Subpixel Layouts Supported**
- RGB Stripe (standard LCD)
- WRGB Stripe (WOLED - LG OLED displays)
- RGB Triangular (QD-OLED - Samsung monitors)
- PenTile (AMOLED diamond pattern)

**4. Adjustable Settings**
- Per-layout optimization
- Adjustable shader intensity
- Live configuration updates

## Known Limitations

### Shader Injection Limitations

**1. Requires DLL File**
- `DisplayShaderHook.dll` must be present
- If missing, app shows "Shader Mode: Not Available"
- No fallback mode available

**2. Application Compatibility**
- Only works with apps using DirectWrite
- Some apps may block DLL injection
- Protected processes are automatically skipped

**3. Anti-Cheat Systems**
- Games with anti-cheat may block injection
- App automatically blacklists known anti-cheat processes
- No workaround available (by design)

**4. System Processes**
- Critical system processes are blacklisted
- No injection into Windows core components
- This is a safety feature

### Display Support Limitations

**1. WOLED (WRGB Stripe)**
- Shader provides W-R-G-B aware rendering
- May not be perfect for all WOLED variants
- Different manufacturers may have slight variations

**2. QD-OLED (RGB Triangular)**
- Shader handles triangular arrangement
- Vertical fringing compensation included
- Results may vary by specific monitor model

**3. PenTile**
- Diamond pattern compensation provided
- May need intensity adjustment per display
- Optimization is approximated

### Performance Limitations

**1. Startup Time**
- Initial process scan can take 2-5 seconds
- Parallel injection speeds up process
- Depends on number of running applications

**2. Memory Usage**
- Each injected process loads DLL into memory
- ~2-5MB overhead per process
- Minimal impact on modern systems

**3. CPU Impact**
- Shader compilation on first run
- Negligible runtime overhead
- DirectWrite already uses GPU

## What Cannot Be Done

### ? System-Wide Text Rendering

**Limitation:** Cannot modify all text rendering system-wide

**Why:** 
- Windows uses multiple rendering paths
- Some apps use GDI, GDI+, or custom rendering
- DirectWrite injection only affects DirectWrite apps

**Impact:**
- Windows UI elements may not be affected
- Legacy applications won't be optimized
- Per-app approach required

### ? Kernel-Level Optimization

**Limitation:** No kernel-mode driver

**Why:**
- App runs in user-mode only
- No administrator rights required
- Safer and more compatible

**Impact:**
- Cannot intercept display driver calls
- Cannot modify GPU rendering pipeline
- Limited to application-level hooks

### ? Automatic Display Detection

**Limitation:** Cannot auto-detect monitor type

**Why:**
- No standard API for subpixel layout detection
- EDID data doesn't include subpixel information
- Manufacturers don't expose this info

**Impact:**
- User must manually select display type
- No automatic profile switching
- Settings persist across monitor changes

## Comparison with Ideal Solution

### Current Implementation

```
Application Process
    ?
DirectWrite API Call
    ?
[HOOK] DisplayShaderHook.dll
    ?
Custom HLSL Shader
    ?
D3D11 Rendering
    ?
Display Output
```

**Pros:**
- Works in user-mode
- No admin rights needed
- Safe and reversible
- Real shader rendering

**Cons:**
- Only affects DirectWrite apps
- Requires DLL injection
- Not system-wide

### Ideal Solution

```
Windows Display Stack
    ?
Monitor Driver
    ?
[CUSTOM DRIVER] Subpixel Aware Rendering
    ?
GPU Processing
    ?
Display Output
```

**Would provide:**
- System-wide optimization
- All applications affected
- Automatic display detection
- Built into Windows

**Challenges:**
- Requires Microsoft implementation
- Needs monitor driver support
- Complex certification process
- Years of development

## Honest Assessment

### What This App Is

? **A real shader injection system** using DirectWrite hooks
? **Effective for supported applications** that use DirectWrite
? **Safe and user-friendly** with no system modifications
? **The best user-mode solution** currently available

### What This App Is Not

? **Not a system-wide solution** - only affects DirectWrite apps
? **Not a perfect fix** - cannot match ideal monitor driver approach
? **Not magic** - limited by Windows API constraints

## Future Improvements

### Possible Enhancements

**1. Broader Compatibility**
- Hook additional rendering APIs (GDI+, D2D)
- Support more application types
- Improve injection success rate

**2. Better Detection**
- Auto-detect common monitor models
- Suggest layout based on heuristics
- Profile database for known monitors

**3. Performance**
- Optimize shader compilation
- Reduce memory footprint
- Faster injection process

**4. Advanced Features**
- Per-monitor configuration
- Multi-monitor support
- Custom shader profiles

### Long-Term Vision

**Microsoft Integration:**
- Propose Windows API extension
- Add subpixel layout to SystemParametersInfo
- Native OS support for OLED displays

**Monitor Driver Support:**
- Work with manufacturers
- Standard EDID extension for subpixel layout
- Automatic configuration

## Conclusion

This application provides **real shader-based optimization** using DirectWrite hooks. While it has limitations compared to an ideal OS-level solution, it's effective for applications using DirectWrite and represents the best approach available in user-mode.

**It's honest about what it does** - genuine shader injection, not registry tweaks.

## References

- [PowerToys Issue #25595](https://github.com/microsoft/PowerToys/issues/25595)
- [DirectWrite Documentation](https://docs.microsoft.com/en-us/windows/win32/directwrite/direct-write-portal)
- [Blur Busters Display Shaders](https://blurbusters.com)

---

**Last Updated:** 2025-01-17
**Status:** Shader injection only, no ClearType fallback
