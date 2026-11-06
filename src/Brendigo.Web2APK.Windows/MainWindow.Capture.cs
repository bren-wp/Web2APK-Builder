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
            var splash = new SplashWindow();
            splash.Show();
            splash.SplashRoot.Measure(new Size(790,490));
            splash.SplashRoot.Arrange(new Rect(0,0,790,490));
            splash.SplashRoot.UpdateLayout();
            var splashBitmap = new RenderTargetBitmap(790,490,96,96,PixelFormats.Pbgra32);
            splashBitmap.Render(splash.SplashRoot);
            var splashEncoder = new PngBitmapEncoder();
            splashEncoder.Frames.Add(BitmapFrame.Create(splashBitmap));
            using (var splashOut=File.Create(Path.Combine(folder,"splash.png"))) splashEncoder.Save(splashOut);
            splash.Close();
            Navigate("welcome"); Capture("welcome");
            _wizardStep=0;
            Navigate("dashboard");Capture("dashboard");
            for(int step=0;step<6;step++)
            {
                _wizardStep=step;
                Navigate("new");
                Capture("wizard-"+(step+1)+"-"+new[]{"basics","appearance","webview","features","permissions","build"}[step]);
            }
            foreach(var page in new[]{"projects","preview","tools","devices","settings","docs","about"})
            {
                Navigate(page);
                Capture(page);
            }

            // True layout smoke-test at additional window widths; no illustrative/generated artwork.
            void CheckCard(Border? card,string label)
            {
                if(card==null || card.ActualWidth<1 || PageHost.ActualWidth<1)
                    throw new InvalidOperationException("Missing responsive card: "+label);
                var location=card.TranslatePoint(new Point(0,0),PageHost);
                if(location.X < -2 || location.X+card.ActualWidth > PageHost.ActualWidth+2)
                    throw new InvalidOperationException("Horizontal UI clipping: "+label+
                        " x="+location.X+" width="+card.ActualWidth+
                        " available="+PageHost.ActualWidth);
            }
            void CaptureCompact(string page,string filename,int width,int height)
            {
                Navigate(page);
                Root.Width=width;Root.Height=height;
                Root.Measure(new Size(width,height));
                Root.Arrange(new Rect(0,0,width,height));
                Root.UpdateLayout();
                UpdateResponsive();
                Root.UpdateLayout();
                if(page=="new")
                {
                    CheckCard(_wizardMainCard,"wizard form at "+width);
                    CheckCard(_wizardSummaryCard,"wizard summary at "+width);
                }
                if(page=="preview")
                {
                    CheckCard(_previewLeftCard,"preview devices at "+width);
                    CheckCard(_previewCenterCard,"preview phone at "+width);
                    CheckCard(_previewRightCard,"preview properties at "+width);
                }
                var image=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);
                image.Render(Root);
                var encoder=new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using var output=File.Create(Path.Combine(folder,filename+".png"));
                encoder.Save(output);
            }
            _wizardStep=0;
            CaptureCompact("new","wizard-compact-1024",1024,740);
            CaptureCompact("preview","preview-compact-1024",1024,740);
            CaptureCompact("new","wizard-small-900",900,650);
            CaptureCompact("preview","preview-small-900",900,650);
            Root.Width=double.NaN;Root.Height=double.NaN;
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
