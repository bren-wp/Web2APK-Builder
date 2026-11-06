using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow
{
    private void BuildStep(StackPanel panel)
    {
        SectionHeader(panel, "Izrada Android aplikacije", "Izvoz Android projekta ili lokalna izrada APK/AAB paketa.");
        var sdkChoice=Row();
        sdkChoice.Margin=new Thickness(0,0,0,10);
        sdkChoice.Children.Add(SelectField("Target Android SDK",_draft.TargetSdk.ToString(),
            ["36","37"],s=>_draft.TargetSdk=int.Parse(s)));
        panel.Children.Add(sdkChoice);
        panel.Children.Add(Btn("Google Play · provjeri spremnost",() =>
            ShowInfo(GooglePlayReadiness.AsText(_draft))));
        panel.Children.Add(Notice("Za APK/AAB moraju biti dostupni JDK 17, Gradle 9.6+ i Android SDK 36+. " +
            "Program ne preuzima razvojne alate bez vašeg odobrenja."));
        var authorized = LicenseActivationStore.Installed("windows")
            .Where(c => c.Tier == LicenseEntitlements.Developer || c.PackageName == _draft.PackageName)
            .OrderByDescending(c => c.Tier switch {
                "developer" => 4, "pro" => 3, "basic" => 2, _ => 1
            }).FirstOrDefault();
        var tierName = authorized is null ? "Nema licence za odabrani package ID"
            : LicenseEntitlements.DisplayName(authorized.Tier);
        panel.Children.Add(Feature("Aktivni licencni paket",tierName+" · "+(authorized?.PackageName ?? _draft.PackageName)));
        var actions = Row();
        var sourceBtn=Btn("⇩  Izvezi Android projekt", () => _ = ExportAsync(_draft));
        sourceBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"source");
        actions.Children.Add(sourceBtn);
        var apkBtn=Btn("▣  Izradi APK", () => _ = BuildAsync("apk"), true);
        apkBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"apk");
        actions.Children.Add(apkBtn);
        var aabBtn=Btn("▣  Izradi AAB", () => _ = BuildAsync("aab"), true);
        aabBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"aab");
        actions.Children.Add(aabBtn);
        actions.Children.Add(Btn("✕  Zaustavi", CancelBuild, false, true));
        panel.Children.Add(actions);
        panel.Children.Add(Divider());
        panel.Children.Add(Txt("Potpisivanje Android paketa (opcionalno)", 15, true, White, new Thickness(0,0,0,8)));
        panel.Children.Add(Txt("Certifikati ostaju na vašem računalu. Lozinke se ne spremaju u projekt.",
            12,false,Muted,new Thickness(0,0,0,11)));
        var signingRow = Row();
        _keyStoreBox = new TextBox { IsReadOnly = true, Width = 390, Text = "", Margin = new Thickness(0,0,10,7) };
        signingRow.Children.Add(_keyStoreBox);
        signingRow.Children.Add(Btn("Odaberi keystore", () => {
            var dialog = new OpenFileDialog { Title = "Android keystore",
                Filter = "Keystore (*.jks;*.keystore;*.p12)|*.jks;*.keystore;*.p12|Sve datoteke (*.*)|*.*" };
            if (dialog.ShowDialog(this) == true) _keyStoreBox.Text = dialog.FileName;
        }));
        panel.Children.Add(signingRow);
        panel.Children.Add(Btn("Generiraj novi slučajni ključ (lokalno)", async () => {
            if (MessageBox.Show("Izraditi trajni privatni ključ na ovom računalu? Za buduća ažuriranja mora se čuvati isti ključ i njegova lozinka.",
                "Novi Android ključ", MessageBoxButton.YesNo, MessageBoxImage.Warning)!=MessageBoxResult.Yes) return;
            try {
                var key=await LocalSigningKeyService.GenerateAsync(Path.Combine(_dataDir,"signing-keys"));
                _keyStoreBox!.Text=key.KeyStorePath;
                _keyAliasBox!.Text=key.Alias;
                _storePassword!.Password=key.Password;
                _keyPassword!.Password=key.Password;
                var password=key.Password;
                MessageBox.Show("Ključ je izrađen u lokalnoj mapi:\n"+key.KeyStorePath+
                    "\n\nAlias: "+key.Alias+"\nLOZINKA (prikazuje se samo sada):\n"+password+
                    "\n\nObavezno izradite sigurnosnu kopiju ključa i lozinke. Bez njih nećete moći potpisivati buduća ažuriranja.",
                    "Novi Android ključ — spremite lozinku",MessageBoxButton.OK,MessageBoxImage.Information);
            } catch(Exception ex) { ShowError("Generiranje ključa nije uspjelo: "+ex.Message); }
        },true));
        var alias = Row();
        alias.Children.Add(Txt("Alias:",12,true,White,new Thickness(0,10,8,0)));
        _keyAliasBox = new TextBox { Width=240, Margin = new Thickness(0,0,0,7) };
        alias.Children.Add(_keyAliasBox);
        panel.Children.Add(alias);
        var passwords = Row();
        var box1 = Stack(); box1.Margin = new Thickness(0,0,12,0);
        box1.Children.Add(Txt("Lozinka keystorea", 12, true, White, new Thickness(0,0,0,7)));
        _storePassword = new PasswordBox { Width = 225, MinHeight = 39 };
        box1.Children.Add(_storePassword);
        passwords.Children.Add(box1);
        var box2 = Stack();
        box2.Children.Add(Txt("Lozinka ključa", 12, true, White, new Thickness(0,0,0,7)));
        _keyPassword = new PasswordBox { Width = 225, MinHeight = 39 };
        box2.Children.Add(_keyPassword);
        passwords.Children.Add(box2);
        panel.Children.Add(passwords);
        panel.Children.Add(Txt("Gradle zapisnik",15,true,White,new Thickness(0,20,0,8)));
        _buildLog = new TextBox { Text = "Izradite projekt ili odaberite APK/AAB. Napredak će biti prikazan ovdje.",
            Height = 165, IsReadOnly = true, TextWrapping=TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontFamily = new FontFamily("Consolas"), FontSize=11 };
        panel.Children.Add(_buildLog);
    }

    private void CancelBuild()
    {
        if (!_busy || _buildCancellation is null) { ShowInfo("Trenutačno nije aktivna izrada."); return; }
        _buildCancellation.Cancel();
        AddLog("Zatraženo zaustavljanje izrade..." + Environment.NewLine);
    }
    private void AddLog(string text)
    {
        if (_buildLog is null) return;
        var trimmed = (_buildLog.Text + text);
        _buildLog.Text = trimmed.Length > 48000 ? trimmed[^48000..] : trimmed;
        _buildLog.ScrollToEnd();
    }

    private async Task ExportAsync(ProjectDefinition project)
    {
        if (_busy) return;
        try
        {
            LicenseActivationStore.Require("windows",project,"source");
            if (!SaveForExport(project, out var saved)) return;
            var dialog = new OpenFolderDialog { Title="Odaberite mapu za izvoz Android projekta", Multiselect=false };
            if (dialog.ShowDialog(this) != true) return;
            var output = Path.Combine(dialog.FolderName,
                saved.PackageName.Replace('.', '-') + "-android-" + saved.VersionName);
            if (Directory.Exists(output)) { ShowError("Projekt već postoji na odredištu. Odaberite novu mapu ili promijenite verziju."); return; }
            AndroidProjectGenerator.Generate(saved, output);
            ShowInfo("Android projekt izvezen:\n" + output);
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }
    private bool SaveForExport(ProjectDefinition requested, out ProjectDefinition stored)
    {
        stored = requested;
        try
        {
            stored = _store.Save(requested);
            if (_draft.Id == stored.Id) _draft = stored.Copy();
            return true;
        }
        catch (Exception ex) { ShowError(ex.Message); return false; }
    }

    private async Task BuildAsync(string format)
    {
        if (_busy) return;
        if (!SaveDraft()) return;
        try { LicenseActivationStore.Require("windows",_draft,format); }
        catch (UnauthorizedAccessException e) { ShowError(e.Message); return; }
        var dialog = new OpenFolderDialog { Title = "Odaberite izlaznu mapu za Android paket", Multiselect = false };
        if (dialog.ShowDialog(this) != true) return;
        SigningOptions? signing = null;
        if (_keyStoreBox is { Text.Length: > 0 })
        {
            signing = new SigningOptions(_keyStoreBox.Text, _keyAliasBox?.Text ?? "",
                _storePassword?.Password ?? "", _keyPassword?.Password ?? "");
        }
        try { signing?.Validate(); }
        catch (Exception ex) { ShowError(ex.Message); return; }
        _busy = true;
        _buildCancellation = new CancellationTokenSource();
        AddLog(Environment.NewLine + "Pokrećem lokalnu izradu " + format.ToUpperInvariant() + "..." + Environment.NewLine);
        try
        {
            var result = await AndroidBuildService.BuildAsync(_draft.Copy(), dialog.FolderName, format,
                signing, line => Dispatcher.BeginInvoke(() => AddLog(line)), _buildCancellation.Token);
            AddLog("Izrada uspješno završena: " + result.File + Environment.NewLine);
            ShowInfo("Paket izrađen:\n" + result.File +
                (result.Signed ? "\nAndroid paket je potpisan." :
                "\nAndroid paket NIJE potpisan; nije spreman za produkcijsku distribuciju."));
        }
        catch (OperationCanceledException) { AddLog("Izrada je otkazana." + Environment.NewLine); }
        catch (Exception ex) { AddLog("POGREŠKA: " + ex.Message + Environment.NewLine); ShowError(ex.Message); }
        finally
        {
            _storePassword?.Clear(); _keyPassword?.Clear();
            _buildCancellation?.Dispose(); _buildCancellation = null;
            _busy = false;
        }
    }

    private void Tools()
    {
        Heading("Razvojni alati", "Dijagnostika lokalnog Windows/Android build okruženja.");
        var tools = Stack();
        tools.Children.Add(Txt("Java / JDK, Gradle i ADB", 17, true));
        var results = new TextBox { IsReadOnly=true, Height=350, TextWrapping=TextWrapping.Wrap,
            VerticalScrollBarVisibility=ScrollBarVisibility.Auto, FontFamily=new FontFamily("Consolas"), FontSize=12,
            Text="Kliknite „Provjeri alate” za pregled verzija." };
        var panel = Row();
        var refresh = Btn("↻  Provjeri alate", async () => {
            results.Text = "Provjera dostupnih alata...";
            var java = await AndroidBuildService.ProbeAsync("java", "-version");
            var gradle = await AndroidBuildService.ProbeAsync("gradle", "--version");
            var adb = await AndroidBuildService.ProbeAsync("adb", "version");
            var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME") ??
                Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "(nije postavljen)";
            results.Text = "JAVA:\n" + java + "\n\nGRADLE:\n" + gradle +
                "\n\nADB:\n" + adb + "\n\nANDROID SDK:\n" + sdk;
        },true);
        panel.Children.Add(refresh);
        tools.Children.Add(panel);
        tools.Children.Add(results);
        tools.Children.Add(Divider());
        tools.Children.Add(Txt("Google Play · priprema za objavu",17,true,White,new Thickness(0,12,0,10)));
        tools.Children.Add(Feature("Target API 36+, potpis, dozvole i politika privatnosti",
            "Provjera podsjeća na Google Play deklaracije i uvjete. Ne zamjenjuje Play Console pregled."));
        tools.Children.Add(Btn("Pokreni provjeru Google Playa",() => {
            try { results.Text=GooglePlayReadiness.AsText(_draft);results.ScrollToHome(); }
            catch(Exception ex) { ShowError(ex.Message); }
        },true));
        tools.Children.Add(Btn("Izvezi kontrolni popis",() => {
            var dialog=new SaveFileDialog { Title="Google Play provjera",Filter="Tekst (*.txt)|*.txt",
                DefaultExt=".txt",FileName="Brendigo-GooglePlay-Checklist.txt" };
            if(dialog.ShowDialog(this)!=true)return;
            try { File.WriteAllText(dialog.FileName,GooglePlayReadiness.AsText(_draft));
                ShowInfo("Kontrolni popis je spremljen."); }
            catch(Exception ex) { ShowError(ex.Message); }
        }));
        tools.Children.Add(Feature("Gradle 9.6.1 · lokalno upravljanje",
            ToolchainService.IsGradleInstalled ? "Službeni Gradle je instaliran u korisničkom profilu." :
                "Gradle nije instaliran unutar Studija. Možete ga preuzeti uz prethodno odobrenje."));
        tools.Children.Add(Btn("Preuzmi i instaliraj provjereni Gradle 9.6.1", async () => {
            if (ToolchainService.IsGradleInstalled) { ShowInfo("Gradle je već instaliran."); return; }
            if (MessageBox.Show("Preuzeti ~135 MB iz službenog Gradle repozitorija?\\n" +
                "Provjerava se SHA-256 i datoteke se pohranjuju samo na vašem računalu.",
                "Instalacija Gradlea", MessageBoxButton.YesNo, MessageBoxImage.Question)!=MessageBoxResult.Yes) return;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(25));
            try {
                results.Text = "Instalacija...";
                var path=await ToolchainService.InstallGradleAsync(
                    msg=>Dispatcher.BeginInvoke(() => results.AppendText(msg+"\\n")), cancellation.Token);
                results.AppendText("Gradle spreman: "+path);
                ShowInfo("Provjereni Gradle 9.6.1 uspješno je instaliran.");
            } catch(Exception ex) { ShowError("Gradle instalacija nije uspjela: "+ex.Message); }
        },true));
        tools.Children.Add(Notice("JDK 17 i Android SDK 36/37 moraju biti instalirani. " +
            "SDK se preuzima nakon prihvaćanja Googleovih uvjeta, stoga se ne uključuje automatski u instalaciju Studija."));
        PageHost.Children.Add(Card(tools));
    }

    private void Devices()
    {
        Heading("Uređaji", "Otkrivanje povezanih Android uređaja i emulatora putem lokalnog ADB-a.");
        var stack = Stack();
        var result = new TextBox { Text="Kliknite Osvježi kako biste pronašli uređaje.\nPotreban je uključen USB debugging.",
            Height=250, IsReadOnly=true, TextWrapping=TextWrapping.Wrap,
            FontFamily=new FontFamily("Consolas"), VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        stack.Children.Add(Btn("↻  Osvježi Android uređaje", async () => {
            result.Text="Traženje uređaja...";
            result.Text = await AndroidBuildService.ProbeAsync("adb", "devices", "-l");
        }, true));
        stack.Children.Add(result);
        stack.Children.Add(Notice("ADB služi samo za dijagnostiku. Program ne instalira aplikacije bez izričite radnje korisnika."));
        PageHost.Children.Add(Card(stack));
    }

    private void Settings()
    {
        Heading("Postavke", "Prilagodite izgled, sigurnost i način rada aplikacije.");
        var settings = Stack();
        var installed=LicenseActivationStore.Installed("windows");
        settings.Children.Add(Txt("Licence i paketi",17,true,White,new Thickness(0,0,0,10)));
        foreach(var c in installed)
            settings.Children.Add(Feature(LicenseEntitlements.DisplayName(c.Tier),
                (c.Tier=="developer"?"Neograničeno aplikacija":c.PackageName)+
                " · doživotno · "+(c.Audience=="both"?"Windows + Android":"Windows")));
        settings.Children.Add(Btn("Dodaj novu licencu",()=>{
            var dialog=new LicenseActivationWindow { Owner=this };
            if(dialog.ShowDialog()==true) Navigate("settings");
        },true));
        settings.Children.Add(Divider());
        settings.Children.Add(Feature("Windows sučelje", "Ujednačeni prikaz, jasni izbornici i prilagodljiv raspored."));
        settings.Children.Add(Feature("Veličina prozora", "Promijenite veličinu prozora ili ga minimizirajte u svakom trenutku."));
        settings.Children.Add(Feature("Prilagodba zaslonu", "Izbornik i obrasci automatski se prilagođavaju dostupnom prostoru."));
        settings.Children.Add(Feature("Lokalna privatnost", "Podaci projekata spremaju se na lokalni disk; nema korisničkih računa ni telemetrije."));
        settings.Children.Add(Feature("Sigurno potpisivanje", "Privatni ključ i lozinke za Android ostaju na vašem računalu."));
        settings.Children.Add(Btn("Otvori lokalnu mapu podataka", () =>
            Process.Start(new ProcessStartInfo { FileName = _dataDir, UseShellExecute = true }), true));
        PageHost.Children.Add(Card(settings));
    }

    private void Docs()
    {
        Heading("Dokumentacija", "Upute za izradu, uređivanje i potpisivanje aplikacija.");
        var docs = Stack();
        docs.Children.Add(Feature("1. Novi projekt", "Unesite naziv, Android applicationId, HTTPS URL ili odaberite lokalnu mapu s index.html."));
        docs.Children.Add(Feature("2. Izgled i WebView", "Postavite boju, temu, orijentaciju, JavaScript, DOM Storage i vanjske poveznice."));
        docs.Children.Add(Feature("3. Spremanje", "Projekt se čuva lokalno i možete ga kasnije otvoriti na popisu „Moji projekti”."));
        docs.Children.Add(Feature("4. Izvoz izvornog projekta", "Koristite „Izvezi Android projekt” za izradu Gradle projekta koji možete otvoriti u Android Studiju."));
        docs.Children.Add(Feature("5. APK/AAB", "Instalirajte JDK 17, Gradle 9.6+ i Android SDK 36. Za potpisivanje odaberite keystore i unesite lozinke."));
        docs.Children.Add(Notice("Za Google Play potrebni su valjani certifikati, testiranje, metapodaci i aktualni zahtjevi trgovine."));
        PageHost.Children.Add(Card(docs));
    }

    private void About()
    {
        Heading("O programu", "Brendigo Web2APK Builder");
        var body=Stack();
        var logo = new Border { Background=Accent, CornerRadius=new CornerRadius(16),
            Width=80, Height=80, HorizontalAlignment=HorizontalAlignment.Left,
            Child=Txt("B",58,true) };
        body.Children.Add(logo);
        body.Children.Add(Txt("Brendigo Web2APK Builder",25,true,White,new Thickness(0,20,0,8)));
        body.Children.Add(Txt("Verzija 0.3.2 · Windows",13,false,Muted));
        body.Children.Add(Txt("Autor: Brendigo\nbrendigo.com",14,false,White,new Thickness(0,12,0,12)));
        body.Children.Add(Txt("Lokalna Windows aplikacija za projektiranje Android WebView aplikacija i generiranje APK/AAB paketa.",
            13,false,Muted,new Thickness(0,0,0,15)));
        body.Children.Add(Btn("Otvori brendigo.com", () =>
            Process.Start(new ProcessStartInfo { FileName = "https://brendigo.com", UseShellExecute = true }),true));
        PageHost.Children.Add(Card(body));
    }
}
