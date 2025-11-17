using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;

namespace DisplayShadersPowerToy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DisplayShaderService _displayShaderService;
    private readonly SettingsService _settingsService;
    private DisplaySettings _currentSettings;
    private DisplaySettings? _previewOriginalSettings;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private DispatcherTimer? _previewTimer;
    private DispatcherTimer? _statusUpdateTimer;
    private const int PreviewDurationSeconds = 15;
    private bool _isDarkMode = false;
    private bool _isInitializing = true;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize diagnostic logging
        Helpers.DiagnosticLogger.LogSystemInfo();
        Helpers.DiagnosticLogger.Log("MainWindow", "Application starting...");
        
        _displayShaderService = new DisplayShaderService();
        _settingsService = new SettingsService();
        
        // Load saved settings
        _currentSettings = _settingsService.LoadSettings();
        Helpers.DiagnosticLogger.LogConfigUpdate(_currentSettings);
        
        // Initialize UI with current settings
        InitializeUIFromSettings();
        
        // Setup system tray icon
        SetupSystemTray();
        
        // Apply light theme by default
        ApplyTheme(false);
        
        // Update preview text
        UpdatePreviewText();

        _isInitializing = false;
        
        // Apply saved settings to restore previous state
        // This will start monitoring if shader injection was enabled
        ApplySettingsImmediate();

        // Update status displays AFTER applying settings
        UpdateAllStatusDisplays();
        
        // Start automatic status updates AFTER initial setup
        StartAutomaticStatusUpdates();
        
        Helpers.DiagnosticLogger.Log("MainWindow", "Application initialized successfully");
        Helpers.DiagnosticLogger.Log("MainWindow", $"Log file: {Helpers.DiagnosticLogger.GetLogFilePath()}");
    }

    /// <summary>
    /// Start automatic status updates every second
    /// </summary>
    private void StartAutomaticStatusUpdates()
    {
        _statusUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        
        _statusUpdateTimer.Tick += (s, e) => UpdateAllStatusDisplays();
        _statusUpdateTimer.Start();
        
        System.Diagnostics.Debug.WriteLine("[MainWindow] Started automatic status updates");
    }

    /// <summary>
    /// Stop automatic status updates
    /// </summary>
    private void StopAutomaticStatusUpdates()
    {
        _statusUpdateTimer?.Stop();
        _statusUpdateTimer = null;
    }

    private void ApplyTheme(bool isDark)
    {
        _isDarkMode = isDark;
        
        if (isDark)
        {
            // Dark theme
            Resources["AppBackground"] = Resources["DarkBackground"];
            Resources["CardBackground"] = Resources["DarkCardBackground"];
            Resources["CardForeground"] = Resources["DarkForeground"];
            Resources["SecondaryForeground"] = Resources["DarkSecondaryForeground"];
            Resources["CardBorder"] = Resources["DarkBorder"];
            Resources["PreviewBackground"] = Resources["DarkCardBackground"];
            Resources["PreviewForeground"] = Resources["DarkForeground"];
            Resources["PreviewSecondaryForeground"] = Resources["DarkSecondaryForeground"];
            Resources["PreviewTipBackground"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66));
            Resources["PreviewTipBorder"] = Resources["DarkBorder"];
            Resources["ButtonBackground"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66));
            Resources["ButtonForeground"] = Resources["DarkForeground"];
            Resources["ButtonBorder"] = Resources["DarkBorder"];
        }
        else
        {
            // Light theme
            Resources["AppBackground"] = Resources["LightBackground"];
            Resources["CardBackground"] = Resources["LightCardBackground"];
            Resources["CardForeground"] = Resources["LightForeground"];
            Resources["SecondaryForeground"] = Resources["LightSecondaryForeground"];
            Resources["CardBorder"] = Resources["LightBorder"];
            Resources["PreviewBackground"] = new SolidColorBrush(Colors.White);
            Resources["PreviewForeground"] = Resources["LightForeground"];
            Resources["PreviewSecondaryForeground"] = Resources["LightSecondaryForeground"];
            Resources["PreviewTipBackground"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(249, 249, 249));
            Resources["PreviewTipBorder"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224));
            Resources["ButtonBackground"] = new SolidColorBrush(Colors.White);
            Resources["ButtonForeground"] = Resources["LightForeground"];
            Resources["ButtonBorder"] = Resources["LightBorder"];
        }
    }

    private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
    {
        ApplyTheme(toggleDarkMode.IsChecked == true);
    }

    private void InitializeUIFromSettings()
    {
        // SHADER INJECTION SETTINGS
        toggleShaderInjection.IsChecked = _currentSettings.EnableShaderInjection;
        
        switch (_currentSettings.ShaderLayout)
        {
            case SubpixelLayout.RgbStripe:
                rbShaderRgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.WrgbStripe:
                rbShaderWrgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.RgbTriangular:
                rbShaderRgbTriangular.IsChecked = true;
                break;
            case SubpixelLayout.Pentile:
                rbShaderPentile.IsChecked = true;
                break;
        }
        
        sliderShaderIntensity.Value = _currentSettings.ShaderIntensity;
        shaderInjectionPanel.IsEnabled = _currentSettings.EnableShaderInjection;

        // CLEARTYPE SETTINGS
        toggleClearType.IsChecked = _currentSettings.EnableClearType;
        
        switch (_currentSettings.ClearTypeLayout)
        {
            case SubpixelLayout.RgbStripe:
                rbClearTypeRgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.WrgbStripe:
                rbClearTypeWrgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.RgbTriangular:
                rbClearTypeRgbTriangular.IsChecked = true;
                break;
            case SubpixelLayout.Pentile:
                rbClearTypePentile.IsChecked = true;
                break;
            case SubpixelLayout.None:
                rbClearTypeNone.IsChecked = true;
                break;
        }

        sliderClearTypeIntensity.Value = _currentSettings.ClearTypeIntensity;
        clearTypePanel.IsEnabled = _currentSettings.EnableClearType;

        // APPLICATION SETTINGS
        cbStartWithWindows.IsChecked = _currentSettings.StartWithWindows;
        cbMinimizeToTray.IsChecked = _currentSettings.MinimizeToTray;
    }

    private void UpdatePreviewText()
    {
        var modes = new List<string>();
        
        if (_currentSettings.EnableShaderInjection)
        {
            string shaderLayout = _currentSettings.ShaderLayout switch
            {
                SubpixelLayout.RgbStripe => "RGB Stripe",
                SubpixelLayout.WrgbStripe => "WRGB Stripe",
                SubpixelLayout.RgbTriangular => "RGB Triangular",
                SubpixelLayout.Pentile => "PenTile",
                _ => "Unknown"
            };
            modes.Add($"Shader ({shaderLayout}, {_currentSettings.ShaderIntensity:P0})");
        }
        
        if (_currentSettings.EnableClearType)
        {
            string clearTypeLayout = _currentSettings.ClearTypeLayout switch
            {
                SubpixelLayout.RgbStripe => "RGB Stripe",
                SubpixelLayout.WrgbStripe => "WRGB Stripe",
                SubpixelLayout.RgbTriangular => "RGB Triangular",
                SubpixelLayout.Pentile => "PenTile",
                SubpixelLayout.None => "Disabled",
                _ => "Unknown"
            };
            modes.Add($"ClearType ({clearTypeLayout}, {_currentSettings.ClearTypeIntensity:P0})");
        }
        
        runPreviewSettings.Text = modes.Count > 0 ? string.Join(" + ", modes) : "None";
    }

    private void SetupSystemTray()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "Display Shaders PowerToy",
            Visible = true
        };

        // Generate and set icon programmatically
        try
        {
            _notifyIcon.Icon = Helpers.IconGenerator.GenerateTrayIcon();
        }
        catch
        {
            // If icon generation fails, try to load from file
            try
            {
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                }
            }
            catch
            {
                // Use default icon if everything else fails
            }
        }

        // Create context menu
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
        
        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    #region Shader Injection Event Handlers
    
    private void ShaderInjection_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.EnableShaderInjection = toggleShaderInjection.IsChecked == true;
        shaderInjectionPanel.IsEnabled = _currentSettings.EnableShaderInjection;
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
        UpdateAllStatusDisplays();
    }

    private void ShaderLayout_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        if (rbShaderRgbStripe.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.RgbStripe;
        else if (rbShaderWrgbStripe.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.WrgbStripe;
        else if (rbShaderRgbTriangular.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.RgbTriangular;
        else if (rbShaderPentile.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.Pentile;
        
        Helpers.DiagnosticLogger.Log("UI", $"Shader layout changed to: {_currentSettings.ShaderLayout}");
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
    }

    private void ShaderIntensity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.ShaderIntensity = sliderShaderIntensity.Value;
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
    }
    
    #endregion

    #region ClearType Event Handlers
    
    private void ClearType_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.EnableClearType = toggleClearType.IsChecked == true;
        clearTypePanel.IsEnabled = _currentSettings.EnableClearType;
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
        UpdateAllStatusDisplays();
    }

    private void ClearTypeLayout_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        if (rbClearTypeRgbStripe.IsChecked == true)
            _currentSettings.ClearTypeLayout = SubpixelLayout.RgbStripe;
        else if (rbClearTypeWrgbStripe.IsChecked == true)
            _currentSettings.ClearTypeLayout = SubpixelLayout.WrgbStripe;
        else if (rbClearTypeRgbTriangular.IsChecked == true)
            _currentSettings.ClearTypeLayout = SubpixelLayout.RgbTriangular;
        else if (rbClearTypePentile.IsChecked == true)
            _currentSettings.ClearTypeLayout = SubpixelLayout.Pentile;
        else if (rbClearTypeNone.IsChecked == true)
            _currentSettings.ClearTypeLayout = SubpixelLayout.None;
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
    }

    private void ClearTypeIntensity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.ClearTypeIntensity = sliderClearTypeIntensity.Value;
        
        // Apply immediately
        ApplySettingsImmediate();
        
        UpdatePreviewText();
    }
    
    #endregion

    #region Application Settings Event Handlers
    
    private void StartWithWindows_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.StartWithWindows = cbStartWithWindows.IsChecked == true;
        
        // Apply immediately
        _settingsService.SetStartWithWindows(_currentSettings.StartWithWindows);
        _settingsService.SaveSettings(_currentSettings);
    }

    private void MinimizeToTray_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null || _isInitializing) return;
        
        _currentSettings.MinimizeToTray = cbMinimizeToTray.IsChecked == true;
        
        // Save immediately
        _settingsService.SaveSettings(_currentSettings);
    }
    
    #endregion

    /// <summary>
    /// Apply settings immediately without user confirmation
    /// </summary>
    private async void ApplySettingsImmediate()
    {
        try
        {
            // Show status during async operation
            var wasEnabled = !_currentSettings.EnableShaderInjection;
            
            if (_currentSettings.EnableShaderInjection && wasEnabled)
            {
                // Enabling - show "Starting..." status
                runShaderModeStatus.Text = "Starting monitoring...";
                txtShaderProcessList.Text = "Initializing shader injection system...";
            }
            else if (!_currentSettings.EnableShaderInjection && !wasEnabled)
            {
                // Disabling - show "Stopping..." status
                runShaderModeStatus.Text = "Stopping monitoring...";
                txtShaderProcessList.Text = "Ejecting DLLs from hooked processes...";
            }
            
            // Apply display shader settings (async to not block UI)
            await Task.Run(() => _displayShaderService.ApplyShaderSettings(_currentSettings));

            // Save settings
            _settingsService.SaveSettings(_currentSettings);

            // Update status immediately after completion
            UpdateAllStatusDisplays();

            System.Diagnostics.Debug.WriteLine("[MainWindow] Settings applied automatically");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] Error applying settings: {ex.Message}");
            
            // Show error in UI
            runShaderModeStatus.Text = "Error";
            txtShaderProcessList.Text = $"Error applying settings: {ex.Message}";
        }
    }

    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Cancel any existing preview
            if (_previewTimer != null)
            {
                _previewTimer.Stop();
                _previewTimer = null;
                RevertPreview();
            }

            // Save current system settings
            _previewOriginalSettings = _displayShaderService.GetCurrentSettings();

            // Apply preview settings
            _displayShaderService.ApplyShaderSettings(_currentSettings);

            // Show notification
            System.Windows.MessageBox.Show(
                $"Preview applied! Settings will revert automatically in {PreviewDurationSeconds} seconds.",
                "Preview Active",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Set up timer to revert changes
            int remainingSeconds = PreviewDurationSeconds;
            _previewTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(PreviewDurationSeconds)
            };
            
            _previewTimer.Tick += (s, args) =>
            {
                _previewTimer?.Stop();
                _previewTimer = null;
                RevertPreview();
            };
            
            _previewTimer.Start();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error applying preview:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            RevertPreview();
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "This will restore default settings for both modes:\n\n" +
            "Shader Injection:\n" +
            "• Enabled\n" +
            "• RGB Stripe layout\n" +
            "• 100% intensity\n\n" +
            "ClearType:\n" +
            "• Enabled\n" +
            "• RGB Stripe layout\n" +
            "• 100% contrast\n\n" +
            "Do you want to continue?",
            "Reset All Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Restore defaults
                _displayShaderService.RestoreWindowsDefaults();

                // Reset to default settings
                _isInitializing = true;
                
                _currentSettings.EnableShaderInjection = true;
                _currentSettings.ShaderLayout = SubpixelLayout.RgbStripe;
                _currentSettings.ShaderIntensity = 1.0;
                
                _currentSettings.EnableClearType = true;
                _currentSettings.ClearTypeLayout = SubpixelLayout.RgbStripe;
                _currentSettings.ClearTypeIntensity = 1.0;
                
                InitializeUIFromSettings();
                UpdatePreviewText();
                
                _isInitializing = false;
                
                // Apply the defaults
                ApplySettingsImmediate();
                UpdateAllStatusDisplays();

                // Save the default settings
                _settingsService.SaveSettings(_currentSettings);

                System.Windows.MessageBox.Show(
                    "Default settings have been restored and applied.\n\n" +
                    "Monitoring is now active.",
                    "Reset Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error restoring default settings:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void RevertPreview()
    {
        try
        {
            if (_previewOriginalSettings != null)
            {
                _displayShaderService.ApplyShaderSettings(_previewOriginalSettings);
                _previewOriginalSettings = null;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error reverting preview:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSettings.MinimizeToTray)
        {
            Hide();
        }
        else
        {
            Close();
        }
    }

    private void ViewLog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Helpers.DiagnosticLogger.OpenLogFile();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Could not open log file:\n\n{ex.Message}\n\nLog location:\n{Helpers.DiagnosticLogger.GetLogFilePath()}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Helpers.DiagnosticLogger.OpenLogDirectory();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Could not open log folder:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Revert any active preview
        if (_previewTimer != null)
        {
            _previewTimer.Stop();
            _previewTimer = null;
            RevertPreview();
        }

        // Only minimize to tray if the checkbox is actually checked
        // Clicking X should always close the app unless explicitly configured otherwise
        if (_currentSettings?.MinimizeToTray == true && cbMinimizeToTray.IsChecked == true)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            // Actually close the application
            _notifyIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _currentSettings.MinimizeToTray)
        {
            Hide();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _previewTimer?.Stop();
        _statusUpdateTimer?.Stop();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        _displayShaderService?.Dispose();
        base.OnClosed(e);
    }

    private void UpdateAllStatusDisplays()
    {
        UpdateShaderInjectionStatus();
        UpdateClearTypeStatus();
    }

    private void UpdateShaderInjectionStatus()
    {
        bool dllAvailable = _displayShaderService.IsShaderModeAvailable();
        bool enabled = _currentSettings.EnableShaderInjection;
        
        if (!dllAvailable)
        {
            // DLL not available
            statusShaderBadge.Visibility = Visibility.Collapsed;
            runShaderModeStatus.Text = "DLL not found - install DisplayShaderHook.dll";
            txtShaderProcessList.Text = "Shader injection requires the native DLL to be built and placed in the application directory.";
            return;
        }
        
        statusShaderBadge.Visibility = Visibility.Visible;
        
        if (!enabled)
        {
            // Disabled
            runShaderStatus.Text = "Disabled";
            statusShaderBadge.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
            statusShaderBadge.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 189, 189));
            runShaderModeStatus.Text = "Disabled";
            txtShaderProcessList.Text = "Enable shader injection to automatically hook into ALL GUI applications";
            return;
        }
        
        int injectedCount = _displayShaderService.GetInjectedProcessCount();
        
        if (injectedCount > 0)
        {
            // Active and injecting
            runShaderStatus.Text = $"Active ({injectedCount})";
            statusShaderBadge.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
            statusShaderBadge.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
            runShaderModeStatus.Text = $"Monitoring ALL processes - {injectedCount} hooked";
            
            var processNames = _displayShaderService.GetInjectedProcessNames();
            if (processNames.Count <= 10)
            {
                txtShaderProcessList.Text = "Hooked processes:\n  • " + string.Join("\n  • ", processNames);
            }
            else
            {
                txtShaderProcessList.Text = "Hooked processes (top 10):\n  • " + 
                    string.Join("\n  • ", processNames.Take(10)) + 
                    $"\n  ... and {processNames.Count - 10} more";
            }
        }
        else
        {
            // Ready but not injecting yet
            runShaderStatus.Text = "Monitoring";
            statusShaderBadge.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 224));
            statusShaderBadge.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0));
            
            int version = ShaderService.GetHookVersion();
            runShaderModeStatus.Text = version > 0 ? $"Monitoring ALL GUI processes (v{version})" : "Monitoring ALL GUI processes";
            txtShaderProcessList.Text = "Continuous monitoring active - will automatically inject into any GUI application.\nWaiting for GUI processes to start...";
        }
    }

    private void UpdateClearTypeStatus()
    {
        bool enabled = _currentSettings.EnableClearType;
        
        if (enabled)
        {
            runClearTypeStatus.Text = "Enabled";
            statusClearTypeBadge.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 242, 253));
            statusClearTypeBadge.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));
        }
        else
        {
            runClearTypeStatus.Text = "Disabled";
            statusClearTypeBadge.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
            statusClearTypeBadge.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 189, 189));
        }
    }
}