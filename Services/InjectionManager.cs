using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DisplayShadersPowerToy.Services;

/// <summary>
/// Manages UNIVERSAL injection of DisplayShaderHook.dll into ALL GUI processes
/// Continuously monitors and hooks new processes automatically
/// </summary>
public class InjectionManager : IDisposable
{
    private readonly HashSet<int> _injectedProcesses = new();
    private readonly Dictionary<int, IntPtr> _injectedModules = new(); // Store module handles for ejection
    private readonly HashSet<int> _failedProcesses = new(); // Track processes that failed injection
    private readonly HashSet<string> _systemProcessBlacklist;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;
    private bool _isMonitoring;
    private bool _disposed;
    
    // Cache for process enumeration to reduce overhead
    private Process[]? _cachedProcesses;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMilliseconds(500);
    private readonly object _cacheLock = new object();

    public InjectionManager()
    {
        // BLACKLIST ONLY: Critical system processes that should NEVER be injected
        _systemProcessBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Core system processes
            "system",
            "registry",
            "smss",
            "csrss",
            "wininit",
            "winlogon",
            "services",
            "lsass",
            "lsm",
            
            // Security processes
            "svchost",
            "dwm",           // Desktop Window Manager - can cause black screens if hooked incorrectly
            "runtimebroker",
            "securityhealthservice",
            "msmpeng",       // Windows Defender
            "nissrv",
            
            // Critical Windows processes
            "taskhostw",
            "sihost",
            "ctfmon",
            "fontdrvhost",
            "conhost",
            "systemsettings", // Windows Settings app - protected process
            
            // Display/Graphics drivers (can cause crashes)
            "nvcontainer",
            "nvcplui",
            "nvdisplay.container",
            "amdrsserv",
            "atieclxx",
            "atiesrxx",
            "igfxem",
            "igfxtray",
            
            // Anti-cheat systems (will ban if injected)
            "easyanticheat",
            "battleye",
            "vanguard",
            "faceit",
            
            // Our own process
            "displayshadersPowerToy",
        };

