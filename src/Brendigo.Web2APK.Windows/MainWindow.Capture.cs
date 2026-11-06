using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow
{
    private async void CaptureNativeScreensOnLoaded(object sender, RoutedEventArgs args)
    {
        Loaded -= CaptureNativeScreensOnLoaded;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        try
        {
            var folder = Path.Combine(Environment.CurrentDirectory,"docs","screenshots","native");
            Directory.CreateDirectory(folder);
            // Render real WPF controls, never illustrative/mock-generated screenshots.
            void Capture(string name)
            {
                var outputSize = new Size(1440,900);
                Root.Width=1440; Root.Height=900;
                Root.Measure(outputSize);
                Root.Arrange(new Rect(outputSize));
                Root.UpdateLayout();
                var img=new RenderTargetBitmap(1440,900,96,96,PixelFormats.Pbgra32);
                img.Render(Root);
                var png=new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(img));
                using var output=File.Create(Path.Combine(folder,name+".png"));
                png.Save(output);
            }
            _wizardStep=0;
            Navigate("dashboard");Capture("dashboard");
            for(int step=0;step<6;step++)
            {
                _wizardStep=step;
                Navigate("new");
                Capture("wizard-"+(step+1)+"-"+new[]{"basics","appearance","webview","features","permissions","build"}[step]);
            }
            foreach(var page in new[]{"projects","tools","devices","settings","docs","about"})
            {
                Navigate(page);
                Capture(page);
            }
            Console.WriteLine("Captured "+Directory.GetFiles(folder,"*.png").Length+" native WPF screenshots.");
            Application.Current.Shutdown(0);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine("Native WPF screenshot capture failed: "+ex);
            Application.Current.Shutdown(1);
        }
    }
}
