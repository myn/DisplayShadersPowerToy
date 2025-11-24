using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;
using Hardcodet.Wpf.TaskbarNotification;

namespace DisplayShadersPowerToy;

/// <summary>
/// Modern, reimagined UI for OLED Text Optimizer
/// Simplified, cohesive design focused on the core functionality
/// </summary>
public partial class MainWindow : Window
{
    private readonly DisplayShaderService _displayShaderService;
    private readonly SettingsService _settingsService;
    private DisplaySettings _currentSettings;
    private DispatcherTimer? _statusUpdateTimer;
    private bool _isInitializing = true;
    private TaskbarIcon? _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        Helpers.DiagnosticLogger.LogSystemInfo();
        Helpers.DiagnosticLogger.Log("MainWindow", "Application starting (Modern UI)...");
        
        _displayShaderService = new DisplayShaderService();
        _settingsService = new SettingsService();
        
        // Load saved settings
        _currentSettings = _settingsService.LoadSettings();
        Helpers.DiagnosticLogger.LogConfigUpdate(_currentSettings);
        
        // Initialize UI
        InitializeUI();
        
        // Setup system tray
        SetupSystemTray();
        
        _isInitializing = false;
        
        // Apply settings
        ApplySettings();
        
        // Start status updates
        StartStatusUpdates();
        
