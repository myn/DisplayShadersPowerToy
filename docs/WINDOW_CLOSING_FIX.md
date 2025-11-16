# Window Closing Behavior Fix

## Problem
When clicking the X button to close the application, it would minimize to the system tray instead of actually shutting down. This happened even when the "Minimize to system tray" checkbox was unchecked.

## Root Cause
Two issues were causing this behavior:

1. **Default Setting**: In `Models/DisplaySettings.cs`, the `MinimizeToTray` property defaulted to `true`:
   ```csharp
   public bool MinimizeToTray { get; set; } = true;  // ? Wrong default
   ```

2. **Incomplete Shutdown**: In `MainWindow_Closing`, even when the setting was false, the app would only dispose the tray icon but not call `Application.Shutdown()`:
   ```csharp
   else
   {
       _notifyIcon?.Dispose();
       // Missing: Application.Current.Shutdown()
   }
   ```

## Solution

### 1. Changed Default Setting
```csharp
public bool MinimizeToTray { get; set; } = false;  // ? Now defaults to false
```

### 2. Fixed Window_Closing Logic
```csharp
private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    // Revert any active preview
    if (_previewTimer != null)
    {
        _previewTimer.Stop();
        _previewTimer = null;
        RevertPreview();
    }

    // Only minimize to tray if the checkbox is ACTUALLY checked
    if (_currentSettings?.MinimizeToTray == true && cbMinimizeToTray.IsChecked == true)
    {
        e.Cancel = true;
        Hide();
    }
    else
    {
        // Actually close the application
        _notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();  // ? Explicitly shut down
    }
}
```

### 3. Improved Cleanup
```csharp
protected override void OnClosed(EventArgs e)
{
    _previewTimer?.Stop();
    _notifyIcon?.Dispose();
    _notifyIcon = null;  // ? Set to null after disposal
    base.OnClosed(e);
}
```

## Behavior Now

- **X Button Click**: 
  - ? **Default**: App closes and debugger stops
  - ?? **With "Minimize to tray" checked**: App minimizes to tray
  
- **System Tray ? Exit**: Always completely shuts down the app

- **Close Button**: Respects the "Minimize to tray" setting

## Testing Checklist
- [ ] Click X with "Minimize to tray" unchecked ? App closes completely
- [ ] Click X with "Minimize to tray" checked ? App minimizes to tray
- [ ] Right-click tray icon ? Exit ? App closes completely
- [ ] Debugger stops running when app is closed

## Files Modified
- `Models/DisplaySettings.cs` - Changed default value
- `MainWindow.xaml.cs` - Fixed closing logic and cleanup
