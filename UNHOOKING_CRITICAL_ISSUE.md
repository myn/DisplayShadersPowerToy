# ? CRITICAL ISSUE: No Unhooking/Cleanup on Disable or Exit

## Problem Identified

The system does **NOT** properly unhook or restore default settings when:
1. Shader injection is disabled
2. Application is exited
3. Process is terminated

## Current Behavior

### When Shader Injection is Disabled

**C# Side (MainWindow.xaml.cs):**
```csharp
private void ShaderInjection_Changed(object sender, RoutedEventArgs e)
{
    _currentSettings.EnableShaderInjection = toggleShaderInjection.IsChecked == true;
    ApplySettingsImmediate(); // ? Calls DisplayShaderService
}
```

**Service Side (DisplayShaderService.cs):**
```csharp
public void ApplyShaderSettings(DisplaySettings settings)
{
    if (settings.EnableShaderInjection && _shaderModeAvailable)
    {
        ApplyRealShaderSettings(settings);
    }
    else if (_injectionManager != null)
    {
        // ? Stops monitoring
        _injectionManager.StopContinuousMonitoring();
    }
    // ? MISSING: Unload DLL from injected processes!
}
```

**What Happens:**
- ? Monitoring stops (no new processes hooked)
- ? **DLL remains loaded in all previously injected processes**
- ? **Hooks still active in those processes**
- ? **Shader configuration still being applied**

### When Application Exits

**C# Side (MainWindow.xaml.cs):**
```csharp
protected override void OnClosed(EventArgs e)
{
    _previewTimer?.Stop();
    _statusUpdateTimer?.Stop();
    _notifyIcon?.Dispose();
    _notifyIcon = null;
    _displayShaderService?.Dispose(); // ? Disposes service
    base.OnClosed(e);
}
```

**Service Disposal (DisplayShaderService.cs):**
```csharp
public void Dispose()
{
    _injectionManager?.Dispose(); // ? Disposes injection manager
    _shaderService?.Dispose();
}
```

**Injection Manager Disposal (InjectionManager.cs):**
```csharp
public void Dispose()
{
    if (_disposed) return;
    
    StopContinuousMonitoring(); // ? Stops monitoring
    _disposed = true;
    
    // ? MISSING: Eject DLL from processes!
    // ? MISSING: Restore original functions!
}
```

**What Happens:**
- ? Monitoring stops
- ? **DLL remains loaded in all processes**
- ? **Hooks remain active**
- ? **Processes keep using shader rendering**

### When Injected Process Exits

**Native DLL Side (dllmain.cpp):**
```cpp
case DLL_PROCESS_DETACH:
    // Stop config watcher if running
    g_running.store(false, std::memory_order_release);
    if (g_configWatcherThread.joinable()) {
        g_configWatcherThread.join();
    }

    // Shutdown components if initialized
    if (g_initialized.load(std::memory_order_acquire)) {
        try {
            DirectWriteHook::Instance().Shutdown(); // ? Unhooks
            SubpixelShader::Instance().Shutdown();
            ConfigLoader::Instance().Shutdown();
        }
        catch (...) {
            // Ignore exceptions during shutdown
        }
    }
    break;
```

**What Happens:**
- ? **When a hooked process exits, DLL cleans up properly**
- ? Hooks are removed
- ? Resources are freed
- ? This part works correctly!

## The Problem

### Scenario 1: User Disables Shader Injection

```
1. User has 47 processes hooked
2. User toggles shader injection OFF
3. Monitoring stops (no new processes hooked)
4. ? 47 processes still have DLL loaded
5. ? 47 processes still apply shader rendering
6. ? User expects text to return to normal
7. ? Text stays modified until processes restart
```

### Scenario 2: User Closes Application

```
1. User has 47 processes hooked
2. User closes DisplayShadersPowerToy
3. Manager process exits
4. ? 47 processes still have DLL loaded
5. ? 47 processes still apply shader rendering
6. ? No way to control settings anymore
7. ? Users must restart all hooked processes
```

### Scenario 3: Application Crashes

```
1. User has 47 processes hooked
2. App crashes or is force-terminated
3. ? 47 processes still have DLL loaded
4. ? 47 processes still apply shader rendering
5. ? No config file updates will be read
6. ? Text stuck in shader mode until restart
```

## What Should Happen

### On Disable

```
1. User toggles shader injection OFF
2. InjectionManager.StopContinuousMonitoring() called
3. ? For each injected process:
   a. Call DLL export: UnhookAndEject()
   b. DLL removes all hooks
   c. DLL calls FreeLibrary on itself
   d. Process returns to normal rendering
4. ? Clear _injectedProcesses HashSet
5. ? Text immediately returns to default ClearType
```

