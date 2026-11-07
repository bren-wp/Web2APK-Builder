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
            // Exhaustive per-pixel comparison against the previous real WPF captures.
            // Design concept art is NOT used as a false pixel-exact baseline.
            var pixelAudit=new List<object>();
            void CompareToPreviousCapture(string name,BitmapSource current)
            {
                var previousPath=Path.Combine(folder,name+".png");
                if(!File.Exists(previousPath))return; // New viewport, no baseline yet.
                using var stream=File.OpenRead(previousPath);
                var decoder=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);
                var before=decoder.Frames[0];
                if(before.PixelWidth!=current.PixelWidth || before.PixelHeight!=current.PixelHeight)
                    throw new InvalidOperationException(
                        "Pixel audit: differing viewport for "+name);
                var oldBitmap=new FormatConvertedBitmap(before,PixelFormats.Bgra32,null,0);
                var newBitmap=new FormatConvertedBitmap(current,PixelFormats.Bgra32,null,0);
                var width=current.PixelWidth;
                var height=current.PixelHeight;
                var stride=checked(width*4);
                var bytes=checked(stride*height);
                var oldPixels=new byte[bytes];
                var newPixels=new byte[bytes];
                oldBitmap.CopyPixels(oldPixels,stride,0);
                newBitmap.CopyPixels(newPixels,stride,0);
                long changed=0;
                for(int i=0;i<bytes;i+=4)
                {
                    var delta=Math.Abs((int)oldPixels[i]-newPixels[i])+
                        Math.Abs((int)oldPixels[i+1]-newPixels[i+1])+
                        Math.Abs((int)oldPixels[i+2]-newPixels[i+2]);
                    if(delta>54)changed++;
                }
                var total=(long)width*height;
                var ratio=(double)changed/total;
                pixelAudit.Add(new {
                    screen=name, width, height, changedPixels=changed, comparedPixels=total,
                    changedFraction=Math.Round(ratio,6), thresholdRgbDelta=54,
                    baseline="previous actual WPF screenshot",
                    pixelPerfectAgainstExternalDesignReference=false
                });
                File.AppendAllText(diagnostics,
                    "Full pixel diff "+name+": "+changed+"/"+total+" ("+
                    ratio.ToString("P2",System.Globalization.CultureInfo.InvariantCulture)+
                    ")"+Environment.NewLine);
                if(ratio>0.55)
                    throw new InvalidOperationException(
                        "Major unexpected full-pixel UI regression on "+name+
                        " ("+ratio.ToString("P2",System.Globalization.CultureInfo.InvariantCulture)+")");
            }
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
                CompareToPreviousCapture(name,img);
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
            CompareToPreviousCapture("splash",splashBitmap);
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
            void AssertCompactChrome(string label,int width)
            {
                bool narrow=width<1020;
                if(Math.Abs(Root.ActualWidth-width)>2)
                    throw new InvalidOperationException("Wrong viewport width: "+label);
                if(narrow && (SidebarColumn.Width.Value!=82 ||
                    BrandWords.Visibility!=Visibility.Collapsed ||
                    SidebarFooter.Visibility!=Visibility.Collapsed ||
                    SidebarBrandArea.ActualWidth>SidebarColumn.ActualWidth+2 ||
                    NavigationHost.Children.OfType<TextBlock>()
                        .Any(c=>c.Visibility!=Visibility.Collapsed)))
                    throw new InvalidOperationException(
                        "Clipped compact sidebar or visible labels: "+label);
                if(narrow && HeaderContent.Margin.Left!=20)
                    throw new InvalidOperationException(
                        "Unresponsive compact header padding: "+label);
                if(width<1060 && SdkStatusBadge.Visibility!=Visibility.Collapsed)
                    throw new InvalidOperationException(
                        "SDK badge conflicts with compact header: "+label);
            }

            void AssertVisibleControlsInsidePage(string label)
            {
                void Visit(DependencyObject parent)
                {
                    for(int index=0;index<VisualTreeHelper.GetChildrenCount(parent);index++)
                    {
                        var child=VisualTreeHelper.GetChild(parent,index);
                        if(child is FrameworkElement visual && visual.IsVisible &&
                            visual.ActualWidth>1 &&
                            (visual is Button or TextBox or ComboBox or CheckBox))
                        {
                            bool insideScrollableStepper=false;
                            // Stepper overflow is intentional and accessible via its
                            // horizontal ScrollViewer; do not treat offscreen steps as
                            // broken controls. All other forms remain strictly checked.
                            for(DependencyObject? ancestor=child;
                                ancestor!=null && !ReferenceEquals(ancestor,PageHost);
                                ancestor=VisualTreeHelper.GetParent(ancestor))
                            {
                                if(ancestor is ScrollViewer scroll &&
                                    scroll.HorizontalScrollBarVisibility is
                                        ScrollBarVisibility.Auto or ScrollBarVisibility.Visible &&
                                    scroll.ExtentWidth>scroll.ViewportWidth+1)
                                {
                                    insideScrollableStepper=true;
                                    break;
                                }
                            }
                            var pos=visual.TranslatePoint(new Point(0,0),PageHost);
                            if(!insideScrollableStepper &&
                                (pos.X< -3 || pos.X+visual.ActualWidth>PageHost.ActualWidth+4))
                                throw new InvalidOperationException(
                                    "Horizontal clipping on "+label+": "+visual.GetType().Name+
                                    " left="+pos.X+" width="+visual.ActualWidth+
                                    " page="+PageHost.ActualWidth);
                        }
                        Visit(child);
                    }
                }
                Visit(PageHost);
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
                AssertCompactChrome(filename,width);
                AssertVisibleControlsInsidePage(filename);
                if(page=="new")
                {
                    CheckCard(_wizardMainCard,"wizard form at "+width);
                    CheckCard(_wizardSummaryCard,"wizard summary at "+width);
                }
                if(page is "welcome" or "dashboard" && _heroVisualColumn?.Width.Value > 0)
                    throw new InvalidOperationException("Hidden welcome/dashboard artwork keeps an empty column.");
                if(page=="preview")
                {
                    if(_previewWebToggle is null || _previewWebToggle.IsChecked!=false)
                        throw new InvalidOperationException(
                            "Preview must start with WebView preview disabled, not misleadingly checked.");
                    CheckCard(_previewLeftCard,"preview devices at "+width);
                    CheckCard(_previewCenterCard,"preview phone at "+width);
                    CheckCard(_previewRightCard,"preview properties at "+width);
                }
                var image=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);
                image.Render(Root);
                CompareToPreviousCapture(filename,image);
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
            CaptureCompact("devices","devices-small-900",900,650);
            CaptureCompact("about","about-small-900",900,650);
            // Inspect each of the six real wizard screens at the compact viewport.
            for(int step=0;step<6;step++)
            {
                _wizardStep=step;
                CaptureCompact("new","wizard-"+(step+1)+"-small-900",900,650);
            }
            // Verify every primary screen at the minimum supported window size,
            // rather than only checking wizard and preview at 900 px.
            foreach(var page in new[]{"welcome","dashboard","projects","preview",
                "tools","devices","settings","docs","about"})
                CaptureCompact(page,page+"-minimum-840",840,570);
            for(int step=0;step<6;step++)
            {
                _wizardStep=step;
                CaptureCompact("new","wizard-"+(step+1)+"-minimum-840",840,570);
                CaptureCompact("new","wizard-"+(step+1)+"-compact-1024",1024,740);
            }
            // Capture actual device-state switches and splash visibility,
            // not only the default phone composition.
            _previewDevice="tablet";
            CaptureCompact("preview","preview-tablet-small-900",900,650);
            _previewDevice="offline";
            CaptureCompact("preview","preview-offline-small-900",900,650);
            _previewDevice="phone";
            _previewSplash=false;
            CaptureCompact("preview","preview-no-splash-small-900",900,650);
            _previewSplash=true;
            CaptureCompact("preview","preview-phone-minimum-840",840,570);

            // Capture the actual locally generated Android source browser at
            // desktop and minimum sizes without synthetic product screenshots.
            var sourceFixture=new Brendigo.Web2APK.Core.ProjectDefinition {
                Name="Brendigo Source Viewer",
                PackageName="com.brendigo.sourcepreview",
                Url="https://example.com",
                JavaScript=true
            };
            var sourceWindow=new SourceViewerWindow(sourceFixture);
            try
            {
                sourceWindow.Show();
                foreach(var (windowWidth,windowHeight,shotName) in new[]{
                    (1130,790,"source-viewer"),
                    (760,520,"source-viewer-minimum-760")
                })
                {
                    sourceWindow.Width=windowWidth;
                    sourceWindow.Height=windowHeight;
                    Dispatcher.Invoke(()=>{},DispatcherPriority.Loaded);
                    sourceWindow.UpdateLayout();
                    if(sourceWindow.Content is not Grid contentGrid)
                        throw new InvalidOperationException("Source viewer content is not a Grid.");
                    var sourceContent=contentGrid.Children.OfType<Grid>().FirstOrDefault();
                    var footer=contentGrid.Children.OfType<Grid>().LastOrDefault();
                    if(sourceContent is null || footer is null)
                        throw new InvalidOperationException("Missing source viewer panels.");
                    var files=sourceContent.Children.OfType<ListBox>().FirstOrDefault();
                    var sourceText=sourceContent.Children.OfType<TextBox>().FirstOrDefault();
                    if(files is null || sourceText is null || files.Items.Count==0)
                        throw new InvalidOperationException("Source viewer files were not generated.");
                    if(sourceText.ActualWidth<180 || files.ActualWidth<150)
                        throw new InvalidOperationException("Source viewer columns clipped: "+shotName);
                    foreach(var action in footer.Children.OfType<WrapPanel>()
                        .SelectMany(panel=>panel.Children.OfType<Button>()))
                    {
                        var pos=action.TranslatePoint(new Point(0,0),contentGrid);
                        if(pos.X< -3 || pos.X+action.ActualWidth>contentGrid.ActualWidth+3)
                            throw new InvalidOperationException(
                                "Source viewer footer clipped at "+shotName+": "+
                                action.Content);
                    }
                    var shotWidth=(int)Math.Round(contentGrid.ActualWidth);
                    var shotHeight=(int)Math.Round(contentGrid.ActualHeight);
                    var img=new RenderTargetBitmap(shotWidth,shotHeight,96,96,
                        PixelFormats.Pbgra32);
                    img.Render(contentGrid);
                    var encoder=new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    using var file=File.Create(Path.Combine(folder,shotName+".png"));
                    encoder.Save(file);
                    File.AppendAllText(diagnostics,
                        "Real source window "+shotName+" "+shotWidth+"x"+shotHeight+
                        Environment.NewLine);
                }
            }
            finally { sourceWindow.Close(); }

            // A synthetic row is used ONLY as an offscreen layout assertion:
            // it is never published as a screenshot or product/project data.
            Reflow(900,650);
            Navigate("projects");
            var testRow=ProjectRow(new Brendigo.Web2APK.Core.ProjectDefinition {
                Name="Dugačak naziv projekta koji mora ostati potpuno dostupan korisniku",
                PackageName="com.brendigo.longproject_testing_responsive_layout",
                Url="https://example.com/long-path"
            },true);
            PageHost.Children.Add(testRow);
            Reflow(900,650);
            CheckCard(testRow,"populated project row at 900");
            if(testRow.Child is not Grid layout ||
                layout.Children.OfType<WrapPanel>().FirstOrDefault() is not { } controls ||
                Grid.GetRow(controls)!=1 || Grid.GetColumn(controls)!=0)
                throw new InvalidOperationException(
                    "Project actions must stack under the title at 900 px.");
            var end=controls.TranslatePoint(new Point(controls.ActualWidth,0),PageHost);
            if(end.X>PageHost.ActualWidth+2)
                throw new InvalidOperationException("Project action buttons exceed viewport.");
            PageHost.Children.Remove(testRow);
            // Exercise actual navigation buttons, not only directly invoked page methods.
            // No activation dialogs or external tools are opened during this smoke test.
            Button? FindAction(DependencyObject root,string label)
            {
                if(root is Button b && b.Content is string title &&
                    title.Contains(label,StringComparison.Ordinal))return b;
                for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)
                {
                    var match=FindAction(VisualTreeHelper.GetChild(root,i),label);
                    if(match!=null)return match;
                }
                return null;
            }
            void ClickAndAssert(string from,string label,string destination)
            {
                Navigate(from);
                Reflow(900,650);
                var button=FindAction(PageHost,label);
                if(button is null || !button.IsEnabled)
                    throw new InvalidOperationException("Navigation action missing: "+from+"/"+label);
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Reflow(900,650);
                if(_page!=destination)
                    throw new InvalidOperationException(
                        "Navigation action failed: "+from+"/"+label+" → "+_page);
                File.AppendAllText(diagnostics,
                    "Real button click passed: "+from+" → "+destination+Environment.NewLine);
            }
            ClickAndAssert("welcome","Novi projekt","new");
            ClickAndAssert("dashboard","Moji projekti","projects");
            ClickAndAssert("projects","Novi projekt","new");
            // Real UI button interactions: switch device preview modes without
            // launching a networked browser in headless CI.
            void ClickPreviewDevice(string expected,string name)
            {
                Navigate("preview");
                Reflow(900,650);
                Button? FindDevice(DependencyObject root)
                {
                    if(root is Button b &&
                       System.Windows.Automation.AutomationProperties.GetName(b)==
                           "Odaberi prikaz uređaja: "+name)return b;
                    for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)
                    {
                        var found=FindDevice(VisualTreeHelper.GetChild(root,i));
                        if(found!=null)return found;
                    }
                    return null;
                }
                var control=FindDevice(PageHost) ??
                    throw new InvalidOperationException("Preview device button missing: "+name);
                control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Reflow(900,650);
                if(_previewDevice!=expected || _page!="preview" ||
                    _previewWebToggle?.IsChecked!=false)
                    throw new InvalidOperationException(
                        "Preview device selection did not refresh UI: "+name);
                File.AppendAllText(diagnostics,
                    "Real preview device switch passed: "+name+Environment.NewLine);
            }
            ClickPreviewDevice("tablet","Android tablet");
            ClickPreviewDevice("offline","Pregled izvan mreže");
            ClickPreviewDevice("phone","Android telefon");
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
            var pixelReport=Path.Combine(Environment.CurrentDirectory,
                "visual-audit","native-full-pixel-audit.json");
            Directory.CreateDirectory(Path.GetDirectoryName(pixelReport)!);
            File.WriteAllText(pixelReport,
                System.Text.Json.JsonSerializer.Serialize(new {
                    generatedUtc=DateTimeOffset.UtcNow,
                    baselineKind="previous real WPF captures",
                    comparison="every pixel at native raster resolution",
                    expectedDesignReferenceMatchNotVerified=true,
                    screens=pixelAudit
                },new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
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
