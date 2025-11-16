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
    private const int PreviewDurationSeconds = 15;
    private bool _isDarkMode = false;

    public MainWindow()
    {
        InitializeComponent();
        
        _displayShaderService = new DisplayShaderService();
        _settingsService = new SettingsService();
        
        // Load saved settings
        _currentSettings = _settingsService.LoadSettings();
        
        // Initialize UI with current settings
        InitializeUIFromSettings();
        
        // Setup system tray icon
        SetupSystemTray();
        
        // Apply light theme by default
        ApplyTheme(false);
        
        // Update preview text
        UpdatePreviewText();
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
        // Set subpixel layout radio buttons
        switch (_currentSettings.SubpixelLayout)
        {
            case SubpixelLayout.RgbStripe:
                rbRgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.WrgbStripe:
                rbWrgbStripe.IsChecked = true;
                break;
            case SubpixelLayout.RgbTriangular:
                rbRgbTriangular.IsChecked = true;
                break;
            case SubpixelLayout.Pentile:
                rbPentile.IsChecked = true;
                break;
            case SubpixelLayout.None:
                rbNone.IsChecked = true;
                break;
        }

        // Set shader settings
        cbEnableShader.IsChecked = _currentSettings.EnableShader;
        sliderIntensity.Value = _currentSettings.ShaderIntensity;
        sliderIntensity.IsEnabled = _currentSettings.EnableShader;

        // Set application settings
        cbStartWithWindows.IsChecked = _currentSettings.StartWithWindows;
        cbMinimizeToTray.IsChecked = _currentSettings.MinimizeToTray;
    }

    private void UpdatePreviewText()
    {
        string layoutName = _currentSettings.SubpixelLayout switch
        {
            SubpixelLayout.RgbStripe => "RGB Stripe",
            SubpixelLayout.WrgbStripe => "WRGB Stripe",
            SubpixelLayout.RgbTriangular => "RGB Triangular",
            SubpixelLayout.Pentile => "PenTile",
            SubpixelLayout.None => "None (ClearType Disabled)",
            _ => "Unknown"
        };

        string enabledStatus = _currentSettings.EnableShader ? "Enabled" : "Disabled";
        int intensityPercent = (int)(_currentSettings.ShaderIntensity * 100);
        
        runPreviewSettings.Text = $"{layoutName}, {enabledStatus}, Intensity: {intensityPercent}%";
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

    private void SubpixelLayout_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null) return;
        
        if (rbRgbStripe.IsChecked == true)
            _currentSettings.SubpixelLayout = SubpixelLayout.RgbStripe;
        else if (rbWrgbStripe.IsChecked == true)
            _currentSettings.SubpixelLayout = SubpixelLayout.WrgbStripe;
        else if (rbRgbTriangular.IsChecked == true)
            _currentSettings.SubpixelLayout = SubpixelLayout.RgbTriangular;
        else if (rbPentile.IsChecked == true)
            _currentSettings.SubpixelLayout = SubpixelLayout.Pentile;
        else if (rbNone.IsChecked == true)
            _currentSettings.SubpixelLayout = SubpixelLayout.None;
        
        UpdatePreviewText();
    }

    private void EnableShader_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null) return;
        
        _currentSettings.EnableShader = cbEnableShader.IsChecked == true;
        sliderIntensity.IsEnabled = _currentSettings.EnableShader;
        UpdatePreviewText();
    }

    private void ShaderIntensity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_currentSettings == null) return;
        
        _currentSettings.ShaderIntensity = sliderIntensity.Value;
        UpdatePreviewText();
    }

    private void StartWithWindows_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null) return;
        
        _currentSettings.StartWithWindows = cbStartWithWindows.IsChecked == true;
    }

    private void MinimizeToTray_Changed(object sender, RoutedEventArgs e)
    {
        if (_currentSettings == null) return;
        
        _currentSettings.MinimizeToTray = cbMinimizeToTray.IsChecked == true;
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

            // Update UI to show preview is active
            btnPreview.Content = $"⏱️ {PreviewDurationSeconds}s";
            btnPreview.IsEnabled = false;
            btnApply.IsEnabled = false;

            // Set up timer to revert changes
            int remainingSeconds = PreviewDurationSeconds;
            _previewTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            
            _previewTimer.Tick += (s, args) =>
            {
                remainingSeconds--;
                if (remainingSeconds <= 0)
                {
                    _previewTimer?.Stop();
                    _previewTimer = null;
                    RevertPreview();
                }
                else
                {
                    btnPreview.Content = $"⏱️ {remainingSeconds}s";
                }
            };
            
            _previewTimer.Start();

            System.Windows.MessageBox.Show(
                $"Preview applied! Settings will automatically revert in {PreviewDurationSeconds} seconds.\n\nLook at text in other applications to see the effect.",
                "Preview Active",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
            "This will restore Windows default ClearType settings.\n\n" +
            "Default settings:\n" +
            "• ClearType: Enabled\n" +
            "• Subpixel Layout: RGB Stripe (Standard)\n" +
            "• Contrast: 1400 (Standard)\n" +
            "• Orientation: RGB\n\n" +
            "Do you want to continue?",
            "Reset to Windows Defaults",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Cancel any active preview
                if (_previewTimer != null)
                {
                    _previewTimer.Stop();
                    _previewTimer = null;
                    _previewOriginalSettings = null;
                    btnPreview.Content = "👁️ Preview";
                    btnPreview.IsEnabled = true;
                    btnApply.IsEnabled = true;
                }

                // Restore Windows defaults
                _displayShaderService.RestoreWindowsDefaults();

                // Update UI to reflect default settings
                _currentSettings.EnableShader = true;
                _currentSettings.SubpixelLayout = SubpixelLayout.RgbStripe;
                _currentSettings.ShaderIntensity = 1.0;
                
                InitializeUIFromSettings();
                UpdatePreviewText();

                // Save the default settings
                _settingsService.SaveSettings(_currentSettings);

                System.Windows.MessageBox.Show(
                    "Windows default ClearType settings have been restored.\n\n" +
                    "You may need to restart applications for text rendering changes to take full effect.",
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
        finally
        {
            btnPreview.Content = "👁️ Preview";
            btnPreview.IsEnabled = true;
            btnApply.IsEnabled = true;
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Cancel any active preview
            if (_previewTimer != null)
            {
                _previewTimer.Stop();
                _previewTimer = null;
                _previewOriginalSettings = null;
                btnPreview.Content = "👁️ Preview";
                btnPreview.IsEnabled = true;
                btnApply.IsEnabled = true;
            }

            // Apply display shader settings
            _displayShaderService.ApplyShaderSettings(_currentSettings);

            // Save settings
            _settingsService.SaveSettings(_currentSettings);

            // Set startup with Windows
            _settingsService.SetStartWithWindows(_currentSettings.StartWithWindows);

            System.Windows.MessageBox.Show(
                "Settings applied successfully!\n\nYou may need to restart applications for text rendering changes to take full effect.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error applying settings:\n\n{ex.Message}",
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
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        base.OnClosed(e);
    }
}