### On Application Exit

```
1. User closes application
2. DisplayShaderService.Dispose() called
3. InjectionManager.Dispose() called
4. ? For each injected process:
   a. Eject DLL properly
   b. Restore original functions
5. ? All processes return to normal
6. ? Clean exit, no orphaned hooks
```

## Missing Functionality

### 1. DLL Ejection

**What's Missing:**
- No `FreeLibrary` call to unload DLL
- No mechanism to eject from remote process
- No cleanup of allocated memory in remote process

**What's Needed:**
```csharp
// InjectionManager.cs
private bool EjectDll(int processId)
{
    IntPtr hProcess = IntPtr.Zero;
    
    try
    {
        hProcess = NativeMethods.OpenProcess(
            ProcessAccessFlags.CreateThread |
            ProcessAccessFlags.VirtualMemoryOperation |
            ProcessAccessFlags.QueryInformation,
            false,
            (uint)processId);
        
        if (hProcess == IntPtr.Zero) return false;
        
        // Get handle to our DLL in the remote process
        IntPtr hModule = GetRemoteModuleHandle(hProcess, "DisplayShaderHook.dll");
        if (hModule == IntPtr.Zero) return false;
        
        // Get address of FreeLibrary
        IntPtr freeLibraryAddr = NativeMethods.GetProcAddress(
            NativeMethods.GetModuleHandle("kernel32.dll"),
            "FreeLibrary");
        
        if (freeLibraryAddr == IntPtr.Zero) return false;
        
        // Create remote thread to call FreeLibrary
        IntPtr hThread = NativeMethods.CreateRemoteThread(
            hProcess,
            IntPtr.Zero,
            0,
            freeLibraryAddr,
            hModule,
            0,
            IntPtr.Zero);
        
        if (hThread != IntPtr.Zero)
        {
            NativeMethods.WaitForSingleObject(hThread, 5000);
            NativeMethods.CloseHandle(hThread);
            return true;
        }
        
        return false;
    }
    finally
    {
        if (hProcess != IntPtr.Zero)
        {
            NativeMethods.CloseHandle(hProcess);
        }
    }
}
```

### 2. GetRemoteModuleHandle Helper

```csharp
private IntPtr GetRemoteModuleHandle(IntPtr hProcess, string moduleName)
{
    // Would need to enumerate modules in remote process
    // Using EnumProcessModulesEx or CreateToolhelp32Snapshot
    // This is complex and requires careful implementation
    
    // Simplified approach: Use well-known module address
    // Or store the base address during injection
    return IntPtr.Zero; // Placeholder
}
```

### 3. Eject All on Disable/Exit

```csharp
public void StopContinuousMonitoring()
{
    if (!_isMonitoring) return;
    
    Debug.WriteLine("[InjectionManager] Stopping continuous monitoring");
    _monitoringCts?.Cancel();
    
    try
    {
        _monitoringTask?.Wait(5000);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[InjectionManager] Error stopping monitoring: {ex.Message}");
    }
    
    _isMonitoring = false;
    _monitoringCts?.Dispose();
    _monitoringCts = null;
    
    // ? NEW: Eject DLL from all injected processes
    EjectFromAllProcesses();
}

private void EjectFromAllProcesses()
{
    Debug.WriteLine($"[InjectionManager] Ejecting DLL from {_injectedProcesses.Count} processes");
    
    int ejected = 0;
    int failed = 0;
    
    foreach (var pid in _injectedProcesses.ToList())
    {
        try
        {
            if (EjectDll(pid))
            {
                ejected++;
                Debug.WriteLine($"[InjectionManager] Ejected from PID {pid}");
            }
            else
            {
                failed++;
                Debug.WriteLine($"[InjectionManager] Failed to eject from PID {pid}");
            }
        }
        catch (Exception ex)
        {
            failed++;
            Debug.WriteLine($"[InjectionManager] Error ejecting from PID {pid}: {ex.Message}");
        }
    }
    
    _injectedProcesses.Clear();
    
    Debug.WriteLine($"[InjectionManager] Ejection complete: {ejected} succeeded, {failed} failed");
}
```

### 4. Update Dispose

```csharp
public void Dispose()
{
    if (_disposed) return;
    
    // Stop monitoring AND eject from all processes
    StopContinuousMonitoring(); // ? Now includes ejection
    
    _disposed = true;
    Debug.WriteLine("[InjectionManager] Disposed");
}
```

