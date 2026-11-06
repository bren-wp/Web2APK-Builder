using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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
            // Render independently from the GitHub runner's ~1024x768 desktop.
            // This avoids black/clipped right and bottom areas in 1440x900 captures.
            if(!ReferenceEquals(Content,Root))
                throw new InvalidOperationException("Unexpected WPF root.");
            Content=null;
            // The runner's normal WPF Window is OS-limited to ~1044x788.
            // A raw HWND source keeps a full 1440x900 composition target.
            var parameters=new HwndSourceParameters("Brendigo Native Capture") {
                Width=1440, Height=900,
                PositionX=-3500, PositionY=-3500,
                WindowStyle=unchecked((int)0x80000000), // WS_POPUP without desktop clamping
                UsesPerPixelOpacity=false
            };
            using var captureSource=new HwndSource(parameters);
            captureSource.RootVisual=Root;
            var folder = Path.Combine(Environment.CurrentDirectory,"docs","screenshots","native");
            Directory.CreateDirectory(folder);
            var diagnostics=Path.Combine(Environment.CurrentDirectory,"docs","screenshots","native-progress.log");
            File.WriteAllText(diagnostics,"WPF capture started"+Environment.NewLine);
            // Render real WPF controls, never illustrative/mock-generated screenshots.
            void Reflow(int width,int height)
            {
                var bounds=new Size(width,height);
                // The HWND stays at 1440x900; the Root visual itself is laid out at the requested viewport.
                Root.Width=width;Root.Height=height;
                captureSource.Dispatcher.Invoke(()=>{ }, DispatcherPriority.Loaded);
                Root.InvalidateMeasure();
                Root.Measure(bounds);
                Root.Arrange(new Rect(bounds));
                Root.UpdateLayout();
                UpdateResponsive();
                captureSource.Dispatcher.Invoke(()=>{ }, DispatcherPriority.Render);
                Root.InvalidateMeasure();
                Root.Measure(bounds);
                Root.Arrange(new Rect(bounds));
                Root.UpdateLayout();
            }
            void Capture(string name)
            {
                File.AppendAllText(diagnostics,"Rendering "+name+Environment.NewLine);
                Reflow(1440,900);
                var img=new RenderTargetBitmap(1440,900,96,96,PixelFormats.Pbgra32);
                img.Render(Root);
                var png=new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(img));
                using var output=File.Create(Path.Combine(folder,name+".png"));
                png.Save(output);
                File.AppendAllText(diagnostics,"Size "+name+
                    ": Root "+Root.ActualWidth+" × "+Root.ActualHeight+
                    "; Capture "+captureSource.CompositionTarget?.TransformToDevice.M11+" DPI scale"+
                    "; Page "+PageHost.ActualWidth+" × "+PageHost.ActualHeight+
                    Environment.NewLine);
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
                    throw new InvalidOperationException("Missing responsive card: "+label+
                        "; cardNull="+(card==null)+"; cardActual="+(card?.ActualWidth ?? -1)+
                        "; cardWidth="+(card?.Width ?? -1)+"; pageActual="+PageHost.ActualWidth+
                        "; rootActual="+Root.ActualWidth+"; pageChildren="+PageHost.Children.Count);
                var location=card.TranslatePoint(new Point(0,0),PageHost);
                if(location.X < -2 || location.X+card.ActualWidth > PageHost.ActualWidth+2)
                    throw new InvalidOperationException("Horizontal UI clipping: "+label+
                        " x="+location.X+" width="+card.ActualWidth+
                        " available="+PageHost.ActualWidth);
            }
            void CaptureCompact(string page,string filename,int width,int height)
            {
                File.AppendAllText(diagnostics,"Rendering "+filename+Environment.NewLine);
                Reflow(width,height);
                Navigate(page);
                Dispatcher.Invoke(() => { }, DispatcherPriority.Loaded);
                Reflow(width,height);
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                Reflow(width,height);
                if(page=="new")
                {
                    CheckCard(_wizardMainCard,"wizard form at "+width);
                    CheckCard(_wizardSummaryCard,"wizard summary at "+width);
                }
                if(page is "welcome" or "dashboard" && _heroVisualColumn?.Width.Value > 0)
                    throw new InvalidOperationException("Hidden welcome/dashboard artwork keeps an empty column.");
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
            CaptureCompact("welcome","welcome-small-900",900,650);
            CaptureCompact("dashboard","dashboard-small-900",900,650);
            CaptureCompact("projects","projects-small-900",900,650);
            CaptureCompact("settings","settings-small-900",900,650);
            CaptureCompact("tools","tools-small-900",900,650);
            CaptureCompact("docs","docs-small-900",900,650);
            // Render high-density raster captures at the same 900x650 logical viewport.
            // This verifies WPF rendering at 125, 150 and 200 percent bitmap density,
            // but is not a substitute for physical per-monitor DPI or font-scale tests.
            foreach(var scale in new[]{125,150,200})
            {
                const int logicalWidth=900,logicalHeight=650;
                var dpi=96d*scale/100d;
                Reflow(logicalWidth,logicalHeight);
                _wizardStep=0;
                Navigate("new");
                Reflow(logicalWidth,logicalHeight);
                CheckCard(_wizardMainCard,"wizard form density "+scale);
                CheckCard(_wizardSummaryCard,"wizard summary density "+scale);
                var pixelsWide=(int)Math.Round(logicalWidth*scale/100d);
                var pixelsHigh=(int)Math.Round(logicalHeight*scale/100d);
                var bitmap=new RenderTargetBitmap(pixelsWide,pixelsHigh,dpi,dpi,
                    PixelFormats.Pbgra32);
                bitmap.Render(Root);
                if(bitmap.PixelWidth!=pixelsWide || bitmap.PixelHeight!=pixelsHigh)
                    throw new InvalidOperationException("Invalid DPI capture dimensions.");
                var encoder=new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var file=File.Create(Path.Combine(folder,
                    "wizard-density-"+scale+".png"));
                encoder.Save(file);
                File.AppendAllText(diagnostics,
                    "WPF density capture "+scale+"%: "+pixelsWide+" x "+pixelsHigh+
                    " pixels at "+dpi+" DPI; logical viewport 900 x 650"+Environment.NewLine);
            }
            Root.Width=double.NaN;Root.Height=double.NaN;
            captureSource.RootVisual=null;
            Console.WriteLine("Captured "+Directory.GetFiles(folder,"*.png").Length+" native WPF screenshots.");
            Application.Current.Shutdown(0);
        }
        catch(Exception ex)
        {
            try
            {
                var output=Path.Combine(Environment.CurrentDirectory,"docs","screenshots","native-error.log");
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                File.WriteAllText(output,ex.ToString());
            }
            catch(IOException) { }
            Console.Error.WriteLine("Native WPF screenshot capture failed: "+ex);
            Application.Current.Shutdown(1);
        }
    }
}
