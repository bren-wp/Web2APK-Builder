using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow
{
    private void Wizard()
    {
        Heading("Novi Android projekt", "Korak " + (_wizardStep + 1) + " od 6 · " + StepNames[_wizardStep]);
        var steps = Row();
        steps.Margin = new Thickness(0,0,0,18);
        for (int i = 0; i < StepNames.Length; i++)
        {
            int chosen = i;
            var inner = new StackPanel { Orientation=Orientation.Horizontal,
                VerticalAlignment=VerticalAlignment.Center };
            var circle=new Border { Width=25,Height=25,CornerRadius=new CornerRadius(13),
                Background=i==_wizardStep?Accent:BrushOf("#111C39"),
                BorderThickness=new Thickness(1),
                BorderBrush=i==_wizardStep?BrushOf("#A090FF"):BrushOf("#40537E") };
            circle.Child=new TextBlock { Text=(i+1).ToString(),FontSize=11,
                Foreground=i==_wizardStep?White:Muted,
                HorizontalAlignment=HorizontalAlignment.Center,
                VerticalAlignment=VerticalAlignment.Center };
            inner.Children.Add(circle);
            inner.Children.Add(Txt(StepNames[i],12,i==_wizardStep,
                i==_wizardStep?White:Muted,new Thickness(8,0,0,0)));
            var button=Btn("",()=>ChangeStep(chosen));
            button.Content=inner;button.BorderThickness=new Thickness(0);
            button.Background=Brushes.Transparent;
            button.Padding=new Thickness(1);
            button.Margin=new Thickness(0,0,13,4);
            steps.Children.Add(button);
            if(i<StepNames.Length-1)
                steps.Children.Add(new Border { Width=27,Height=1,Background=Stroke,
                    Margin=new Thickness(0,0,14,0),VerticalAlignment=VerticalAlignment.Center });
        }
        PageHost.Children.Add(steps);
        var columns = Row();
        columns.Margin = new Thickness(0,0,0,0);
        var editor = Stack();
        var sidebar = Stack();
        _wizardMainCard = Card(editor, new Thickness(23));
        _wizardSummaryCard = Card(sidebar, new Thickness(23));
        _wizardMainCard.Width = 710;
        _wizardSummaryCard.Width = 305;
        columns.Children.Add(_wizardMainCard);
        columns.Children.Add(_wizardSummaryCard);
        PageHost.Children.Add(columns);
        switch (_wizardStep)
        {
            case 0: BasicStep(editor); break;
            case 1: AppearanceStep(editor); break;
            case 2: WebViewStep(editor); break;
            case 3: FeaturesStep(editor); break;
            case 4: PermissionStep(editor); break;
            case 5: BuildStep(editor); break;
        }
        Footer(editor);
        Summary(sidebar);
        UpdateResponsive();
    }

    private static void SectionHeader(StackPanel panel, string header, string desc)
    {
        panel.Children.Add(Txt(header, 17, true, White, new Thickness(0,0,0,6)));
        panel.Children.Add(Txt(desc, 12, false, Muted, new Thickness(0,0,0,21)));
    }

    private void BasicStep(StackPanel panel)
    {
        SectionHeader(panel, "Osnovne informacije", "Odredite izvor sadržaja i identitet Android aplikacije.");
        var two = Row();
        two.Children.Add(InputField("Naziv aplikacije *", _draft.Name, s => {
            _draft.Name = s; if (_summaryTitle != null) _summaryTitle.Text = s; }));
        two.Children.Add(InputField("Paket (applicationId) *", _draft.PackageName, s => _draft.PackageName = s));
        panel.Children.Add(two);
        var source = Row();
        source.Children.Add(Btn("🌐  Web-stranica · URL", () => { _draft.SourceType="url"; Navigate("new"); }, _draft.SourceType == "url"));
        source.Children.Add(Btn("◇  Lokalni HTML projekt", () => { _draft.SourceType="local"; Navigate("new"); }, _draft.SourceType == "local"));
        panel.Children.Add(source);
        if (_draft.SourceType == "url")
        {
            panel.Children.Add(InputField("URL web-stranice *", _draft.Url, s => _draft.Url = s, true));
            panel.Children.Add(Txt("Preporučeno: HTTPS i provjerena domena.", 11, false, Muted, new Thickness(0,0,0,15)));
        }
        else
        {
            panel.Children.Add(Txt("Lokalna mapa s index.html *", 12, true, White, new Thickness(0,0,0,7)));
            panel.Children.Add(Txt(string.IsNullOrEmpty(_draft.LocalPath) ? "Mapa nije odabrana." : _draft.LocalPath, 12,
                false, Muted, new Thickness(0,0,0,8)));
            panel.Children.Add(Btn("Odaberi HTML mapu", () => {
                var dialog = new OpenFolderDialog { Title = "Odaberi lokalni HTML projekt", Multiselect = false };
                if (dialog.ShowDialog(this) == true) { _draft.LocalPath=dialog.FolderName; Navigate("new"); }
            }, true));
        }
        var versions = Row();
        versions.Children.Add(InputField("Verzija aplikacije", _draft.VersionName, s => _draft.VersionName = s));
        versions.Children.Add(InputField("Version code", _draft.VersionCode.ToString(), s =>
            _draft.VersionCode = int.TryParse(s, out int n) ? n : 0));
        panel.Children.Add(versions);
        var sdk = Row();
        sdk.Children.Add(SelectField("Minimalni Android SDK", _draft.MinSdk.ToString(),
            ["24", "26", "28", "30", "33", "34", "35", "36"],
            s => _draft.MinSdk = int.Parse(s)));
        sdk.Children.Add(SelectField("Target SDK", _draft.TargetSdk.ToString(), ["36","37"],
            s => _draft.TargetSdk = int.Parse(s)));
        panel.Children.Add(sdk);
    }

    private StackPanel InputField(string label, string value, Action<string> set, bool wide = false)
    {
        var cell = Stack();
        cell.Margin = new Thickness(0,0,13,12);
        cell.Width = wide ? 475 : 300;
        cell.Tag = wide ? "wide" : "normal";
        cell.Children.Add(Txt(label, 12, true, White, new Thickness(0,0,0,7)));
        var box = new TextBox { Text = value, MaxLength = wide ? 2000 : 180,
            HorizontalAlignment = HorizontalAlignment.Stretch };
        box.TextChanged += (_, _) => set(box.Text);
        cell.Children.Add(box);
        _fieldCells.Add(cell);
        return cell;
    }

    private StackPanel SelectField(string label, string selected, string[] options, Action<string> set)
    {
        var cell = Stack();
        cell.Margin = new Thickness(0,0,13,12);
        cell.Width = 300;
        cell.Tag = "normal";
        cell.Children.Add(Txt(label, 12, true, White, new Thickness(0,0,0,7)));
        var cb = new ComboBox { ItemsSource = options, SelectedItem = selected };
        cb.SelectionChanged += (_, _) => { if (cb.SelectedItem is string s) set(s); };
        cell.Children.Add(cb);
        _fieldCells.Add(cell);
        return cell;
    }

    private static CheckBox OptionCheck(string label, string help, bool current, Action<bool> set)
    {
        var checkbox = new CheckBox { IsChecked = current, Content = new StackPanel {
            Children = {
                Txt(label, 13, true),
                Txt(help, 11, false, Muted, new Thickness(0,3,0,0))
            }
        }, Margin = new Thickness(0,5,0,9) };
        checkbox.Checked += (_, _) => set(true);
        checkbox.Unchecked += (_, _) => set(false);
        return checkbox;
    }

    private void AppearanceStep(StackPanel panel)
    {
        SectionHeader(panel, "Izgled i brending", "Postavite temu, orijentaciju i primarnu boju.");
        var row = Row();
        row.Children.Add(SelectField("Tema", _draft.Theme, ["dark","light"], s=>_draft.Theme=s));
        row.Children.Add(SelectField("Orijentacija", _draft.Orientation, ["portrait","landscape","unspecified"],
            s=>_draft.Orientation=s));
        panel.Children.Add(row);
        panel.Children.Add(Txt("Primarna boja", 12, true, White, new Thickness(0,12,0,8)));
        var colors = Row();
        foreach (var color in new[] { "#7253FF", "#3A86FF", "#31D4FF", "#20C997", "#FF8A4D" })
        {
            string chosen = color;
            var b = Btn(_draft.Accent == color ? "✓" : "●", () => { _draft.Accent=chosen; Navigate("new"); }, _draft.Accent == color);
            b.Background = BrushOf(color);
            b.Foreground = color == "#31D4FF" ? BrushOf("#10213F") : White;
            b.Width = 50;
            colors.Children.Add(b);
        }
        panel.Children.Add(colors);
        panel.Children.Add(Txt("Launcher ikonica (PNG)", 12, true, White, new Thickness(0,14,0,7)));
        panel.Children.Add(Txt(string.IsNullOrWhiteSpace(_draft.IconPath) ?
            "Zadana Brendigo ikona — odaberite svoju PNG datoteku." : _draft.IconPath,
            12, false, Muted, new Thickness(0,0,0,9)));
        panel.Children.Add(Btn("Odaberi launcher PNG (min. 192×192)", () => {
            var dialog = new OpenFileDialog { Title="Odaberi launcher ikonu", Filter="PNG image (*.png)|*.png" };
            if (dialog.ShowDialog(this)==true) { _draft.IconPath=dialog.FileName; Navigate("new"); }
        }));
        if (!string.IsNullOrEmpty(_draft.IconPath))
            panel.Children.Add(Btn("Vrati zadanu ikonu", () => { _draft.IconPath=""; Navigate("new"); }));
        panel.Children.Add(OptionCheck("Dinamički početni splash",
            "Prikaži izvornu Android splash animaciju i zadrži zaslon od 0 do 3000 ms.",
            _draft.Splash, b=>_draft.Splash=b));
        if (_draft.Splash)
            panel.Children.Add(SelectField("Trajanje splash zaslona (ms)", _draft.SplashDurationMs.ToString(),
                ["0","300","500","700","1000","1500","2000","3000"],
                s=>_draft.SplashDurationMs=int.Parse(s)));
        panel.Children.Add(OptionCheck("Statusna traka", "Prikaži standardnu Android statusnu traku.", _draft.StatusBar,
            b=>_draft.StatusBar=b));
        panel.Children.Add(OptionCheck("Puni zaslon", "Ukloni statusnu traku u generiranoj aplikaciji.", _draft.Fullscreen,
            b=>_draft.Fullscreen=b));
        panel.Children.Add(Notice("Prilagođavanje launcher ikonice i animiranog splash zaslona zahtijeva zaseban modul. " +
            "Za sada se generira osnovna vektorska Brendigo ikona."));
    }

    private void WebViewStep(StackPanel panel)
    {
        SectionHeader(panel, "WebView postavke", "Kontrola sadržaja i sigurnog pristupa web-stranici.");
        panel.Children.Add(OptionCheck("JavaScript", "Omogućite samo pouzdanim web-stranicama.", _draft.JavaScript, b=>_draft.JavaScript=b));
        panel.Children.Add(Divider());
        panel.Children.Add(OptionCheck("DOM Storage", "Lokalno spremanje u Android WebViewu.", _draft.DomStorage,b=>_draft.DomStorage=b));
        panel.Children.Add(Divider());
        panel.Children.Add(OptionCheck("Vanjske poveznice", "Poveznice na druge domene otvaraju se u zadanoj Android aplikaciji preglednika.",
            _draft.OpenExternal,b=>_draft.OpenExternal=b));
        panel.Children.Add(Divider());
        panel.Children.Add(OptionCheck("Dopusti nesigurni HTTP", "Koristi se samo za web-stranice koje ne podržavaju HTTPS.",
            _draft.AllowHttp,b=>_draft.AllowHttp=b));
        panel.Children.Add(Notice("File URL pristup, univerzalni pristup lokalnim datotekama i daljinsko debugiranje WebViewa zadano su isključeni."));
    }

    private void FeaturesStep(StackPanel panel)
    {
        SectionHeader(panel, "Funkcionalnosti", "Opcije koje su dostupne u trenutačnom Windows generatoru.");
        panel.Children.Add(Feature("✓  Android APK", "Izrada instalacijskog paketa korištenjem lokalnog Gradlea."));
        panel.Children.Add(Feature("✓  Android AAB", "Izrada App Bundlea za distribuciju prema pravilima trgovine."));
        panel.Children.Add(Feature("✓  Izvoz izvornog projekta", "Android Gradle projekt koji možete otvoriti u Android Studiju."));
        panel.Children.Add(OptionCheck("Odabir datoteka i prijenos u web obrasce",
            "Omogućuje Androidov sistemski odabir datoteka uz korisničku potvrdu.",
            _draft.FileUploads,b=>_draft.FileUploads=b));
        panel.Children.Add(OptionCheck("Preuzimanje datoteka",
            "Preuzima HTTP(S) datoteke u prostor aplikacije preko Android DownloadManagera.",
            _draft.Downloads,b=>_draft.Downloads=b));
        panel.Children.Add(Feature("✓  Lokalni keystore", "Vlastiti ključ ili sigurno generiran lokalni ključ."));
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Google AdMob · banner oglasi", 15, true));
        panel.Children.Add(OptionCheck("Omogući AdMob", "SDK se ugrađuje samo uz vaše vlastite AdMob identifikatore i UMP pristanak korisnika.",
            _draft.AdMobEnabled,b=>{ _draft.AdMobEnabled=b; Navigate("new"); }));
        if (_draft.AdMobEnabled) {
            panel.Children.Add(InputField("AdMob App ID (ca-app-pub-…~…)",_draft.AdMobAppId,
                s=>_draft.AdMobAppId=s,true));
            panel.Children.Add(InputField("Banner Ad Unit ID (ca-app-pub-…/…)",_draft.AdMobBannerId,
                s=>_draft.AdMobBannerId=s,true));
        }
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Firebase Cloud Messaging · Push", 15, true));
        panel.Children.Add(OptionCheck("Omogući Firebase Push",
            "Zahtijeva google-services.json s istim Android package ID-jem.",
            _draft.FirebasePushEnabled,b=>{_draft.FirebasePushEnabled=b;Navigate("new");}));
        if (_draft.FirebasePushEnabled) {
            panel.Children.Add(Txt(_draft.FirebaseConfigPath.Length == 0 ?
                "Nije odabrana google-services.json datoteka." : _draft.FirebaseConfigPath,
                12,false,Muted,new Thickness(0,0,0,8)));
            panel.Children.Add(Btn("Odaberi google-services.json", () => {
                var dialog=new OpenFileDialog { Title="Firebase Android konfiguracija",
                    Filter="Firebase JSON (google-services.json)|google-services.json|JSON datoteke (*.json)|*.json" };
                if (dialog.ShowDialog(this)==true) { _draft.FirebaseConfigPath=dialog.FileName;Navigate("new"); }
            }));
        }
        panel.Children.Add(Notice("AdMob traži stvarne identifikatore i pravilno konfiguriran pristanak. Firebase dostava zahtijeva Firebase projekt i test na uređaju."));

    }

    private void PermissionStep(StackPanel panel)
    {
        SectionHeader(panel,"Android dozvole i privatnost",
            "Svaka osjetljiva dozvola traži odobrenje korisnika na Android uređaju.");
        panel.Children.Add(OptionCheck("Kamera (WebRTC)",
            "Dodaje CAMERA dozvolu; sigurni WebView pita korisnika prije korištenja kamere.",
            _draft.Camera,b=>_draft.Camera=b));
        panel.Children.Add(Divider());
        panel.Children.Add(OptionCheck("Mikrofon (WebRTC)",
            "Dodaje RECORD_AUDIO; Android traži dopuštenje pri web zahtjevu.",
            _draft.Microphone,b=>_draft.Microphone=b));
        panel.Children.Add(Divider());
        panel.Children.Add(OptionCheck("Približna lokacija",
            "Dodaje ACCESS_COARSE_LOCATION; web-lokacija i Android zasebno provjeravaju dopuštenje.",
            _draft.Location,b=>_draft.Location=b));
        panel.Children.Add(Divider());
        panel.Children.Add(Feature("✓  Datoteke bez široke storage dozvole",
            "Sistemski odabir dokumenta ne zahtijeva READ/WRITE_EXTERNAL_STORAGE."));
        panel.Children.Add(Feature("✓  HTTPS i provjera web izvora",
            "Kamera, mikrofon i lokacija dopuštaju se samo za podrijetlo izvorne web-stranice."));
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Google Play · privatnost i objava",16,true,White,new Thickness(0,12,0,8)));
        panel.Children.Add(InputField("Javna HTTPS adresa politike privatnosti",_draft.PrivacyPolicyUrl,
            url=>_draft.PrivacyPolicyUrl=url,true));
        panel.Children.Add(OptionCheck("Imam pravo objaviti ovu web-stranicu kao aplikaciju",
            "Google Play ne dopušta neovlaštene WebView omote tuđih web-stranica.",
            _draft.WebsitePublishingPermissionConfirmed,b=>_draft.WebsitePublishingPermissionConfirmed=b));
        panel.Children.Add(OptionCheck("Pregledao/la sam Data safety i deklaracije SDK-ova",
            "Potvrda ne znači automatsko odobrenje; morate predati točne deklaracije u Play Consoleu.",
            _draft.DataSafetyReviewed,b=>_draft.DataSafetyReviewed=b));
        panel.Children.Add(Btn("Provjeri zahtjeve Google Playa",() =>
            ShowInfo(GooglePlayReadiness.AsText(_draft)),true));
        panel.Children.Add(Notice("Uključuj samo dozvole koje trebaju tvojoj aplikaciji. Android će zatražiti korisnikovu odluku, a odbijanje se poštuje."));
    }

    private static Border Feature(string title, string description)
    {
        var body = Stack();
        body.Children.Add(Txt(title, 14, true));
        body.Children.Add(Txt(description, 12, false, Muted, new Thickness(0,5,0,0)));
        var card = Card(body, new Thickness(13));
        card.Background = BrushOf("#172747");
        card.Margin = new Thickness(0,0,0,9);
        return card;
    }
    private static Border Notice(string text) => new() {
        Background = BrushOf("#252140"), BorderBrush = BrushOf("#594A91"),
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9),
        Padding = new Thickness(13), Margin = new Thickness(0,14,0,4),
        Child = Txt(text, 12, false, BrushOf("#C4B8E4")) };

    private void Footer(StackPanel panel)
    {
        var footer = Row();
        footer.Margin = new Thickness(0,25,0,0);
        if (_wizardStep > 0) footer.Children.Add(Btn("← Natrag", () => ChangeStep(_wizardStep-1)));
        footer.Children.Add(Btn("Spremi projekt", () => { if (SaveDraft()) ShowInfo("Projekt je lokalno spremljen."); }));
        footer.Children.Add(Btn("▷  Pregled", () => { if(SaveDraft()) Navigate("preview"); }));
        if (_wizardStep < StepNames.Length - 1)
            footer.Children.Add(Btn("Nastavi na " + StepNames[_wizardStep+1] + "  →", () => ChangeStep(_wizardStep+1), true));
        panel.Children.Add(footer);
    }

    private void Summary(StackPanel panel)
    {
        SectionHeader(panel, "Pregled projekta", "Trenutačne postavke.");
        var logo = new Border { Width=44, Height=44, CornerRadius=new CornerRadius(12),
            Background=Accent, Child=Txt("B", 27, true), HorizontalAlignment=HorizontalAlignment.Left };
        panel.Children.Add(logo);
        _summaryTitle = Txt(_draft.Name, 15, true, White, new Thickness(0,12,0,4));
        panel.Children.Add(_summaryTitle);
        panel.Children.Add(Txt(_draft.PackageName, 11, false, Muted));
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Izvor", 12, true));
        panel.Children.Add(Txt(_draft.SourceType == "local" ? "Lokalni HTML" : "HTTPS/URL",12,false,Muted));
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Target Android SDK",12,true));
        panel.Children.Add(Txt(_draft.TargetSdk.ToString(),12,false,Muted));
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Izvoz",12,true));
        panel.Children.Add(Txt("Gradle projekt, APK, AAB",12,false,Muted));
    }

    private void ChangeStep(int step)
    {
        if (_busy || step is < 0 or > 5) return;
        if (!SaveDraft()) return;
        _wizardStep=step;
        Navigate("new");
    }
    private bool SaveDraft()
    {
        try
        {
            _draft = _store.Save(_draft);
            return true;
        }
        catch (Exception e) { ShowError(e.Message); return false; }
    }
}
