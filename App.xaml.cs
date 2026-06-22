using System.Threading;
using System.Windows;

namespace DisplayShadersPowerToy;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = @"Local\DisplayShadersPowerToy.SingleInstance";
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        var singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            singleInstanceMutex.Dispose();
            Shutdown();
            return;
        }

        _singleInstanceMutex = singleInstanceMutex;

        base.OnStartup(e);

        bool startMinimized = e.Args.Any(arg =>
            string.Equals(arg, "--minimized", StringComparison.OrdinalIgnoreCase));

        try
        {
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            if (startMinimized)
            {
                mainWindow.StartMinimized();
            }
            else
            {
                mainWindow.Show();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error loading settings on startup:\n\n{ex.Message}",
                "Display Shaders PowerToy",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;

        base.OnExit(e);
    }
}
