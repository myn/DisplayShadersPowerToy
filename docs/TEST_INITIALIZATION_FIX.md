# ? Quick Test - Initialization Fix

## Issue Fixed

**Before:** App didn't start monitoring on launch, even if shader injection was enabled.

**After:** Monitoring starts automatically when app launches.

## How to Test

### Test 1: Fresh Launch with Monitoring Enabled

```powershell
# 1. Stop the running app (if any)

# 2. Rebuild
dotnet build

# 3. Run
.\bin\Debug\net8.0-windows\DisplayShadersPowerToy.exe

# 4. What you should see:
#    - Shader Injection toggle is ON (green)
#    - Status shows "Monitoring ALL processes - N hooked"
#    - Process list appears automatically
#    - No need to toggle anything!
```

### Test 2: Toggle Behavior

```powershell
# With app running:

# 1. Toggle Shader Injection OFF
#    ? Status should show "Disabled"
#    ? Process list should say "Enable shader injection..."

# 2. Toggle Shader Injection ON
#    ? Status should show "Monitoring..."
#    ? After 2 seconds: processes appear

# 3. Open Notepad
#    ? Wait 3 seconds
#    ? Notepad should appear in list

# 4. Close Notepad
#    ? Wait 3 seconds
#    ? Notepad should disappear from list
```

### Test 3: Auto-Update Verification

```powershell
# Monitor the debug output window while:

# 1. Opening new apps
#    ? Should see: "Auto-injected into: <app>"
#    ? Count should increase

# 2. Closing apps
#    ? Should see: "Cleaned up N dead processes"
#    ? Count should decrease
```

## Expected Results

### On App Launch
```
?? Shader Hook: Active (47)
Status: Monitoring ALL processes - 47 hooked

Hooked processes (top 10):
  • chrome (PID: 1234)
  • code (PID: 5678)
  • explorer (PID: 9012)
  ... and 37 more
```

### After Toggle OFF
```
? Shader Hook: Disabled
Status: Disabled

Enable shader injection to automatically hook into ALL GUI applications
```

### After Toggle ON Again
```
?? Shader Hook: Monitoring
Status: Monitoring ALL GUI processes

Continuous monitoring active - will automatically inject into any GUI application.
Waiting for GUI processes to start...

(After 2 seconds)

?? Shader Hook: Active (47)
Status: Monitoring ALL processes - 47 hooked
```

## What to Watch For

### ? Should Happen
- App launches with monitoring active
- Process list shows immediately
- Count updates automatically
- Toggle works correctly

### ? Should NOT Happen
- App shows "0 hooked" on launch (if enabled)
- Need to toggle OFF then ON to start
- Process list never updates
- Stale process data

## Debug Console

You should see:
```
[MainWindow] Started automatic status updates
[DisplayShaderService] ApplyShaderSettings called
  - Shader Injection: True
  - ClearType: True
[DisplayShaderService] Starting continuous monitoring for ALL GUI processes...
[InjectionManager] Started continuous monitoring
[InjectionManager] Scanning 247 processes...
[InjectionManager] ? Injected: 47
```

---

**If everything works: You're done! ??**

**If issues persist:** Check the debug output for errors.
