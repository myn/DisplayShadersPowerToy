using System.Windows;

namespace DisplayShadersPowerToy;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Check if started minimized
        bool startMinimized = e.Args.Length > 0 && e.Args.Contains("--minimized");

        // Apply saved settings on startup
        try
        {
            var settingsService = new Services.SettingsService();
            var displayShaderService = new Services.DisplayShaderService();
            
            var settings = settingsService.LoadSettings();
            displayShaderService.ApplyShaderSettings(settings);

            // If started minimized, don't show the main window
            if (startMinimized && MainWindow != null)
            {
                MainWindow.WindowState = WindowState.Minimized;
                MainWindow.Hide();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error loading settings on startup:\n\n{ex.Message}",
                "Display Shaders PowerToy",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}

