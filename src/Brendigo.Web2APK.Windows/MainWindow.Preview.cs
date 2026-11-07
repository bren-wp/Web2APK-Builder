using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow
{
    private async void StartLivePreview()
    {
        if (_previewDevice == "offline")
        {
            _previewWebViewEnabled = false;
            if (_previewWebToggle != null) _previewWebToggle.IsChecked = false;
            ShowInfo("Izvanmrežni prikaz je simulacija izgleda bez mreže. Za pregled lokalnog HTML-a odaberite Telefon ili Tablet.");
            return;
        }
        if (_previewSurface is null) return;
        try
        {
            _draft.Validate();
            _previewWebViewEnabled=true;
            _previewBrowser?.Dispose();
            var browser = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch, DefaultBackgroundColor = System.Drawing.Color.White };
            _previewBrowser = browser;
            _previewSurface.Children.Clear();
            _previewSurface.Children.Add(browser);
            var folder = Path.Combine(_dataDir, "webview2-preview");
            Directory.CreateDirectory(folder);
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: folder);
            if (_previewBrowser != browser || _page != "preview") { browser.Dispose();return; }
            await browser.EnsureCoreWebView2Async(environment);
            if (_previewBrowser != browser || _page != "preview") { browser.Dispose();return; }
            browser.CoreWebView2.Settings.IsScriptEnabled = _draft.JavaScript;
            browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            browser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            browser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            browser.CoreWebView2.PermissionRequested += (_, e) => {
                // Preview is not a permissions grant. Real Android permissions remain separate.
                e.State = CoreWebView2PermissionState.Deny;
            };
            browser.CoreWebView2.NewWindowRequested += (_, e) => e.Handled = true;
            browser.CoreWebView2.DownloadStarting += (_, e) => e.Cancel = true;
            string url;
            if (_draft.SourceType == "local")
            {
                var path = Path.GetFullPath(_draft.LocalPath);
                browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "brendigo-preview.invalid", path, CoreWebView2HostResourceAccessKind.DenyCors);
                url = "https://brendigo-preview.invalid/index.html";
            }
            else
            {
                url = _draft.Url;
            }
            var origin = new Uri(url);
            browser.CoreWebView2.NavigationStarting += (_, e) =>
            {
                if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var to) ||
                    !string.Equals(to.Scheme,origin.Scheme,StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(to.Host,origin.Host,StringComparison.OrdinalIgnoreCase) ||
                    to.Port!=origin.Port)
                    e.Cancel=true;
            };
            browser.CoreWebView2.Navigate(url);
        }
        catch (Exception e)
        {
            _previewWebViewEnabled=false;
            if(_previewWebToggle != null && _previewWebToggle.IsChecked==true)
                _previewWebToggle.IsChecked=false;
            _previewBrowser?.Dispose();
            _previewBrowser = null;
            if (_previewSurface != null)
            {
                _previewSurface.Children.Clear();
                _previewSurface.Children.Add(Txt("Pregled nije dostupan. Provjerite URL i instalaciju Microsoft Edge WebView2 Runtimea.",
                    13,false,BrushOf("#F2C1CA"),new Thickness(14)));
            }
            ShowError("Pregled aplikacije nije dostupan: " + e.Message);
        }
    }

    private void Preview()
    {
        var headline=new StackPanel { Margin=new Thickness(0,0,0,18) };
        headline.Children.Add(Txt("Pregled aplikacije",29,true,White,new Thickness(0,0,0,5)));
        headline.Children.Add(Txt("Provjerite izgled i navigaciju prije generiranja Android paketa.",
            13,false,Muted));
        var buttons=Row();
        buttons.Margin=new Thickness(0,1,0,10);
        buttons.Children.Add(Btn("▷  Pokreni test", () => {
            if(_previewWebToggle?.IsChecked==false)
                _previewWebToggle.IsChecked=true; // Handles starting the live WebView.
            else StartLivePreview();
        }, true));
        buttons.Children.Add(Btn("↗  Otvori u pregledniku",()=>{
            if(_draft.SourceType!="url") { ShowInfo("Lokalni HTML provjerite pomoću gumba Pokreni test.");return; }
            if(!Uri.TryCreate(_draft.Url,UriKind.Absolute,out var uri) ||
                (uri.Scheme!=Uri.UriSchemeHttps && !(_draft.AllowHttp && uri.Scheme==Uri.UriSchemeHttp)))
            {ShowError("Neispravan web URL.");return;}
            Process.Start(new ProcessStartInfo{FileName=uri.AbsoluteUri,UseShellExecute=true});
        }));
        if(PageScroll.ActualWidth>1010)
        {
            var top=new DockPanel{LastChildFill=true,Margin=new Thickness(0,0,0,16)};
            DockPanel.SetDock(buttons,Dock.Right);
            top.Children.Add(buttons);
            top.Children.Add(headline);
            PageHost.Children.Add(top);
        }
        else
        {
            PageHost.Children.Add(headline);
            PageHost.Children.Add(buttons);
        }
        var columns = Row();
        columns.Margin = new Thickness(0);
        var chooser=Stack();
        var center=Stack();
        var properties=Stack();
        _previewLeftCard=Card(chooser,new Thickness(21));
        _previewCenterCard=Card(center,new Thickness(22));
        _previewRightCard=Card(properties,new Thickness(21));
        _previewLeftCard.MinHeight=565;
        _previewCenterCard.MinHeight=565;
        _previewRightCard.MinHeight=565;
        columns.Children.Add(_previewLeftCard);
        columns.Children.Add(_previewCenterCard);
        columns.Children.Add(_previewRightCard);
        PageHost.Children.Add(columns);

        chooser.Children.Add(Txt("Uređaj",17,true,White,new Thickness(0,0,0,14)));
        chooser.Children.Add(DevicePreviewButton("phone","▯","Android telefon","1080 × 2400 · portret"));
        chooser.Children.Add(DevicePreviewButton("tablet","▣","Android tablet","2560 × 1600 · krajolik"));
        chooser.Children.Add(DevicePreviewButton("offline","◉","Pregled izvan mreže","Simulirano offline stanje"));
        chooser.Children.Add(new Border { Margin=new Thickness(0,26,0,0),Padding=new Thickness(13),
            Background=BrushOf("#302939"),BorderBrush=BrushOf("#7E624C"),BorderThickness=new Thickness(1),
            CornerRadius=new CornerRadius(11),
            Child=Txt("ⓘ  Prikaz je informativan. Završnu aplikaciju provjerite na Android uređaju.",
                12,false,BrushOf("#EBD0A8")) });

        var area=new Grid { Height=511, VerticalAlignment=VerticalAlignment.Center };
        center.Children.Add(area);
        var tablet = _previewDevice == "tablet";
        var offline = _previewDevice == "offline";
        var width = tablet?385:252;
        var height = tablet?340:477;
        var frame = new Border { Width=width+16, Height=height+16, CornerRadius=new CornerRadius(tablet?30:34),
            BorderThickness=new Thickness(8), BorderBrush=BrushOf("#44527B"),
            Background=BrushOf("#222B5A"), HorizontalAlignment=HorizontalAlignment.Center,
            VerticalAlignment=VerticalAlignment.Center, ClipToBounds=true };
        var phone=new Grid { Background = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#2B468D"),
            (Color)ColorConverter.ConvertFromString("#59309D"),95) };
        phone.RowDefinitions.Add(new RowDefinition {Height=new GridLength(21)});
        phone.RowDefinitions.Add(new RowDefinition {Height=new GridLength(1,GridUnitType.Star)});
        phone.RowDefinitions.Add(new RowDefinition {Height=new GridLength(14)});
        var notch=new Border { Width=56,Height=10,CornerRadius=new CornerRadius(0,0,7,7),
            Background=BrushOf("#18264A"),HorizontalAlignment=HorizontalAlignment.Center,
            VerticalAlignment=VerticalAlignment.Top };
        phone.Children.Add(notch);
        _previewSurface = new Grid { Margin=new Thickness(1),ClipToBounds=true };
        Grid.SetRow(_previewSurface,1);
        var splash=Stack();
        splash.HorizontalAlignment=HorizontalAlignment.Center;
        splash.VerticalAlignment=VerticalAlignment.Center;
        if (offline)
        {
            splash.Children.Add(Txt("◉",48,true,BrushOf("#B0A8FF"),new Thickness(0,0,0,12)));
            splash.Children.Add(Txt("Nema internetske veze",15,true,White));
            splash.Children.Add(Txt("Offline simulacija",12,false,BrushOf("#CFCEF3"),
                new Thickness(0,10,0,0)));
        }
        else
        {
            if (_previewSplash && _draft.Splash)
            {
                var logo = new Border { Width=70,Height=70,CornerRadius=new CornerRadius(15),
                    Background=Accent,HorizontalAlignment=HorizontalAlignment.Center,
                    Child=new Image { Source=PreviewIcon(),Width=59,Height=59,Stretch=Stretch.Uniform } };
                splash.Children.Add(logo);
            }
            splash.Children.Add(Txt(_draft.Name,17,true,White,new Thickness(0,17,0,22)));
            splash.Children.Add(new Border { Width=155,Height=42,Background=BrushOf("#D9D6F4"),
                CornerRadius=new CornerRadius(12),Margin=new Thickness(0,0,0,21) });
            foreach (var itemWidth in new[] {150,150,105})
                splash.Children.Add(new Border { Width=itemWidth,Height=7,Background=BrushOf("#7767C1"),
                    CornerRadius=new CornerRadius(4),Margin=new Thickness(0,0,0,9) });
        }
        _previewSurface.Children.Add(splash);
        phone.Children.Add(_previewSurface);
        frame.Child=phone;
        area.Children.Add(frame);

        properties.Children.Add(Txt("Pregled svojstava",17,true,White,new Thickness(0,0,0,14)));
        _previewWebToggle=PreviewToggle("WebView sadržaj",
            "Pregled URL-a / lokalnog HTML-a",_previewWebViewEnabled,
            enabled => {
                if(enabled) StartLivePreview();
                else
                {
                    _previewWebViewEnabled=false;
                    Navigate("preview"); // Restore native splash, dispose live browser.
                }
            });
        System.Windows.Automation.AutomationProperties.SetName(_previewWebToggle,
            "Uključi ili isključi živi WebView pregled");
        properties.Children.Add(_previewWebToggle);
        properties.Children.Add(Divider());
        properties.Children.Add(PreviewToggle("Splash zaslon","Ikonica i početna boja",_previewSplash&&_draft.Splash,
            enabled => {
                _previewWebViewEnabled=false;
                _previewSplash=enabled;
                _draft.Splash=enabled;
                Navigate("preview");
            }));
        properties.Children.Add(Divider());
        properties.Children.Add(PreviewToggle("Vanjske poveznice","Otvori u pregledniku na Androidu",_draft.OpenExternal,
            enabled => _draft.OpenExternal=enabled));
        properties.Children.Add(Divider());
        properties.Children.Add(Txt("Kamera, mikrofon i lokacija zahtijevaju stvarni Android uređaj i korisnikovo dopuštenje.",
            11,false,Muted,new Thickness(0,8,0,18)));
        properties.Children.Add(Btn("⚙  Uredi projekt",()=>Navigate("new")));
        properties.Children.Add(Btn("✓  Spremi svojstva",()=>{ if(SaveDraft())ShowInfo("Postavke pregleda su spremljene."); },true));
        UpdateResponsive();
    }

    private ImageSource PreviewIcon()
    {
        try
        {
            if(!string.IsNullOrWhiteSpace(_draft.IconPath) && File.Exists(_draft.IconPath))
            {
                var icon=new BitmapImage();
                icon.BeginInit();
                icon.UriSource=new Uri(Path.GetFullPath(_draft.IconPath));
                icon.CacheOption=BitmapCacheOption.OnLoad;
                icon.DecodePixelWidth=72;
                icon.EndInit();
                icon.Freeze();
                return icon;
            }
        }
        catch(IOException) { }
        catch(NotSupportedException) { }
        return new BitmapImage(new Uri("pack://application:,,,/Assets/Brendigo.ico"));
    }

    private Border DevicePreviewButton(string id,string symbol,string name,string subtitle)
    {
        var entry=new StackPanel { Orientation=Orientation.Horizontal };
        var glyph=new Border { Width=45,Height=45,CornerRadius=new CornerRadius(11),
            Background=BrushOf("#332F75"),Margin=new Thickness(0,0,12,0),
            Child=Txt(symbol,24,true,BrushOf("#B6A7FF"),new Thickness(13,7,0,0)) };
        entry.Children.Add(glyph);
        var label=Stack();
        label.VerticalAlignment=VerticalAlignment.Center;
        label.Children.Add(Txt(name,12,true));
        label.Children.Add(Txt(subtitle,11,false,Muted,new Thickness(0,5,0,0)));
        entry.Children.Add(label);
        var selected=id==_previewDevice;
        var button=Btn("",()=>{
            _previewWebViewEnabled=false;
            _previewDevice=id;
            Navigate("preview");
        });
        System.Windows.Automation.AutomationProperties.SetName(button,
            "Odaberi prikaz uređaja: "+name);
        button.Content=entry;
        button.HorizontalContentAlignment=HorizontalAlignment.Left;
        button.Padding=new Thickness(9);
        button.Background=selected?BrushOf("#292760"):BrushOf("#19243E");
        button.BorderBrush=selected?BrushOf("#655BC3"):Stroke;
        button.MinHeight=76;
        button.Margin=new Thickness(0,0,0,10);
        return new Border { Child=button };
    }

    private CheckBox PreviewToggle(string title,string desc,bool active,Action<bool> changed)
    {
        var row=Stack();
        row.Children.Add(Txt(title,12,true));
        row.Children.Add(Txt(desc,11,false,Muted,new Thickness(0,5,0,0)));
        var check=new CheckBox { Content=row,IsChecked=active,Margin=new Thickness(0,2,0,6) };
        check.Checked+=(_,_)=>changed(true);
        check.Unchecked+=(_,_)=>changed(false);
        return check;
    }
}