        Helpers.DiagnosticLogger.Log("MainWindow", "Application initialized successfully (Modern UI)");
        Helpers.DiagnosticLogger.Log("MainWindow", $"Log file: {Helpers.DiagnosticLogger.GetLogFilePath()}");
    }

    private void InitializeUI()
    {
        // Set display type
        switch (_currentSettings.ShaderLayout)
        {
            case SubpixelLayout.RgbStripe:
                rbDisplayRgbLcd.IsChecked = true;
                break;
            case SubpixelLayout.WrgbStripe:
                rbDisplayWoled.IsChecked = true;
                break;
            case SubpixelLayout.RgbTriangular:
                rbDisplayQdOled.IsChecked = true;
                break;
            case SubpixelLayout.Pentile:
                rbDisplayPentile.IsChecked = true;
                break;
        }

        // Set intensity
        sliderIntensity.Value = _currentSettings.ShaderIntensity;
        UpdateIntensityDisplay();

        // Set toggles
        toggleQuickEnable.IsChecked = _currentSettings.EnableShaderInjection;
        toggleAutoInject.IsChecked = _currentSettings.EnableShaderInjection;
        cbStartWithWindows.IsChecked = _currentSettings.StartWithWindows;
        cbMinimizeToTray.IsChecked = _currentSettings.MinimizeToTray;

        // Set log path
        txtLogPath.Text = Helpers.DiagnosticLogger.GetLogFilePath();
    }

    private void SetupSystemTray()
    {
        try
        {
            _notifyIcon = new TaskbarIcon();
            _notifyIcon.Icon = Helpers.IconGenerator.GenerateTrayIcon();
            _notifyIcon.ToolTipText = "OLED Text Optimizer";
            
            // Double-click to show window
            _notifyIcon.TrayMouseDoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };
            
            // Context menu
            var contextMenu = new System.Windows.Controls.ContextMenu();
            
            var openItem = new MenuItem { Header = "Open" };
            openItem.Click += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };
            contextMenu.Items.Add(openItem);
            
            contextMenu.Items.Add(new Separator());
            
            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (s, e) =>
            {
                _notifyIcon?.Dispose();
                _notifyIcon = null;
                System.Windows.Application.Current.Shutdown();
            };
            contextMenu.Items.Add(exitItem);
            
            _notifyIcon.ContextMenu = contextMenu;
            
            Helpers.DiagnosticLogger.Log("MainWindow", "System tray icon initialized");
        }
        catch (Exception ex)
        {
            Helpers.DiagnosticLogger.LogError("MainWindow", "Failed to setup system tray", ex);
        }
    }

    private void ApplySettings()
    {
        Helpers.DiagnosticLogger.Log("MainWindow", "Applying settings...");
        
        // Apply via service
        _displayShaderService.ApplyShaderSettings(_currentSettings);
        
        // Save settings
        _settingsService.SaveSettings(_currentSettings);
        
        // Update status
        UpdateQuickStatus();
    }

    private void StartStatusUpdates()
    {
        _statusUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _statusUpdateTimer.Tick += (s, e) => UpdateQuickStatus();
        _statusUpdateTimer.Start();
    }

    private void UpdateQuickStatus()
    {
        try
        {
            int processCount = _displayShaderService.GetInjectedProcessCount();
            bool isActive = _currentSettings.EnableShaderInjection && processCount > 0;

            if (isActive)
            {
                txtQuickStatus.Text = $"Optimizing {processCount} application{(processCount == 1 ? "" : "s")}";
                txtQuickStatusDetail.Text = "Real-time shader injection active";
                
                // Show active processes card
                borderActiveProcesses.Visibility = Visibility.Visible;
                txtProcessCount.Text = $"{processCount} active";
                
                var processes = _displayShaderService.GetInjectedProcessNames();
                listActiveProcesses.ItemsSource = processes.Take(10); // Show max 10
            }
            else if (_currentSettings.EnableShaderInjection)
            {
                txtQuickStatus.Text = "Waiting for applications...";
                txtQuickStatusDetail.Text = "Ready to optimize new processes";
                borderActiveProcesses.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtQuickStatus.Text = "Optimization disabled";
                txtQuickStatusDetail.Text = "Toggle to enable real-time optimization";
                borderActiveProcesses.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            Helpers.DiagnosticLogger.LogError("MainWindow", "Error updating status", ex);
        }
    }

    private void UpdateIntensityDisplay()
    {
        txtIntensityValue.Text = $"{sliderIntensity.Value:P0}";
    }

    // Event Handlers
    private void QuickEnable_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.EnableShaderInjection = toggleQuickEnable.IsChecked == true;
        toggleAutoInject.IsChecked = _currentSettings.EnableShaderInjection;
        
        Helpers.DiagnosticLogger.Log("UI", $"Quick enable toggled: {_currentSettings.EnableShaderInjection}");
        ApplySettings();
    }

    private void DisplayType_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        if (rbDisplayRgbLcd.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.RgbStripe;
        else if (rbDisplayWoled.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.WrgbStripe;
        else if (rbDisplayQdOled.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.RgbTriangular;
        else if (rbDisplayPentile.IsChecked == true)
            _currentSettings.ShaderLayout = SubpixelLayout.Pentile;

        Helpers.DiagnosticLogger.Log("UI", $"Display type changed to: {_currentSettings.ShaderLayout}");
        ApplySettings();
    }

    private void Intensity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;

        _currentSettings.ShaderIntensity = sliderIntensity.Value;
        UpdateIntensityDisplay();
        
        Helpers.DiagnosticLogger.Log("UI", $"Intensity changed to: {_currentSettings.ShaderIntensity:F2}");
        ApplySettings();
    }

    private void AutoInject_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.EnableShaderInjection = toggleAutoInject.IsChecked == true;
        toggleQuickEnable.IsChecked = _currentSettings.EnableShaderInjection;
        
        Helpers.DiagnosticLogger.Log("UI", $"Auto-inject toggled: {_currentSettings.EnableShaderInjection}");
        ApplySettings();
    }

    private void StartWithWindows_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.StartWithWindows = cbStartWithWindows.IsChecked == true;
        _settingsService.SaveSettings(_currentSettings);
        
        // TODO: Implement startup registry entry
    }

    private void MinimizeToTray_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.MinimizeToTray = cbMinimizeToTray.IsChecked == true;
        _settingsService.SaveSettings(_currentSettings);
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
        if (_currentSettings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            _statusUpdateTimer?.Stop();
            _notifyIcon?.Dispose();
            _displayShaderService?.Dispose();
            Helpers.DiagnosticLogger.Log("MainWindow", "Application closing");
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
        _statusUpdateTimer?.Stop();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        base.OnClosed(e);
    }
}