## Implementation Priority

### Critical (Must Fix)

1. ? **Implement DLL ejection** on disable
2. ? **Implement DLL ejection** on application exit
3. ? **Store module base address** during injection for easy ejection
4. ? **Add cleanup** to Dispose methods

### Important (Should Fix)

1. ?? **Graceful degradation** if ejection fails
2. ?? **Timeout handling** for stuck processes
3. ?? **Error reporting** when ejection fails

### Nice to Have

1. ?? **Progress indication** during mass ejection
2. ?? **Partial success** handling (some eject, some don't)
3. ?? **Force eject** option for stuck processes

## Risks of Current Implementation

### 1. Orphaned Hooks

**Problem:**
- Hooks remain active after app closes
- No way to disable them
- Users confused why text still looks different

**Impact:**
- Medium to High
- User must manually restart all affected apps

### 2. Memory Leaks

**Problem:**
- DLL remains loaded indefinitely
- Allocated memory in remote process not freed
- Config watcher thread keeps running

**Impact:**
- Low (minimal memory per process)
- But multiplied by 47+ processes = significant

### 3. Resource Contention

**Problem:**
- Config file watcher still running
- File handle held open
- Unnecessary CPU cycles

**Impact:**
- Very Low
- But wasteful and unprofessional

### 4. User Confusion

**Problem:**
- Toggle OFF doesn't immediately revert text
- Must restart apps to see changes
- No feedback that cleanup is needed

**Impact:**
- High
- Poor user experience

## Recommended Solution

### Phase 1: Immediate Fix (Essential)

1. **Store module handle during injection**
   ```csharp
   private struct InjectedProcess
   {
       public int ProcessId;
       public IntPtr ModuleHandle;
   }
   
   private Dictionary<int, IntPtr> _injectedProcesses = new();
   ```

2. **Implement basic ejection**
   ```csharp
   private bool EjectDll(int pid, IntPtr hModule)
   {
       // Use stored module handle
       // Call FreeLibrary via remote thread
   }
   ```

3. **Call on disable/exit**
   ```csharp
   StopContinuousMonitoring() // Already calls this
   {
       // ...existing code...
       EjectFromAllProcesses();
   }
   ```

### Phase 2: Robust Cleanup (Important)

1. **Handle ejection failures gracefully**
2. **Add user notification** if some processes couldn't be cleaned
3. **Log which processes failed** for debugging

### Phase 3: Enhanced UX (Nice to Have)

1. **Progress bar** during mass ejection
2. **"Force Cleanup"** button for manual intervention
3. **List of orphaned processes** if any remain

## Testing Plan

### Test 1: Disable Works

```
1. Enable shader injection
2. Wait for processes to be hooked
3. Toggle shader injection OFF
4. Verify:
   - ? Monitoring stopped
   - ? DLL ejected from all processes
   - ? _injectedProcesses cleared
   - ? Text rendering returns to normal immediately
```

### Test 2: Exit Cleans Up

```
1. Enable shader injection
2. Wait for processes to be hooked
3. Close application
4. Verify:
   - ? All DLLs ejected
   - ? No orphaned hooks
   - ? Text rendering normal in all apps
```

### Test 3: Crash Recovery

```
1. Enable shader injection
2. Wait for processes to be hooked
3. Kill application forcefully (Task Manager)
4. Expected:
   - ? DLLs remain loaded (unavoidable)
   - ? Hooks remain active (unavoidable)
5. Mitigation:
   - Provide "cleanup" tool
   - Or instructions to restart affected apps
```

## Summary

| Scenario | Current Behavior | Expected Behavior | Status |
|----------|-----------------|-------------------|--------|
| Toggle OFF | Monitoring stops, DLLs stay loaded | DLLs ejected, hooks removed | ? BROKEN |
| App Exit | Monitoring stops, DLLs stay loaded | DLLs ejected, hooks removed | ? BROKEN |
| App Crash | DLLs stay loaded | DLLs stay loaded (unavoidable) | ?? LIMITATION |
| Process Exit | DLL cleans up properly | DLL cleans up properly | ? WORKS |

**VERDICT:** ?? **CRITICAL BUG** - Must be fixed before release

The current implementation leaves orphaned DLLs and active hooks when:
- User disables shader injection
- User closes the application

This creates a poor user experience and violates the principle of least surprise.

---

**Status:** ? CRITICAL ISSUE IDENTIFIED
**Priority:** P0 (Must fix immediately)
**Estimated Fix Time:** 2-4 hours
**Risk:** High (affects all users)
