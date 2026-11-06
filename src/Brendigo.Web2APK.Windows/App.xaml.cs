using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Brendigo.Web2APK.Windows;

public partial class App : Application
{
    private readonly bool _capture = Environment.GetCommandLineArgs().Contains("--capture-native");
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            CaptureFailure(args.Exception);
            args.Handled = true;
            Shutdown(1);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception) CaptureFailure(exception);
        };
        base.OnStartup(e);
        if (_capture)
        {
            var main = new MainWindow();
            MainWindow = main;
            main.Show();
            return;
        }
        var splash = new SplashWindow();
        MainWindow = splash;
        splash.Show();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1100) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                var main = new MainWindow();
                MainWindow = main;
                main.Show();
                splash.Close();
            }
            catch (Exception exception)
            {
                CaptureFailure(exception);
                Shutdown(1);
            }
        };
        timer.Start();
    }

    private void CaptureFailure(Exception error)
    {
        // Fail visibly, instead of silently returning success when WPF cannot initialize.
        var details = error.ToString();
        try
        {
            var directory = _capture ?
                Path.Combine(Environment.CurrentDirectory,"docs","screenshots") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Brendigo","Web2APK Builder");
            Directory.CreateDirectory(directory);
            var file=Path.Combine(directory,_capture ? "native-error.log" : "error.log");
            File.AppendAllText(file,DateTimeOffset.UtcNow.ToString("O")+"\n"+details+"\n\n");
        }
        catch(IOException) { }
        if (!_capture)
            MessageBox.Show("Aplikacija je naišla na pogrešku. Detalji su zapisani lokalno.",
                "Brendigo Web2APK Builder",MessageBoxButton.OK,MessageBoxImage.Error);
    }
}