        Debug.WriteLine("[InjectionManager] Initialized - UNIVERSAL MODE");
        Debug.WriteLine($"[InjectionManager] System blacklist: {_systemProcessBlacklist.Count} processes");
    }

    /// <summary>
    /// Start continuous monitoring and auto-injection
    /// </summary>
    public void StartContinuousMonitoring()
    {
        if (_isMonitoring)
        {
            Debug.WriteLine("[InjectionManager] Already monitoring");
            return;
        }

        if (!ShaderService.IsHookDllAvailable())
        {
            Debug.WriteLine("[InjectionManager] Hook DLL not available, cannot start monitoring");
            return;
        }

        _monitoringCts = new CancellationTokenSource();
        _isMonitoring = true;

        _monitoringTask = Task.Run(async () =>
        {
            Debug.WriteLine("[InjectionManager] Started continuous monitoring");
            
            // Initial injection pass (async to not block)
            await Task.Run(() => InjectIntoAllProcesses());

            // Monitor for new processes
            while (!_monitoringCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(2000, _monitoringCts.Token); // Check every 2 seconds
                    
                    // Inject into any new processes
                    InjectIntoNewProcesses();
                    
                    // Cleanup dead processes
                    CleanupDeadProcesses();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[InjectionManager] Monitoring error: {ex.Message}");
                }
            }

            Debug.WriteLine("[InjectionManager] Stopped continuous monitoring");
        }, _monitoringCts.Token);
    }

    /// <summary>
    /// Stop continuous monitoring
    /// </summary>
    public void StopContinuousMonitoring()
    {
        if (!_isMonitoring)
        {
            return;
        }

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
        
        // Eject DLL from all injected processes (async to not block)
        Task.Run(() => EjectFromAllProcesses()).Wait(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Inject into ALL eligible processes (initial pass)
    /// </summary>
    public int InjectIntoAllProcesses()
    {
        return InjectIntoProcesses();
    }

    /// <summary>
    /// Inject into new processes that appeared since last check
    /// </summary>
    private int InjectIntoNewProcesses()
    {
        if (!ShaderService.IsHookDllAvailable())
        {
            return 0;
        }

        string dllPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DisplayShaderHook.dll");

        int injectedCount = 0;

        var processes = GetProcessesCached(); // Use cache
        if (processes == null || processes.Length == 0)
        {
            return 0;
        }

        foreach (var process in processes)
        {
            try
            {
                // Skip if already injected
                if (_injectedProcesses.Contains(process.Id))
                {
                    continue;
                }

                if (ShouldInjectIntoProcess(process))
                {
                    if (InjectDll(process, dllPath))
                    {
                        _injectedProcesses.Add(process.Id);
                        injectedCount++;
                        Debug.WriteLine($"[InjectionManager] Auto-injected into: {process.ProcessName} (PID: {process.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InjectionManager] Failed to process {process.ProcessName}: {ex.Message}");
                Helpers.DiagnosticLogger.LogError("InjectionManager", $"Failed to process {process.ProcessName}", ex);
            }
        }

        if (injectedCount > 0)
        {
            Debug.WriteLine($"[InjectionManager] Auto-injection: {injectedCount} new processes hooked");
            InvalidateProcessCache(); // Invalidate after injection
        }

        return injectedCount;
    }

    /// <summary>
    /// Inject hook DLL into all eligible processes
    /// </summary>
    public int InjectIntoProcesses()
    {
        if (!ShaderService.IsHookDllAvailable())
        {
            Debug.WriteLine("[InjectionManager] Hook DLL not available, cannot inject");
            Helpers.DiagnosticLogger.Log("InjectionManager", "Hook DLL not available, cannot inject");
            return 0;
        }

        string dllPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DisplayShaderHook.dll");

        Helpers.DiagnosticLogger.Log("InjectionManager", $"DLL Path: {dllPath}");
        Helpers.DiagnosticLogger.Log("InjectionManager", $"DLL Exists: {File.Exists(dllPath)}");

        int injectedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        var processes = GetProcessesCached();
        if (processes == null)
        {
            Helpers.DiagnosticLogger.LogError("InjectionManager", "GetProcessesCached returned null");
            return 0;
        }

        Debug.WriteLine($"[InjectionManager] Scanning {processes.Length} processes...");
        Helpers.DiagnosticLogger.Log("InjectionManager", $"Scanning {processes.Length} processes...");

        // Filter eligible processes first (fast operation)
        var eligibleProcesses = processes
            .Where(p => {
                try
                {
                    bool eligible = ShouldInjectIntoProcess(p);
                    if (!eligible) skippedCount++;
                    return eligible;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Helpers.DiagnosticLogger.LogError("InjectionManager", $"Error checking process eligibility", ex);
                    return false;
                }
            })
            .ToList();

        Debug.WriteLine($"[InjectionManager] Found {eligibleProcesses.Count} eligible processes");
        Helpers.DiagnosticLogger.Log("InjectionManager", $"Found {eligibleProcesses.Count} eligible processes");

        // Parallel injection for massive speedup
        var lockObj = new object();
        var parallelOptions = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) // Use up to 8 threads
        };

        Parallel.ForEach(eligibleProcesses, parallelOptions, process =>
        {
            try
            {
                if (InjectDll(process, dllPath))
                {
                    lock (lockObj)
                    {
                        _injectedProcesses.Add(process.Id);
                        injectedCount++;
                    }
                    Debug.WriteLine($"[InjectionManager] ? Injected: {process.ProcessName} (PID: {process.Id})");
                }
                else
                {
                    lock (lockObj)
                    {
                        errorCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InjectionManager] ? Error processing {process.ProcessName}: {ex.Message}");
                Helpers.DiagnosticLogger.LogError("InjectionManager", $"Error processing {process.ProcessName}", ex);
                lock (lockObj)
                {
                    errorCount++;
                }
            }
            finally
            {
                process.Dispose();
            }
        });

        // Dispose remaining processes
        foreach (var p in processes.Except(eligibleProcesses))
        {
            p.Dispose();
        }

        Debug.WriteLine($"[InjectionManager] Injection complete:");
        Debug.WriteLine($"  ? Injected: {injectedCount}");
        Debug.WriteLine($"  ? Skipped: {skippedCount}");
        Debug.WriteLine($"  ? Errors: {errorCount}");

        Helpers.DiagnosticLogger.Log("InjectionManager", $"Injection complete: {injectedCount} injected, {skippedCount} skipped, {errorCount} errors");

        return injectedCount;
    }

    /// <summary>
    /// Inject into a specific process by ID
    /// </summary>
    public bool InjectIntoProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            if (!ShouldInjectIntoProcess(process))
            {
                return false;
            }

            string dllPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "DisplayShaderHook.dll");

            if (InjectDll(process, dllPath))
            {
                _injectedProcesses.Add(processId);
                Debug.WriteLine($"[InjectionManager] Injected into specific process: {process.ProcessName} (PID: {processId})");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InjectionManager] Failed to inject into process {processId}: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Get all processes with caching to reduce overhead
    /// </summary>
    private Process[] GetProcessesCached()
    {
        lock (_cacheLock)
        {
            if (_cachedProcesses != null && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedProcesses;
            }
            
            // Cache expired or empty, refresh
            _cachedProcesses?.ToList().ForEach(p => p.Dispose());
            _cachedProcesses = Process.GetProcesses();
            _cacheExpiry = DateTime.UtcNow + _cacheLifetime;
            
            return _cachedProcesses;
        }
    }
    
    /// <summary>
    /// Invalidate process cache
    /// </summary>
    private void InvalidateProcessCache()
    {
        lock (_cacheLock)
        {
            _cachedProcesses?.ToList().ForEach(p => p.Dispose());
            _cachedProcesses = null;
            _cacheExpiry = DateTime.MinValue;
        }
    }

    /// <summary>
    /// UNIVERSAL FILTERING: Check if we should inject into this process
    /// Only skips critical system processes - everything else gets hooked!
    /// </summary>
    private bool ShouldInjectIntoProcess(Process process)
    {
        try
        {
            // Skip if already injected
            if (_injectedProcesses.Contains(process.Id))
            {
                return false;
            }

            // Skip if already failed
            if (_failedProcesses.Contains(process.Id))
            {
                return false;
            }

            // Skip Session 0 (system services) - prevents BSOD
            if (process.SessionId == 0)
            {
                return false;
            }

            // Skip our own process
            if (process.Id == Process.GetCurrentProcess().Id)
            {
                return false;
            }

            string processName = process.ProcessName.ToLowerInvariant();

            // Check system blacklist (critical processes only)
            if (_systemProcessBlacklist.Contains(processName))
            {
                return false;
            }

            // Skip processes without a main window (console apps, background services)
            // This automatically filters out most services while keeping ALL GUI apps
            if (process.MainWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            // HOOK EVERYTHING ELSE!
            // This includes:
            // - All browsers (Chrome, Firefox, Edge, Opera, Brave, etc.)
            // - All editors (Notepad, Notepad++, VS Code, Visual Studio, etc.)
            // - All Office apps (Word, Excel, PowerPoint, Outlook, etc.)
            // - All communication apps (Slack, Teams, Discord, Zoom, etc.)
            // - All file managers (Explorer, Total Commander, etc.)
            // - All IDEs (Visual Studio, Rider, IntelliJ, etc.)
            // - All games (with UI text)
            // - ALL other GUI applications!

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InjectionManager] Error checking process eligibility: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Inject DLL into process using CreateRemoteThread
    /// </summary>
    private bool InjectDll(Process process, string dllPath)
    {
        IntPtr hProcess = IntPtr.Zero;
        IntPtr allocMemAddress = IntPtr.Zero;

        try
        {
            Helpers.DiagnosticLogger.Log("InjectionManager", $"Attempting injection into {process.ProcessName} (PID: {process.Id})");

            // Open process with required access
            hProcess = NativeMethods.OpenProcess(
                ProcessAccessFlags.CreateThread |
                ProcessAccessFlags.VirtualMemoryOperation |
                ProcessAccessFlags.VirtualMemoryWrite,
                false,
                (uint)process.Id);

            if (hProcess == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Helpers.DiagnosticLogger.LogInjectionAttempt(
                    process.Id, 
                    process.ProcessName, 
                    false, 
                    $"OpenProcess failed with error {error}");
                return false;
            }

            // Allocate memory in target process for DLL path
            allocMemAddress = NativeMethods.VirtualAllocEx(
                hProcess,
                IntPtr.Zero,
                (uint)((dllPath.Length + 1) * Marshal.SizeOf<char>()),
                AllocationType.Commit | AllocationType.Reserve,
                MemoryProtection.ReadWrite);

            if (allocMemAddress == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Helpers.DiagnosticLogger.LogInjectionAttempt(
                    process.Id,
                    process.ProcessName,
                    false,
                    $"VirtualAllocEx failed with error {error}");
                return false;
            }

            // Write DLL path to allocated memory
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(dllPath);
            if (!NativeMethods.WriteProcessMemory(
                hProcess,
                allocMemAddress,
                bytes,
                (uint)bytes.Length,
                out _))
            {
                int error = Marshal.GetLastWin32Error();
                Helpers.DiagnosticLogger.LogInjectionAttempt(
                    process.Id,
                    process.ProcessName,
                    false,
                    $"WriteProcessMemory failed with error {error}");
                return false;
            }

            // Get address of LoadLibraryW
            IntPtr loadLibraryAddr = NativeMethods.GetProcAddress(
                NativeMethods.GetModuleHandle("kernel32.dll"),
                "LoadLibraryW");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Helpers.DiagnosticLogger.LogInjectionAttempt(
                    process.Id,
                    process.ProcessName,
                    false,
                    $"GetProcAddress(LoadLibraryW) failed with error {error}");
                return false;
            }

            // Create remote thread that calls LoadLibraryW with our DLL path
            IntPtr hThread = NativeMethods.CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                loadLibraryAddr,
                allocMemAddress,
                0,
                IntPtr.Zero);

            if (hThread == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Helpers.DiagnosticLogger.LogInjectionAttempt(
                    process.Id,
                    process.ProcessName,
                    false,
                    $"CreateRemoteThread failed with error {error}");
                return false;
            }

            // Wait for thread to complete
            uint waitResult = NativeMethods.WaitForSingleObject(hThread, 2000);
            
            bool success = waitResult == 0; // WAIT_OBJECT_0 = success
            
            if (success)
            {
                // Get the return value (module handle)
                if (NativeMethods.GetExitCodeThread(hThread, out uint moduleHandle))
                {
                    if (moduleHandle != 0)
                    {
                        // Store module handle for later ejection
                        _injectedModules[process.Id] = new IntPtr((int)moduleHandle);
                    }
                }
            }
            
            NativeMethods.CloseHandle(hThread);

            if (success)
            {
                Helpers.DiagnosticLogger.LogInjectionAttempt(process.Id, process.ProcessName, true);
            }
            else
            {
                // Track failed injection to prevent retry
                _failedProcesses.Add(process.Id);
                Helpers.DiagnosticLogger.LogInjectionAttempt(process.Id, process.ProcessName, false, "Thread wait timeout or failed");
            }

            return success;
        }
        catch (Exception ex)
        {
            // Track failed injection to prevent retry
            _failedProcesses.Add(process.Id);
            Helpers.DiagnosticLogger.LogError("InjectionManager", $"DLL injection failed for {process.ProcessName}", ex);
            return false;
        }
        finally
        {
            if (allocMemAddress != IntPtr.Zero && hProcess != IntPtr.Zero)
            {
                NativeMethods.VirtualFreeEx(hProcess, allocMemAddress, 0, FreeType.Release);
            }

            if (hProcess != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(hProcess);
            }
        }
    }

    /// <summary>
    /// Eject DLL from a specific process
    /// </summary>
    private bool EjectDll(int processId)
    {
        // Check if we have a module handle for this process
        if (!_injectedModules.TryGetValue(processId, out IntPtr hModule))
        {
            Debug.WriteLine($"[InjectionManager] No module handle found for PID {processId}");
            return false;
        }

        IntPtr hProcess = IntPtr.Zero;
        
        try
        {
            // Open process with required access
            hProcess = NativeMethods.OpenProcess(
                ProcessAccessFlags.CreateThread |
                ProcessAccessFlags.VirtualMemoryOperation |
                ProcessAccessFlags.QueryInformation,
                false,
                (uint)processId);

            if (hProcess == IntPtr.Zero)
            {
                Debug.WriteLine($"[InjectionManager] Failed to open process {processId} for ejection");
                return false;
            }

            // Get address of FreeLibrary
            IntPtr freeLibraryAddr = NativeMethods.GetProcAddress(
                NativeMethods.GetModuleHandle("kernel32.dll"),
                "FreeLibrary");

            if (freeLibraryAddr == IntPtr.Zero)
            {
                Debug.WriteLine($"[InjectionManager] Failed to get FreeLibrary address");
                return false;
            }

            // Create remote thread to call FreeLibrary with our module handle
            IntPtr hThread = NativeMethods.CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                freeLibraryAddr,
                hModule,
                0,
                IntPtr.Zero);

            if (hThread == IntPtr.Zero)
            {
                Debug.WriteLine($"[InjectionManager] Failed to create remote thread for ejection");
                return false;
            }

            // Wait for FreeLibrary to complete (reduced timeout)
            uint waitResult = NativeMethods.WaitForSingleObject(hThread, 1500); // Reduced from 5000ms to 1500ms
            
            // Get return value (TRUE if successful)
            bool success = false;
            if (waitResult == 0 && NativeMethods.GetExitCodeThread(hThread, out uint exitCode))
            {
                success = (exitCode != 0);
            }
            
            NativeMethods.CloseHandle(hThread);

            if (success)
            {
                // Remove from tracking
                _injectedModules.Remove(processId);
                Debug.WriteLine($"[InjectionManager] Successfully ejected from PID {processId}");
            }
            else
            {
                Debug.WriteLine($"[InjectionManager] FreeLibrary failed or timed out for PID {processId}");
            }

            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InjectionManager] Error ejecting from process {processId}: {ex.Message}");
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

    /// <summary>
    /// Eject DLL from all injected processes
    /// </summary>
    private void EjectFromAllProcesses()
    {
        if (_injectedProcesses.Count == 0)
        {
            Debug.WriteLine("[InjectionManager] No processes to eject from");
            return;
        }

        Debug.WriteLine($"[InjectionManager] Ejecting DLL from {_injectedProcesses.Count} processes...");
        
        int ejected = 0;
        int failed = 0;
        
        // Create a copy of the list to avoid modification during iteration
        var processIds = _injectedProcesses.ToList();
        
        // Parallel ejection for faster cleanup
        var lockObj = new object();
        var parallelOptions = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8)
        };

        Parallel.ForEach(processIds, parallelOptions, pid =>
        {
            try
            {
                if (EjectDll(pid))
                {
                    lock (lockObj)
                    {
                        ejected++;
                    }
                }
                else
                {
                    lock (lockObj)
                    {
                        failed++;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    failed++;
                }
                Debug.WriteLine($"[InjectionManager] Exception ejecting from PID {pid}: {ex.Message}");
            }
        });
        
        // Clear all tracking
        _injectedProcesses.Clear();
        _injectedModules.Clear();
        
        Debug.WriteLine($"[InjectionManager] Ejection complete:");
        Debug.WriteLine($"  ? Ejected: {ejected}");
        Debug.WriteLine($"  ? Failed: {failed}");
    }
    
    /// <summary>
    /// Get count of currently injected processes
    /// </summary>
    public int GetInjectedProcessCount()
    {
        CleanupDeadProcesses();
        return _injectedProcesses.Count;
    }

    /// <summary>
    /// Get list of injected process names
    /// </summary>
    public List<string> GetInjectedProcessNames()
    {
        var names = new List<string>();
        
        _injectedProcesses.RemoveWhere(pid =>
        {
            try
            {
                var process = Process.GetProcessById(pid);
                names.Add($"{process.ProcessName} (PID: {pid})");
                process.Dispose();
                return false; // Process still alive
            }
            catch
            {
                return true; // Process dead, remove it
            }
        });

        return names.OrderBy(n => n).ToList();
    }

    /// <summary>
    /// Remove dead processes from tracking
    /// </summary>
    public void CleanupDeadProcesses()
    {
        int removedInjected = _injectedProcesses.RemoveWhere(pid =>
        {
            try
            {
                var process = Process.GetProcessById(pid);
                process.Dispose();
                return false; // Still alive
            }
            catch
            {
                return true; // Dead, remove
            }
        });

        int removedFailed = _failedProcesses.RemoveWhere(pid =>
        {
            try
            {
                var process = Process.GetProcessById(pid);
                process.Dispose();
                return false; // Still alive
            }
            catch
            {
                return true; // Dead, remove
            }
        });

        if (removedInjected > 0 || removedFailed > 0)
        {
            Debug.WriteLine($"[InjectionManager] Cleaned up {removedInjected} dead injected processes and {removedFailed} dead failed processes");
        }
    }

    /// <summary>
    /// Clear all injected process tracking
    /// </summary>
    public void ClearInjectedProcesses()
    {
        _injectedProcesses.Clear();
        _failedProcesses.Clear();
        Debug.WriteLine("[InjectionManager] Cleared all injected and failed process tracking");
    }

    /// <summary>
    /// Get monitoring status
    /// </summary>
    public bool IsMonitoring => _isMonitoring;

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopContinuousMonitoring();
        _disposed = true;
        
        Debug.WriteLine("[InjectionManager] Disposed");
    }

    /// <summary>
    /// Native methods for process injection
    /// </summary>
    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            bool bInheritHandle,
            uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            FreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
    }

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        Terminate = 0x0001,
        CreateThread = 0x0002,
        VirtualMemoryOperation = 0x0008,
        VirtualMemoryRead = 0x0010,
        VirtualMemoryWrite = 0x0020,
        DuplicateHandle = 0x0040,
        SetInformation = 0x0200,
        QueryInformation = 0x0400,
        Synchronize = 0x00100000
    }

    [Flags]
    private enum AllocationType : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Reset = 0x80000,
        LargePages = 0x20000000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000
    }

    [Flags]
    private enum MemoryProtection : uint
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }

    [Flags]
    private enum FreeType : uint
    {
        Decommit = 0x4000,
        Release = 0x8000
    }
}
