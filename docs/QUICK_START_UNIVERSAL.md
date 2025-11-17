# ?? Quick Reference: Universal Continuous Injection

## One-Time Setup

```powershell
# 1. Build (if not done)
.\build-complete.ps1

# 2. Run app
.\bin\Debug\net8.0-windows\DisplayShadersPowerToy.exe

# 3. In the app:
#    - Toggle "Shader Injection" ON (green)
#    - Click "Apply Settings"
#    - DONE! Monitoring starts automatically
```

## What Happens Automatically

```
? Injects into ALL current GUI processes
? Monitors every 2 seconds for new processes
? Auto-hooks new GUI apps within 2-3 seconds
? Cleans up dead processes automatically
? Continues until you disable it
```

## What Gets Hooked

```
? ANY app with a visible window
   - All browsers
   - All editors
   - All Office apps
   - All chat apps
   - All IDEs
   - All file managers
   - All games with UI
   - EVERYTHING with text!

? Excluded for safety
   - System processes (csrss, lsass, etc.)
   - Display drivers
   - Anti-cheat systems
   - Console apps without GUI
```

## Status Indicators

```
?? "Monitoring ALL processes - N hooked"
   ? Everything working perfectly
   ? N processes have shader hooks

?? "Monitoring ALL GUI processes"
   ? Monitoring active
   ? Waiting for GUI processes

?? "Disabled"
   ? Shader injection turned off
   ? No monitoring
```

## Quick Test

```powershell
# Run automated test
.\test-universal-injection.ps1

# Or manual test:
# 1. Note current count (e.g., "47 hooked")
# 2. Open Notepad
# 3. Wait 3 seconds
# 4. Count should increase to 48!
```

## Verify Injection

```powershell
# List all hooked processes
Get-Process | Where-Object {
    try { $_.Modules.ModuleName -contains "DisplayShaderHook.dll" } catch { $false }
} | Select ProcessName, Id
```

## Troubleshooting

```
Problem: Count not increasing
Fix: Check status shows "Monitoring", try opening GUI apps

Problem: Status shows 0 hooked
Fix: Open a GUI app, wait 2-3 seconds

Problem: Specific app not hooked
Fix: Check if it's blacklisted or console-only
```

## Key Features

```
? Universal: ALL GUI apps (not just a whitelist)
? Automatic: Continuous background monitoring
? Fast: 2-3 second detection for new processes
? Safe: Protected blacklist for system processes
? Efficient: Minimal CPU/memory overhead
? Complete: No manual intervention needed
```

## Files

- `Services/InjectionManager.cs` - Universal injection engine
- `Services/DisplayShaderService.cs` - Service integration
- `test-universal-injection.ps1` - Automated test
- `UNIVERSAL_INJECTION_COMPLETE.md` - Full documentation
- `UNIVERSAL_INJECTION_SUMMARY.md` - Detailed guide

---

**That's it! Enable once, enjoy forever.** ??
