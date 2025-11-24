using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace DisplayShadersPowerToy.Helpers;

/// <summary>
/// Centralized logging system for debugging and diagnostics
/// Writes to both Debug output and a log file
/// </summary>
public static class DiagnosticLogger
{
    private static readonly string LogFilePath;
    private static readonly object LockObj = new object();
    private static bool _initialized = false;

    static DiagnosticLogger()
    {
        try
        {
            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DisplayShadersPowerToy",
                "Logs");

            Directory.CreateDirectory(logDir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            LogFilePath = Path.Combine(logDir, $"diagnostic_{timestamp}.log");

            _initialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiagnosticLogger] Failed to initialize: {ex.Message}");
            LogFilePath = Path.Combine(Path.GetTempPath(), "DisplayShadersPowerToy_diagnostic.log");
        }
    }

    public static void Log(string category, string message)
    {
        if (!_initialized) return;

        try
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] [{category}] {message}";

            // Write to Debug output
            Debug.WriteLine(logMessage);

            // Write to file
            lock (LockObj)
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
        }
        catch
        {
            // Fail silently - don't crash the app
        }
    }

    public static void LogError(string category, string message, Exception? ex = null)
    {
        string errorMsg = ex != null ? $"{message} - Exception: {ex}" : message;
        Log(category, $"ERROR: {errorMsg}");
    }

    public static void LogSystemInfo()
    {
        Log("System", "=== System Information ===");
        Log("System", $"OS: {Environment.OSVersion}");
        Log("System", $"64-bit OS: {Environment.Is64BitOperatingSystem}");
        Log("System", $"64-bit Process: {Environment.Is64BitProcess}");
        Log("System", $"Processor Count: {Environment.ProcessorCount}");
        Log("System", $"Machine Name: {Environment.MachineName}");
        Log("System", $"User Name: {Environment.UserName}");
        Log("System", $"CLR Version: {Environment.Version}");
        Log("System", $"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
        Log("System", "=========================");
    }

    public static void LogInjectionAttempt(int processId, string processName, bool success, string? error = null)
    {
        if (success)
        {
            Log("Injection", $"SUCCESS: {processName} (PID: {processId})");
        }
        else
        {
            Log("Injection", $"FAILED: {processName} (PID: {processId}) - {error ?? "Unknown error"}");
        }
    }

    public static void LogConfigUpdate(Models.DisplaySettings settings)
    {
        Log("Config", "=== Configuration Update ===");
        Log("Config", $"Shader Injection: {settings.EnableShaderInjection}");
        Log("Config", $"Shader Layout: {settings.ShaderLayout}");
        Log("Config", $"Shader Intensity: {settings.ShaderIntensity:F2}");
        Log("Config", "============================");
    }

    public static string GetLogFilePath() => LogFilePath;

    public static void OpenLogFile()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogFilePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiagnosticLogger] Failed to open log file: {ex.Message}");
        }
    }

    public static void OpenLogDirectory()
    {
        try
        {
            string? logDir = Path.GetDirectoryName(LogFilePath);
            if (logDir != null && Directory.Exists(logDir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logDir,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiagnosticLogger] Failed to open log directory: {ex.Message}");
        }
    }

    public static string GetFullLog()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                return File.ReadAllText(LogFilePath);
            }
        }
        catch
        {
            // Ignore
        }

        return "Log file not available";
    }
}
