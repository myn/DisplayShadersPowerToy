# ?? QUICK START GUIDE - Shader Injection System

## ? Run Everything in 30 Seconds

```powershell
# 1. Build everything
.\build-complete.ps1

# 2. Test everything
.\test-complete-system.ps1

# 3. Monitor injection
.\monitor-injection.ps1
```

## ?? Status Badge Quick Reference

| Badge | Meaning | What to Do |
|-------|---------|------------|
| ![Blue Badge](https://via.placeholder.com/15/E8F4F8/000000?text=+) **ClearType Optimization** | ClearType mode active | Settings working! |
| ![Orange](https://via.placeholder.com/15/FF9800/000000?text=+) **• Shader DLL Ready** | DLL present, not injecting | Click Apply to inject |
| ![Green](https://via.placeholder.com/15/4CAF50/000000?text=+) **• X processes hooked** | Shaders actively running | Hover to see list |

## ?? Three Ways to Use This App

### 1. ClearType Mode (Works Now)
```
? No DLL needed
? Works immediately
? Improves OLED text via registry
?? Limited effectiveness
```

### 2. Shader Mode - Manual (DLL Present)
```
? DLL detected automatically
? Click Apply to inject
? True subpixel rendering
? Full OLED optimization
```

### 3. Shader Mode - Auto (Future)
```
? Auto-inject on app launch
? Monitor new processes
? Always-on shader mode
```

## ?? Test Checklist

### Before Testing
- [ ] Build completed successfully
- [ ] DLL present (optional, but recommended)
- [ ] Notepad or Chrome open
- [ ] Notepad has some text

### During Test
- [ ] Badge shows correct initial status
- [ ] Can select subpixel layout
- [ ] Can adjust intensity slider
- [ ] Apply button works
- [ ] Status badge updates
- [ ] Process count shows (if DLL present)
- [ ] Tooltip shows process list
- [ ] Success message appears

### After Test
- [ ] Settings persist after restart
- [ ] Text rendering changed (visible effect)
- [ ] No crashes or errors
- [ ] Status remains accurate

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Badge stays blue | DLL not present - build Native project |
| Injection count 0 | No whitelisted processes - open Notepad |
| Badge doesn't update | Restart app or check Debug output |
| DLL not loading | Check dependencies (d3d11.dll, etc.) |
| Build failed | Check VS 2022 installed with C++ workload |

## ?? File Map

| File | Purpose |
|------|---------|
| `build-complete.ps1` | Build C# + Native |
| `test-complete-system.ps1` | Full system test |
| `monitor-injection.ps1` | Watch DLL loading |
| `Services\InjectionManager.cs` | Process injection |
| `Services\DisplayShaderService.cs` | Mode manager |
| `MainWindow.xaml.cs` | UI logic |

## ?? UI Elements

### Status Badge (Top)
- **Location:** Under app title
- **Purpose:** Shows active mode
- **States:** Blue, Blue+Orange, Green
- **Interactive:** Hover for details

### Settings Section (Left)
- **Subpixel Layout:** Radio buttons
- **Enable Checkbox:** Toggle optimization
- **Intensity Slider:** 0-100%
- **Apply Button:** Triggers injection

### Info Box (Left)
- **Blue background:** Current mode info
- **Text:** Explains what's happening
- **Updates:** When mode changes

## ?? Configuration Files

### shader_config.ini
```ini
[Shader]
Enabled=True
Layout=WrgbStripe
Intensity=0.8000
```

**Location:** `bin\Debug\net8.0-windows\shader_config.ini`  
**Created:** When Apply clicked  
**Read by:** DisplayShaderHook.dll

### Registry
```
HKCU\Control Panel\Desktop\
  FontSmoothing = "2"
  FontSmoothingType = 2
  FontSmoothingGamma = 800
```

**Modified:** Every Apply click  
**Used by:** Windows ClearType

## ?? PowerShell Commands

### Check DLL Presence
```powershell
Test-Path "bin\Debug\net8.0-windows\DisplayShaderHook.dll"
```

### List Injected Processes
```powershell
Get-Process | Where-Object {
    $_.Modules.ModuleName -contains "DisplayShaderHook.dll"
} | Select ProcessName, Id
```

### Watch Config File
```powershell
Get-Content "bin\Debug\net8.0-windows\shader_config.ini" -Wait
```

### Monitor ClearType
```powershell
Get-ItemProperty "HKCU:\Control Panel\Desktop" | Select FontSmoothing*
```

## ?? Expected Results

### With DLL
```
Initial State:
  Badge: Blue + Orange
  Text: "ClearType Optimization"
  Indicator: "• Shader DLL Ready"

After Apply:
  Badge: Green
  Text: "Display Shaders (Active)"
  Indicator: "• 1 processes hooked"
  Tooltip: "notepad (PID: 12345)"
```

### Without DLL
```
Initial State:
  Badge: Blue only
  Text: "ClearType Optimization"
  Indicator: None

After Apply:
  Badge: Blue only (no change)
  Text: "ClearType Optimization"
  Registry: Updated
  Message: "ClearType settings updated"
```

## ?? Pro Tips

1. **Open target apps BEFORE clicking Apply**
   - App scans running processes
   - Can't inject into apps that aren't running

2. **Use monitor-injection.ps1 for debugging**
   - Shows real-time DLL loading
   - Updates every 2 seconds
   - Press Ctrl+C to stop

3. **Check Debug output in VS**
   - See detailed injection logs
   - Identify why injection failed
   - Track process lifecycle

4. **Hover over indicators**
   - Green: Shows process list
   - Orange: Shows instructions
   - Detailed tooltips

5. **Restart apps for effects**
   - Text rendering updates on repaint
   - Close and reopen for full effect
   - Or type new text to see changes

## ?? Success Indicators

### You know it's working when:
- ? Build completes without errors
- ? App launches successfully
- ? Badge shows appropriate status
- ? Apply button updates status
- ? Process count increases
- ? Tooltip shows process names
- ? No error messages
- ? Text rendering changes visible

## ?? Getting Help

### Check These First
1. **Build logs** - Look for errors
2. **Debug output** - See injection attempts
3. **Event Viewer** - Check for crashes
4. **Task Manager** - Verify DLL loaded

### Common Issues
- **DLL not found** ? Build Native project
- **Injection fails** ? Check admin rights
- **No visual change** ? DLL may need implementation
- **App crashes** ? Check error logs

## ?? Deploy Checklist

### Before Deployment
- [ ] Both C# and Native build successfully
- [ ] All tests pass
- [ ] DLL copied to bin folder
- [ ] Injection tested with Notepad
- [ ] Status badge updates correctly
- [ ] No memory leaks
- [ ] No crashes on error
- [ ] Documentation complete

### Deployment Steps
1. Build Release configuration
2. Bundle DLL with executable
3. Create installer
4. Test on clean system
5. Deploy to users

---

**Quick Ref Version:** 1.0  
**Last Updated:** 2024  
**Full Docs:** See `COMPLETE_IMPLEMENTATION.md`
