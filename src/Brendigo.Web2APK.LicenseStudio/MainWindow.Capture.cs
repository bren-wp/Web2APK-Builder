#if UI_CAPTURE
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Brendigo.Web2APK.LicenseStudio;

/// <summary>Real WPF capture only for the isolated CI screenshot executable.</summary>
public partial class MainWindow
{
    private async void CaptureNativeStudioOnLoaded(object sender,RoutedEventArgs args)
    {
        Loaded-=CaptureNativeStudioOnLoaded;
        await Dispatcher.InvokeAsync(() => { },DispatcherPriority.ApplicationIdle);
        try
        {
            if(Content is not FrameworkElement root)
                throw new InvalidOperationException("License Studio content is not a WPF visual.");
            root.UpdateLayout();
            int width=(int)Math.Ceiling(root.ActualWidth);
            int height=(int)Math.Ceiling(root.ActualHeight);
            if(width<760 || height<530)
                throw new InvalidOperationException($"License Studio screenshot is clipped: {width}x{height}.");
            var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);
            bitmap.Render(root);
            var png=new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bitmap));
            var folder=Path.Combine(Environment.CurrentDirectory,"docs","screenshots","native");
            Directory.CreateDirectory(folder);
            using(var file=File.Create(Path.Combine(folder,"license-studio.png")))
                png.Save(file);
            Console.WriteLine($"Captured real License Studio at {width}x{height}.");
            Application.Current.Shutdown(0);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine("License Studio WPF capture failed: "+ex);
            Application.Current.Shutdown(1);
        }
    }
}
#endif
