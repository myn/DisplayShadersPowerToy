using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DisplayShadersPowerToy.Models;
using DisplayShadersPowerToy.Services;
using Hardcodet.Wpf.TaskbarNotification;
using WinForms = System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;

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

    public ObservableCollection<MonitorViewModel> Monitors { get; set; } = new ObservableCollection<MonitorViewModel>();
    private MonitorViewModel? _selectedMonitor;

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
        // Initialize Monitors
        RefreshMonitors();

        // Set toggles
        toggleQuickEnable.IsChecked = _currentSettings.EnableShaderInjection;
        toggleAutoInject.IsChecked = _currentSettings.EnableShaderInjection;
        toggleClearType.IsChecked = _currentSettings.EnableClearType;
        UpdateToggleStatusDisplay();
        cbStartWithWindows.IsChecked = _currentSettings.StartWithWindows;
        cbMinimizeToTray.IsChecked = _currentSettings.MinimizeToTray;

        // Set log path
        txtLogPath.Text = Helpers.DiagnosticLogger.GetLogFilePath();
    }

    private void RefreshMonitors()
    {
        Monitors.Clear();
        var screens = WinForms.Screen.AllScreens;
        
        // Calculate bounding box of all screens to normalize coordinates for UI
        int minX = screens.Min(s => s.Bounds.X);
        int minY = screens.Min(s => s.Bounds.Y);
        int maxX = screens.Max(s => s.Bounds.Right);
        int maxY = screens.Max(s => s.Bounds.Bottom);
        
        double totalWidth = maxX - minX;
        double totalHeight = maxY - minY;
        
        // UI area for monitors (increased width to fill container)
        double uiWidth = 750;
        double uiHeight = 200;
        
        // Calculate scale to fit within UI area
        double scale = Math.Min(uiWidth / totalWidth, uiHeight / totalHeight) * 0.85;

        // Calculate centered offsets
        double scaledTotalWidth = totalWidth * scale;
        double scaledTotalHeight = totalHeight * scale;
        double offsetX = (uiWidth - scaledTotalWidth) / 2;
        double offsetY = (uiHeight - scaledTotalHeight) / 2;

        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];
            var monitor = new MonitorViewModel
            {
                DeviceName = screen.DeviceName,
                FriendlyName = $"Display {i + 1}",
                Index = i + 1,
                IsPrimary = screen.Primary,
                Bounds = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height),
                Left = (screen.Bounds.X - minX) * scale + offsetX + 20, // +20 for padding
                Top = (screen.Bounds.Y - minY) * scale + offsetY + 20,
                Width = screen.Bounds.Width * scale,
                Height = screen.Bounds.Height * scale
            };
            
            Monitors.Add(monitor);
        }

        icMonitors.ItemsSource = Monitors;

        // Select primary or first monitor
        var primary = Monitors.FirstOrDefault(m => m.IsPrimary) ?? Monitors.FirstOrDefault();
        if (primary != null)
        {
            SelectMonitor(primary);
        }
    }

    private void SelectMonitor(MonitorViewModel monitor)
    {
        if (_selectedMonitor != null) _selectedMonitor.IsSelected = false;
        _selectedMonitor = monitor;
        if (_selectedMonitor != null) _selectedMonitor.IsSelected = true;

        UpdateControlsForSelectedMonitor();
    }

    private void UpdateControlsForSelectedMonitor()
    {
        if (_selectedMonitor == null) return;

        // Get settings for this monitor
        if (!_currentSettings.MonitorSettings.TryGetValue(_selectedMonitor.DeviceName, out var settings))
        {
            // If no settings for this monitor, use default/legacy settings
            settings = new MonitorSettings 
            { 
                ShaderLayout = _currentSettings.ShaderLayout,
                ShaderIntensity = _currentSettings.ShaderIntensity
            };
            _currentSettings.MonitorSettings[_selectedMonitor.DeviceName] = settings;
        }

        _isInitializing = true; // Prevent triggering change events

        // Set display type
        switch (settings.ShaderLayout)
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
        sliderIntensity.Value = settings.ShaderIntensity;
        UpdateIntensityDisplay();

        _isInitializing = false;
        
        txtConfigHeader.Text = $"{_selectedMonitor.FriendlyName} Configuration";
    }

    private void Monitor_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is MonitorViewModel monitor)
        {
            SelectMonitor(monitor);
        }
    }

    private void Identify_Click(object sender, RoutedEventArgs e)
    {
        foreach (var screen in WinForms.Screen.AllScreens)
        {
            var idWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                Topmost = true,
                ShowInTaskbar = false,
                Left = screen.Bounds.Left,
                Top = screen.Bounds.Top,
                Width = screen.Bounds.Width,
                Height = screen.Bounds.Height,
                Content = new Border 
                { 
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 0, 0)),
                    Child = new TextBlock 
                    { 
                        Text = (Array.IndexOf(WinForms.Screen.AllScreens, screen) + 1).ToString(),
                        FontSize = 200,
                        Foreground = System.Windows.Media.Brushes.White,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        FontWeight = FontWeights.Bold
                    }
                }
            };
            idWindow.Show();
            
            // Close after 3 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, ev) => { idWindow.Close(); timer.Stop(); };
            timer.Start();
        }
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
        
        // Sync ClearType layout with shader layout if ClearType is enabled
        SyncClearTypeWithShaderSettings();
        
        // Apply via service
        _displayShaderService.ApplyShaderSettings(_currentSettings);
        
        // Save settings
        _settingsService.SaveSettings(_currentSettings);
        
        // Update status
        UpdateQuickStatus();
    }

    /// <summary>
    /// Synchronizes ClearType settings with shader settings
    /// ClearType layout and intensity follow the shader settings when ClearType is enabled
    /// </summary>
    private void SyncClearTypeWithShaderSettings()
    {
        if (_currentSettings.EnableClearType)
        {
            _currentSettings.ClearTypeLayout = _currentSettings.ShaderLayout;
            _currentSettings.ClearTypeIntensity = _currentSettings.ShaderIntensity;
        }
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

    private void UpdateToggleStatusDisplay()
    {
        if (toggleQuickEnable.IsChecked == true)
        {
            txtToggleStatus.Text = "ENABLED";
            txtToggleStatus.Foreground = (System.Windows.Media.Brush)FindResource("Success");
        }
        else
        {
            txtToggleStatus.Text = "DISABLED";
            txtToggleStatus.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary");
        }
    }

    // Event Handlers
    private void QuickEnable_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.EnableShaderInjection = toggleQuickEnable.IsChecked == true;
        toggleAutoInject.IsChecked = _currentSettings.EnableShaderInjection;
        UpdateToggleStatusDisplay();
        
        Helpers.DiagnosticLogger.Log("UI", $"Quick enable toggled: {_currentSettings.EnableShaderInjection}");
        ApplySettings();
    }

    private void DisplayType_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        SubpixelLayout layout = SubpixelLayout.RgbStripe;
        if (rbDisplayRgbLcd.IsChecked == true) layout = SubpixelLayout.RgbStripe;
        else if (rbDisplayWoled.IsChecked == true) layout = SubpixelLayout.WrgbStripe;
        else if (rbDisplayQdOled.IsChecked == true) layout = SubpixelLayout.RgbTriangular;
        else if (rbDisplayPentile.IsChecked == true) layout = SubpixelLayout.Pentile;

        // Update selected monitor settings
        if (_selectedMonitor != null)
        {
            if (!_currentSettings.MonitorSettings.TryGetValue(_selectedMonitor.DeviceName, out var settings))
            {
                settings = new MonitorSettings();
                _currentSettings.MonitorSettings[_selectedMonitor.DeviceName] = settings;
            }
            settings.ShaderLayout = layout;
            
            // Also update global legacy settings if this is primary
            if (_selectedMonitor.IsPrimary)
            {
                _currentSettings.ShaderLayout = layout;
            }
        }

        Helpers.DiagnosticLogger.Log("UI", $"Display type changed to: {layout} for monitor {_selectedMonitor?.FriendlyName}");
        ApplySettings();
    }

    private void Intensity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;

        double intensity = sliderIntensity.Value;
        UpdateIntensityDisplay();
        
        // Update selected monitor settings
        if (_selectedMonitor != null)
        {
            if (!_currentSettings.MonitorSettings.TryGetValue(_selectedMonitor.DeviceName, out var settings))
            {
                settings = new MonitorSettings();
                _currentSettings.MonitorSettings[_selectedMonitor.DeviceName] = settings;
            }
            settings.ShaderIntensity = intensity;

            // Also update global legacy settings if this is primary
            if (_selectedMonitor.IsPrimary)
            {
                _currentSettings.ShaderIntensity = intensity;
            }
        }
        
        Helpers.DiagnosticLogger.Log("UI", $"Intensity changed to: {intensity:F2} for monitor {_selectedMonitor?.FriendlyName}");
        ApplySettings();
    }

    private void AutoInject_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.EnableShaderInjection = toggleAutoInject.IsChecked == true;
        toggleQuickEnable.IsChecked = _currentSettings.EnableShaderInjection;
        UpdateToggleStatusDisplay();
        
        Helpers.DiagnosticLogger.Log("UI", $"Auto-inject toggled: {_currentSettings.EnableShaderInjection}");
        ApplySettings();
    }

    private void ClearType_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        _currentSettings.EnableClearType = toggleClearType.IsChecked == true;
        
        // Note: SyncClearTypeWithShaderSettings will be called in ApplySettings
        
        Helpers.DiagnosticLogger.Log("UI", $"ClearType toggled: {_currentSettings.EnableClearType}");
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

    /// <summary>
    /// DEVELOPER ONLY: Force eject all DLLs from hooked processes
    /// WARNING: Will cause crashes! Only use when rebuilding/deleting DLLs
    /// </summary>
    private void ForceEjectDlls_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "?? WARNING: Force Eject DLLs ??\n\n" +
            "This will forcibly unload DisplayShaderHook.dll from all hooked processes.\n\n" +
            "CONSEQUENCES:\n" +
            "� Applications will likely crash or freeze\n" +
            "� You may lose unsaved work\n" +
            "� Windows Explorer may need to restart\n\n" +
            "Only proceed if you need to rebuild or delete the DLL files during development.\n\n" +
            "Do you want to continue?",
            "Developer Tool - Force Eject DLLs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Helpers.DiagnosticLogger.Log("MainWindow", "?? User initiated force DLL ejection");
                _displayShaderService.ForceEjectAllDlls();
                
                System.Windows.MessageBox.Show(
                    "DLL ejection complete.\n\n" +
                    "Some applications may have crashed.\n" +
                    "The DLL file should now be unlocked for deletion/rebuild.",
                    "Force Eject Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                // Update status
                UpdateQuickStatus();
            }
            catch (Exception ex)
            {
                Helpers.DiagnosticLogger.LogError("MainWindow", "Force eject failed", ex);
                System.Windows.MessageBox.Show(
                    $"Force eject failed:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

public class MonitorViewModel : System.ComponentModel.INotifyPropertyChanged
{
    public string DeviceName { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public int Index { get; set; }
    public bool IsPrimary { get; set; }
    public Rect Bounds { get; set; }
    
    // For UI scaling/positioning
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    
    private bool _isSelected;
    public bool IsSelected 
    { 
        get => _isSelected; 
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } 
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}
