# ?? CRITICAL ISSUES IDENTIFIED

## Issue 1: Shader Layout Changes Don't Apply to Hooked Processes

### Problem
When the user changes the shader subpixel layout (RGB Stripe ? WRGB Stripe ? etc.), the hooked processes continue using the **old** configuration. The new layout only applies to newly hooked processes.

### Root Cause
```csharp
// ShaderLayout_Changed event handler
private void ShaderLayout_Changed(object sender, RoutedEventArgs e)
{
    _currentSettings.ShaderLayout = SubpixelLayout.WrgbStripe;
    
    // Calls ApplySettingsImmediate()
    //   ? DisplayShaderService.ApplyShaderSettings()
    //     ? ShaderService.UpdateShaderConfig()  
    //       ? Writes shader_config.ini
    
    // ? PROBLEM: DLL reads config.ini on STARTUP only
    // ? Existing hooked processes never reload!
}
```

**What happens:**
1. User changes from "RGB Stripe" to "WRGB Stripe"
2. Config file is written: `shader_config.ini`
3. ? **Already injected processes keep using RGB Stripe**
4. ? New processes will use WRGB Stripe (they read config on DLL_PROCESS_ATTACH)

### Expected Behavior
When shader layout changes, **all hooked processes** should immediately update to use the new layout.

---

## Issue 2: Preview Doesn't Show Shader Effects

### Problem
The "Preview" button only previews **ClearType changes**, not **shader injection changes**. Users can't see the difference between different shader layouts or intensities.

### Root Cause
```csharp
private void Preview_Click(object sender, RoutedEventArgs e)
{
    // Saves current ClearType settings
    _previewOriginalSettings = _displayShaderService.GetCurrentSettings();
    
    // Applies new settings
    _displayShaderService.ApplyShaderSettings(_currentSettings);
    
    // ? PROBLEM: GetCurrentSettings() only reads ClearType registry!
    // ? Shader injection state is not captured or restored!
}
```

**What `GetCurrentSettings()` actually does:**
```csharp
public DisplaySettings GetCurrentSettings()
{
    var settings = new DisplaySettings();
    
    // ? Reads ClearType from registry
    settings.EnableClearType = ReadFromRegistry();
    settings.ClearTypeLayout = DetectFromRegistry();
    
    // ? Hardcoded shader defaults (doesn't read current state!)
    settings.EnableShaderInjection = true;  // Always true!
    settings.ShaderLayout = SubpixelLayout.RgbStripe;  // Always RGB!
    settings.ShaderIntensity = 1.0;  // Always 100%!
    
    return settings;
}
```

### Expected Behavior
Preview should:
1. Save **both** ClearType **and** shader injection state
2. Apply preview settings to **both** modes
3. Allow user to see shader changes in real-time
4. Properly revert **both** modes after 15 seconds

---

## Impact

### Issue 1 Impact
| Severity | HIGH |
|----------|------|
| **User confusion** | "Why didn't changing the layout do anything?" |
| **Wasted time** | Users must restart all apps to see changes |
| **Poor UX** | Defeats purpose of instant apply |
| **Bug reports** | "Settings don't work!" |

### Issue 2 Impact
| Severity | MEDIUM |
|----------|--------|
| **Limited testing** | Can't compare shader layouts |
| **Decision paralysis** | Can't choose best setting |
| **Frustration** | "How do I know what works?" |
| **Abandoned feature** | Users won't use preview |

---

## Solutions

### Solution 1: Live Config Reload in Hooked Processes

The native DLL already has `ConfigLoader` with `WatchForChanges()`:

```cpp
// dllmain.cpp - ALREADY HAS THIS!
void ConfigWatcherThreadFunc() {
    ConfigLoader::Instance().WatchForChanges([](const ShaderConfig& config) {
        Debug WriteLine(L"Config updated, applying changes...");
        DirectWriteHook::Instance().UpdateConfig(config);  // ? Updates live!
    });
}
```

**What we need to do:** NOTHING! The DLL already reloads config automatically!

**The real problem:** The `shader_config.ini` file format doesn't match what `ConfigLoader` expects.

**Current format (ShaderService.cs):**
```ini
[Shader]
Enabled=True
Layout=WrgbStripe
Intensity=0.8500
```

**Expected format (ConfigLoader.cpp):**
```ini
[SubpixelLayout]
Type=WRGB_STRIPE
Intensity=0.85
```

### Solution 2: Capture and Restore Full State for Preview

Need to:
1. Save **current shader injection state** before preview
2. Apply **both** shader and ClearType in preview
3. Restore **both** after timeout

---

## Implementation Plan

### Fix 1: Correct Config File Format

Update `ShaderService.WriteConfigFile()` to match what ConfigLoader expects.

### Fix 2: Add Live Config Update Trigger

After writing config, send a signal to hooked processes to reload.

### Fix 3: Full State Capture for Preview

Capture shader injection state, not just ClearType.

### Fix 4: Preview for Both Modes

Apply and revert both shader and ClearType settings.

---

**Status:** ?? CRITICAL
**Priority:** P0 (Must fix before release)
**Estimated Time:** 2-3 hours
