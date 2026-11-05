using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
    private Border? _wizardMainCard;
    private Border? _wizardSummaryCard;
    private CancellationTokenSource? _buildCancellation;
    private TextBox? _buildLog;
    private TextBox? _keyStoreBox;
    private TextBox? _keyAliasBox;
    private PasswordBox? _storePassword;
    private PasswordBox? _keyPassword;
    private bool _busy;

    private static Brush BrushOf(string color) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    private static readonly Brush Background = BrushOf("#0C1630");
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
        Navigate("dashboard");
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateResponsive();
    private void UpdateResponsive()
    {
        if (NavigationHost is null) return;
        var compact = ActualWidth < 1080;
        SidebarColumn.Width = new GridLength(compact ? 84 : 245);
        BrandWords.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        foreach (var entry in _navButtons)
        {
            var item = entry.Value.Content as StackPanel;
            if (item?.Children.Count > 1 && item.Children[1] is TextBlock label)
                label.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }
        var viewport = Math.Max(550, PageScroll?.ActualWidth ?? 900);
        var statsPerRow = viewport > 1100 ? 4 : viewport > 630 ? 2 : 1;
        var statWidth = Math.Max(210, (viewport - 80 - (statsPerRow - 1) * 14) / statsPerRow);
        foreach (var card in _stats) card.Width = statWidth;
        var fieldWidth = Math.Clamp((viewport - 130) / (viewport > 1050 ? 2 : 1), 245, 410);
        foreach (var field in _fieldCells) field.Width = fieldWidth;
        if (_wizardMainCard is not null && _wizardSummaryCard is not null)
        {
            var two = viewport > 1040;
            _wizardMainCard.Width = two ? viewport - 420 : viewport - 72;
            _wizardSummaryCard.Width = two ? 300 : viewport - 72;
        }
    }

    private void MakeNavigation()
    {
        NavigationHost.Children.Clear();
        NavigationHost.Children.Add(Caption("RADNI PROSTOR"));
        foreach (var (key, label, icon) in new[] {
            ("dashboard","Početna","\uE80F"),("new","Novi projekt","\uE710"),
            ("projects","Moji projekti","\uE8B7"),("tools","Razvojni alati","\uE713"),
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
            Background = primary ? Accent : danger ? BrushOf("#573047") : BrushOf("#1C2A4C"),
            Margin = new Thickness(0,0,10,8), MinHeight = 42 };
        b.Click += (_, _) => click();
        return b;
    }
    private static WrapPanel Row() => new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0,9,0,8) };
    private static Border Divider() => new() { Height = 1, Background = Stroke, Margin = new Thickness(0,13,0,15) };

    private void Navigate(string page)
    {
        _page = page;
        _summaryTitle = null; _wizardMainCard = null; _wizardSummaryCard = null;
        _stats.Clear(); _fieldCells.Clear();
        PageHost.Children.Clear();
        Breadcrumb.Text = "Radni prostor   ›   " + (page switch {
            "dashboard" => "Početna", "new" => "Novi projekt", "projects" => "Moji projekti",
            "tools" => "Razvojni alati", "devices" => "Uređaji", "settings" => "Postavke",
            "docs" => "Dokumentacija", "about" => "O programu", _ => "Web2APK" });
        foreach (var item in _navButtons)
            item.Value.Background = item.Key == page ? BrushOf("#302B73") : Brushes.Transparent;
        try
        {
            switch (page)
            {
                case "dashboard": Dashboard(); break;
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
        var hero = new Grid { MinHeight = 235, Margin = new Thickness(0,0,0,16) };
        hero.Background = new LinearGradientBrush(BrushOf("#203969") is SolidColorBrush s ? s.Color : Colors.Navy,
            (Color)ColorConverter.ConvertFromString("#413083"), 17);
        var border = new Border { BorderBrush = BrushOf("#5750A5"), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(21), Background = hero.Background, Padding = new Thickness(30,25,20,20),
            Margin = new Thickness(0,0,0,0) };
        var body = Stack();
        body.Children.Add(Txt("✦ STUDIO · NOVI PROJEKT", 11, true, BrushOf("#C3AFFF"), new Thickness(0,0,0,12)));
        body.Children.Add(Txt("Od web-stranice do Android aplikacije.", 28, true, White,
            new Thickness(0,0,0,10)));
        body.Children.Add(Txt("Povežite URL ili lokalni HTML projekt, prilagodite izgled i izradite Android paket.", 14, false,
            BrushOf("#C6D1EC"), new Thickness(0,0,0,16)));
        var actions = Row();
        actions.Children.Add(Btn("＋  Novi projekt", StartNew, true));
        actions.Children.Add(Btn("▣  Moji projekti", () => Navigate("projects")));
        body.Children.Add(actions);
        border.Child = body;
        hero.Children.Add(border);
        PageHost.Children.Add(hero);

        var projects = _store.List();
        var stats = Row();
        stats.Children.Add(Stat("▣", projects.Count.ToString(), "Ukupno projekata", "#214D85"));
        stats.Children.Add(Stat("◇", projects.Count(p => p.SourceType == "local").ToString(), "HTML projekti", "#3D317A"));
        stats.Children.Add(Stat("⬡", projects.Count(p => !p.AllowHttp).ToString(), "HTTPS konfiguracije", "#155B55"));
        stats.Children.Add(Stat("◷", projects.Count(p => p.UpdatedAt > DateTime.UtcNow.AddDays(-7)).ToString(), "Aktivni ovaj tjedan", "#645032"));
        PageHost.Children.Add(stats);

        var title = new DockPanel { Margin = new Thickness(0,13,0,12) };
        var all = Btn("Svi projekti  →", () => Navigate("projects"));
        all.HorizontalAlignment = HorizontalAlignment.Right;
        DockPanel.SetDock(all, Dock.Right);
        title.Children.Add(all);
        title.Children.Add(Txt("Posljednji projekti", 18, true, White, new Thickness(0,3,0,0)));
        PageHost.Children.Add(title);
        var list = Stack();
        if (projects.Count == 0)
        {
            list.Children.Add(Txt("Još nema projekata. Izradi prvi Android projekt.", 14, false, Muted, new Thickness(0,12,0,15)));
            list.Children.Add(Btn("＋  Kreiraj projekt", StartNew, true));
        }
        else foreach (var p in projects.Take(5)) list.Children.Add(ProjectRow(p, false));
        PageHost.Children.Add(Card(list));
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
