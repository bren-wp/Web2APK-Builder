using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow : Window
{
    private readonly string _dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Brendigo", "Web2APK Builder");
    private readonly ProjectStore _store;
    private string _page = "dashboard";
    private int _wizardStep;
    private ProjectDefinition _draft = new();
    private readonly List<FrameworkElement> _fieldCells = [];
    private readonly List<Border> _stats = [];
    private readonly Dictionary<string, Button> _navButtons = [];
    private TextBlock? _summaryTitle;
    private TextBlock? _summaryPackageText;
    private Button? _wizardInlineNext, _wizardSummaryNext;
    private Border? _wizardMainCard;
    private Border? _wizardSummaryCard;
    private CancellationTokenSource? _buildCancellation;
    private TextBox? _buildLog;
    private TextBox? _keyStoreBox;
    private TextBox? _keyAliasBox;
    private PasswordBox? _storePassword;
    private PasswordBox? _keyPassword;
    private bool _busy;
    private Border? _previewLeftCard, _previewCenterCard, _previewRightCard;
    private Microsoft.Web.WebView2.Wpf.WebView2? _previewBrowser;
    private Grid? _previewSurface;
    private string _previewDevice = "phone";
    private bool _previewSplash = true;
    private FrameworkElement? _heroPhone;

    private static Brush BrushOf(string color) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    private static readonly Brush Stroke = BrushOf("#2B3B60");
    private static readonly Brush Muted = BrushOf("#9DB0D4");
    private static readonly Brush White = BrushOf("#F1F4FF");
    private static readonly Brush Accent = BrushOf("#7554F6");
    private static readonly string[] StepNames = ["Osnovno", "Izgled", "WebView", "Funkcije", "Dozvole", "Izrada"];

    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(_dataDir);
        _store = new ProjectStore(_dataDir);
        MakeNavigation();
        Navigate(_store.List().Count == 0 ? "welcome" : "dashboard");
#if UI_CAPTURE
        if (Environment.GetCommandLineArgs().Contains("--capture-native"))
            Loaded += CaptureNativeScreensOnLoaded;
#endif
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateResponsive();
    private void UpdateResponsive()
    {
        if (NavigationHost is null) return;
        var logicalWidth=double.IsNaN(Root.Width) || Root.Width<=0?ActualWidth:Root.Width;
        var compact = logicalWidth < 1020;
        SidebarColumn.Width = new GridLength(compact ? 82 : 250);
        BrandWords.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        SidebarFooter.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        if (_heroPhone is not null) _heroPhone.Visibility = logicalWidth < 1200 ? Visibility.Collapsed : Visibility.Visible;
        foreach (var entry in _navButtons)
        {
            var item = entry.Value.Content as StackPanel;
            if (item?.Children.Count > 1 && item.Children[1] is TextBlock label)
                label.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }
        var viewport = Math.Max(550, PageScroll?.ActualWidth ?? 900);
        var statsPerRow = viewport > 980 ? 4 : viewport > 630 ? 2 : 1;
        var statWidth = Math.Max(190, (viewport - 80 - (statsPerRow - 1) * 14) / statsPerRow);
        foreach (var card in _stats) card.Width = statWidth;
        if (_wizardMainCard is not null && _wizardSummaryCard is not null)
        {
            var available=Math.Max(400,viewport-64);
            var sideBySide=available >= 1040;
            var summaryWidth=Math.Min(370,available*.34);
            _wizardMainCard.Width=sideBySide?available-summaryWidth-28:available-14;
            _wizardSummaryCard.Width=sideBySide?summaryWidth:available-14;
            if(_wizardInlineNext!=null)
                _wizardInlineNext.Visibility=sideBySide?Visibility.Collapsed:Visibility.Visible;
            if(_wizardSummaryNext!=null)
                _wizardSummaryNext.Visibility=sideBySide?Visibility.Visible:Visibility.Collapsed;
            var contentWidth=Math.Max(260,_wizardMainCard.Width-48);
            var paired=contentWidth>=565;
            var normal=paired?(contentWidth-30)/2:contentWidth-14;
            foreach(var field in _fieldCells)
                field.Width=field.Tag is string role && role=="wide"?contentWidth-14:normal;
        }
        if (_previewLeftCard is not null && _previewCenterCard is not null && _previewRightCard is not null)
        {
            var available=Math.Max(400,viewport-64);
            var wide=available>=1050;
            var medium=available>=740;
            if(wide)
            {
                _previewLeftCard.Width=280;
                _previewRightCard.Width=315;
                _previewCenterCard.Width=available-280-315-48;
            }
            else if(medium)
            {
                _previewLeftCard.Width=265;
                _previewCenterCard.Width=available-265-32;
                _previewRightCard.Width=available-14;
            }
            else
            {
                _previewLeftCard.Width=available-14;
                _previewCenterCard.Width=available-14;
                _previewRightCard.Width=available-14;
            }
        }
    }

    private void MakeNavigation()
    {
        NavigationHost.Children.Clear();
        NavigationHost.Children.Add(Caption("RADNI PROSTOR"));
        foreach (var (key, label, icon) in new[] {
            ("dashboard","Početna","\uE80F"),("new","Novi projekt","\uE710"),
            ("projects","Moji projekti","\uE8B7"),("preview","Pregled aplikacije","\uE8A7"),
            ("devices","Uređaji","\uE8EA"),("settings","Postavke","\uE713") })
            AddNav(key, label, icon);
        var help = Caption("POMOĆ");
        help.Margin = new Thickness(14,24,0,8);
        NavigationHost.Children.Add(help);
        AddNav("docs", "Dokumentacija", "\uE8A5");
        AddNav("about", "O programu", "\uE946");
    }

    private void AddNav(string key, string label, string glyph)
    {
        var inner = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        inner.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 17,
            Width = 26, Foreground = BrushOf("#B5C6E8"), VerticalAlignment = VerticalAlignment.Center });
        inner.Children.Add(new TextBlock { Text = label, FontSize = 13, Foreground = White, VerticalAlignment = VerticalAlignment.Center });
        var b = new Button { Content = inner, Style = (Style)FindResource("NavigationButton"), Tag = key,
            Margin = new Thickness(0,2,0,4), HorizontalAlignment = HorizontalAlignment.Stretch };
        b.ToolTip = label;
        b.Click += (_, _) => { if (key == "new") StartNew(); else Navigate(key); };
        _navButtons[key] = b;
        NavigationHost.Children.Add(b);
    }

    private static TextBlock Caption(string text) => new() { Text = text, Foreground = BrushOf("#7189B6"),
        FontSize = 10, FontWeight = FontWeights.SemiBold, Margin = new Thickness(14,8,0,9) };

    private static TextBlock Txt(string content, int size = 13, bool bold = false, Brush? color = null, Thickness? margin = null)
        => new() { Text = content, FontSize = size, Foreground = color ?? White,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Margin = margin ?? new Thickness(0),
            TextWrapping = TextWrapping.Wrap };

    private static Border Card(UIElement child, Thickness? padding = null) => new() {
        Background = BrushOf("#15203A"), BorderBrush = Stroke, BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(16), Padding = padding ?? new Thickness(20),
        Child = child, Margin = new Thickness(0,0,14,14) };

    private static StackPanel Stack() => new() { Orientation = Orientation.Vertical };
    private static Button Btn(string title, Action click, bool primary = false, bool danger = false)
    {
        var b = new Button { Content = title, Style = (Style)Application.Current.FindResource("StudioButton"),
            Background=primary?new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#547FFA"),
                (Color)ColorConverter.ConvertFromString("#8350F2"),0)
                :danger?BrushOf("#573047"):BrushOf("#1C2A4C"),
            Margin = new Thickness(0,0,10,8), MinHeight = 42 };
        b.Click += (_, _) => click();
        return b;
    }
    private static WrapPanel Row() => new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0,9,0,8) };
    private static Border Divider() => new() { Height = 1, Background = Stroke, Margin = new Thickness(0,13,0,15) };

    private void Navigate(string page)
    {
        if (_page == "preview") { _previewBrowser?.Dispose(); _previewBrowser = null; }
        _page = page;
        _previewLeftCard = null; _previewCenterCard = null; _previewRightCard = null; _previewSurface = null;
        _heroPhone = null;
        _summaryTitle=null;_summaryPackageText=null;
        _wizardMainCard=null;_wizardSummaryCard=null;
        _wizardInlineNext=null;_wizardSummaryNext=null;
        _stats.Clear(); _fieldCells.Clear();
        PageHost.Children.Clear();
        Breadcrumb.Text = "Radni prostor   ›   " + (page switch {
            "welcome" => "Dobrodošli", "dashboard" => "Početna", "new" => "Novi projekt", "projects" => "Moji projekti",
            "preview" => "Pregled aplikacije",
            "tools" => "Razvojni alati", "devices" => "Uređaji", "settings" => "Postavke",
            "docs" => "Dokumentacija", "about" => "O programu", _ => "Web2APK" });
        foreach (var item in _navButtons)
        {
            var selected = item.Key == page || (page == "welcome" && item.Key == "dashboard");
            item.Value.Background = selected ? BrushOf("#302A74") : Brushes.Transparent;
            item.Value.BorderThickness = selected ? new Thickness(1) : new Thickness(0);
            item.Value.BorderBrush = selected ? BrushOf("#5547C8") : Brushes.Transparent;
            if (item.Value.Content is StackPanel icons && icons.Children.Count > 1 &&
                icons.Children[1] is TextBlock title)
                title.FontWeight = selected ? FontWeights.Bold : FontWeights.Medium;
        }
        try
        {
            switch (page)
            {
                case "welcome": Welcome(); break;
                case "dashboard": Dashboard(); break;
                case "preview": Preview(); break;
                case "new": Wizard(); break;
                case "projects": Projects(); break;
                case "tools": Tools(); break;
                case "devices": Devices(); break;
                case "settings": Settings(); break;
                case "docs": Docs(); break;
                default: About(); break;
            }
            PageScroll.ScrollToTop();
            UpdateResponsive();
        }
        catch (Exception e) { ShowError(e.Message); }
    }

    private void Heading(string heading, string sub)
    {
        PageHost.Children.Add(Txt(heading, 29, true, White, new Thickness(0,0,0,4)));
        PageHost.Children.Add(Txt(sub, 13, false, Muted, new Thickness(0,0,0,22)));
    }

    private void Dashboard()
    {
        var projects=_store.List();
        var hero=new Grid { MinHeight=198 };
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(206) });
        var info=Stack();
        info.Children.Add(new Border {
            Background=BrushOf("#302D70"),BorderBrush=BrushOf("#6756B7"),
            BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(14),
            HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(11,5,11,5),
            Child=Txt("✧  STUDIO · NOVI PROJEKT",10,true,BrushOf("#CBBAFF"))
        });
        info.Children.Add(Txt("Od web-stranice do Android aplikacije.",26,true,White,new Thickness(0,13,0,10)));
        info.Children.Add(Txt("Izradite Android projekt s vlastitim izgledom, dozvolama i potpisivanjem.",
            13,false,BrushOf("#D1D9EF"),new Thickness(0,0,0,15)));
        var actions=Row();
        actions.Children.Add(Btn("＋  Novi projekt",StartNew,true));
        actions.Children.Add(Btn("▣  Moji projekti",()=>Navigate("projects")));
        info.Children.Add(actions);
        hero.Children.Add(info);
        var phone=new Border {
            Width=121,Height=191,CornerRadius=new CornerRadius(23),
            BorderThickness=new Thickness(5),BorderBrush=BrushOf("#6778C5"),
            Background=BrushOf("#302777"),HorizontalAlignment=HorizontalAlignment.Center,
            VerticalAlignment=VerticalAlignment.Center,
            RenderTransform=new RotateTransform(-14),RenderTransformOrigin=new Point(.5,.5)
        };
        var icon=new System.Windows.Controls.Image {
            Source=new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/Assets/Brendigo.ico")),
            Width=63,Height=63,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center
        };
        phone.Child=icon;
        Grid.SetColumn(phone,1);hero.Children.Add(phone);
        _heroPhone=phone;
        PageHost.Children.Add(new Border {
            Child=hero,Background=new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#233D76"),
                (Color)ColorConverter.ConvertFromString("#41318C"),10),
            BorderBrush=BrushOf("#56529A"),BorderThickness=new Thickness(1),
            CornerRadius=new CornerRadius(20),Padding=new Thickness(28,19,20,15),
            Margin=new Thickness(0,0,0,20),ClipToBounds=true
        });

        var summary=Row();
        summary.Margin=new Thickness(0,0,0,15);
        summary.Children.Add(Stat("▣",projects.Count.ToString(),"Ukupno projekata","#223F72"));
        summary.Children.Add(Stat("◇",projects.Count(p=>p.SourceType=="local").ToString(),"Lokalni HTML","#3D317A"));
        summary.Children.Add(Stat("⬡",projects.Count(p=>!p.AllowHttp).ToString(),"HTTPS projekti","#155B55"));
        summary.Children.Add(Stat("◷",projects.Count(p=>p.UpdatedAt>DateTime.UtcNow.AddDays(-7)).ToString(),
            "Aktivni ovaj tjedan","#645032"));
        PageHost.Children.Add(summary);

        var section=new DockPanel { Margin=new Thickness(0,2,0,10) };
        var all=Btn("Svi projekti  →",()=>Navigate("projects"));
        DockPanel.SetDock(all,Dock.Right);
        section.Children.Add(all);
        section.Children.Add(Txt("Posljednji projekti",18,true,White,new Thickness(0,7,0,0)));
        PageHost.Children.Add(section);
        var rows=Stack();
        if (PageScroll.ActualWidth > 830) rows.Children.Add(DashboardTableRow(null));
        foreach(var project in projects.Take(6))
        {
            if(PageScroll.ActualWidth>830)rows.Children.Add(DashboardTableRow(project));
            else rows.Children.Add(ProjectRow(project,false));
        }
        if(projects.Count==0)
        {
            rows.Children.Add(Txt("Još nema projekata. Kreirajte prvi projekt.",
                13,false,Muted,new Thickness(14,17,0,12)));
            rows.Children.Add(Btn("＋  Novi projekt",StartNew,true));
        }
        PageHost.Children.Add(Card(rows,new Thickness(10,8,10,9)));
    }

    private Border DashboardTableRow(ProjectDefinition? project)
    {
        var row=new Grid { MinHeight=project==null?40:53 };
        foreach(double weight in new[]{2.3,1.5,1.0,1.2,1.0,0.9})
            row.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(weight,GridUnitType.Star)});
        var values=project==null
            ?new[]{"PROJEKT","IZVOR","VRSTA","DATUM","STATUS",""}
            :new[]{project.Name,project.SourceType=="url"?new Uri(project.Url).Host:"Lokalni HTML",
                "WebView",project.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy"),"Spremljeno","Uredi"};
        for(int i=0;i<values.Length;i++)
        {
            FrameworkElement cell;
            if(project!=null&&i==5)cell=Btn("Uredi",()=>Edit(project));
            else cell=Txt(values[i],project==null?10:12,project==null||i==0,
                project==null?BrushOf("#879CC7"):i==4?BrushOf("#81E6C7"):White);
            cell.VerticalAlignment=VerticalAlignment.Center;
            cell.Margin=new Thickness(9,6,5,6);
            Grid.SetColumn(cell,i);row.Children.Add(cell);
        }
        return new Border { Child=row,BorderBrush=Stroke,BorderThickness=new Thickness(0,0,0,1) };
    }

    private Border Stat(string symbol, string number, string caption, string bg)
    {
        var body = Stack();
        body.Children.Add(Txt(symbol, 26, true, BrushOf("#91BBFF"), new Thickness(0,0,0,12)));
        body.Children.Add(Txt(number, 30, true));
        body.Children.Add(Txt(caption, 12, false, Muted));
        var card = Card(body, new Thickness(19));
        card.Background = BrushOf("#16213D");
        card.MinHeight = 135; card.Width = 238;
        _stats.Add(card);
        return card;
    }

    private Border ProjectRow(ProjectDefinition project, bool full)
    {
        var row = new DockPanel { LastChildFill = true };
        var buttons = Row();
        buttons.Margin = new Thickness(8,0,0,0);
        buttons.Children.Add(Btn("Uredi", () => Edit(project), true));
        if (full) {
            buttons.Children.Add(Btn("Izvezi", () => _ = ExportAsync(project)));
            buttons.Children.Add(Btn("Dijeli JSON", () => ExportMobileProject(project)));

            buttons.Children.Add(Btn("Obriši", () => Delete(project), false, true));
        }
        DockPanel.SetDock(buttons, Dock.Right);
        row.Children.Add(buttons);
        var info = Stack();
        info.Children.Add(Txt("◈  " + project.Name, 14, true));
        var source = project.SourceType == "url" ? project.Url : "Lokalni HTML projekt";
        info.Children.Add(Txt(source + "    ·    " + project.PackageName, 11, false, Muted, new Thickness(0,3,0,0)));
        row.Children.Add(info);
        return new Border { Child = row, Margin = new Thickness(0,0,0,8), Padding = new Thickness(13),
            Background = BrushOf("#182543"), BorderThickness = new Thickness(1), BorderBrush = Stroke,
            CornerRadius = new CornerRadius(11), MinHeight = 67 };
    }

    private void Projects()
    {
        Heading("Moji projekti", "Svi spremljeni Android projekti, lokalno i privatno.");
        var row = Row();
        row.Children.Add(Btn("＋  Novi projekt", StartNew, true));
        row.Children.Add(Btn("Osvježi", () => Navigate("projects")));
        row.Children.Add(Btn("Uvezi Android JSON", ImportMobileProject));

        PageHost.Children.Add(row);
        var stack = Stack();
        var all = _store.List();
        if (all.Count == 0) stack.Children.Add(Txt("Nema spremljenih projekata.", 15, false, Muted));
        foreach (var project in all) stack.Children.Add(ProjectRow(project, true));
        PageHost.Children.Add(Card(stack));
    }

    private void Edit(ProjectDefinition p)
    {
        _draft = p.Copy(); _wizardStep = 0; Navigate("new");
    }
    private void Delete(ProjectDefinition p)
    {
        if (MessageBox.Show("Obrisati projekt „" + p.Name + "”? Izvezeni Android paketi neće biti uklonjeni.",
            "Brisanje projekta", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try { _store.Delete(p.Id); if (_draft.Id == p.Id) _draft = new ProjectDefinition(); Navigate("projects"); }
        catch (Exception ex) { ShowError(ex.Message); }
    }
    private void ImportMobileProject()
    {
        var dialog=new OpenFileDialog { Title="Uvoz projekta s Android klijenta",
            Filter="Brendigo projekt (*.json)|*.json", CheckFileExists=true };
        if (dialog.ShowDialog(this)!=true) return;
        try
        {
            if (new FileInfo(dialog.FileName).Length>256*1024)
                throw new InvalidDataException("JSON projekt je prevelik.");
            var jsonText=File.ReadAllText(dialog.FileName);
            using var raw=JsonDocument.Parse(jsonText);
            bool requestedPush=raw.RootElement.TryGetProperty("FirebasePushRequested",out var pushRequest) &&
                pushRequest.ValueKind==JsonValueKind.True;
            var input=JsonSerializer.Deserialize<ProjectDefinition>(jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive=true })
                ?? throw new InvalidDataException("JSON projekt je prazan.");
            if (input.SourceType!="url")
                throw new InvalidDataException("Mobilni uvoz podržava samo URL projekte.");
            input.Id=Guid.NewGuid();
            input.FirebaseConfigPath="";
            input.FirebasePushEnabled=false;
            input.IconPath="";
            var saved=_store.Save(input);
            ShowInfo("Projekt uspješno uvezen: "+saved.Name+"\nPregledajte AdMob podatke i dozvole prije izrade." +
                (requestedPush ? "\n\nAndroid klijent zatražio je Firebase Push. U koraku Funkcije uključite Firebase i odaberite google-services.json." : ""));
            Navigate("projects");
        }
        catch(Exception e) { ShowError("Uvoz projekta nije uspio: "+e.Message); }
    }
    private void ExportMobileProject(ProjectDefinition project)
    {
        var dialog=new SaveFileDialog { Title="Izvoz projekta za Android klijenta",
            Filter="Brendigo projekt (*.json)|*.json", DefaultExt=".json",
            FileName="Brendigo-"+project.PackageName+".json" };
        if(dialog.ShowDialog(this)!=true)return;
        try
        {
            var mobile=project.Copy();
            mobile.FirebaseConfigPath="";
            mobile.FirebasePushEnabled=false;
            mobile.IconPath="";
            mobile.LocalPath="";
            if(mobile.SourceType=="local")
                throw new InvalidDataException("Lokalni HTML projekt nije prenosiv na telefon putem JSON konfiguracije.");
            File.WriteAllText(dialog.FileName,JsonSerializer.Serialize(mobile,
                new JsonSerializerOptions { WriteIndented=true }));
            ShowInfo("Projektni JSON izvezen. Uvezite datoteku u Android klijentu.");
        }
        catch(Exception e){ ShowError(e.Message); }
    }

    private void StartNew()
    {
        if (_busy) return;
        _draft = new ProjectDefinition(); _wizardStep = 0; Navigate("new");
    }

    private static void ShowError(string content) => MessageBox.Show(content, "Brendigo Web2APK Builder",
        MessageBoxButton.OK, MessageBoxImage.Error);
    private static void ShowInfo(string content) => MessageBox.Show(content, "Brendigo Web2APK Builder",
        MessageBoxButton.OK, MessageBoxImage.Information);
}
