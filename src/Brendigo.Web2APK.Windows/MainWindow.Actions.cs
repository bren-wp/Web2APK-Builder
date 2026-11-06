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
        panel.Children.Add(Notice("Za APK/AAB Studio provjerava JDK 17, Gradle 9.6+ i Android SDK 36. " +
            "Ako alat nedostaje, sam ga priprema u lokalnoj mapi nakon vaše privole. " +
            "Android Studio ne morate instalirati."));
        var authorized = LicenseActivationStore.Installed("windows")
            .Where(c => LicenseActivationStore.ScopeMatches(c,_draft.PackageName))
            .OrderByDescending(c => c.Tier switch {
                "developer" => 5, "agency" => 4, "pro" => 3, "basic" => 2, _ => 1
            }).FirstOrDefault();
        var tierName = authorized is null ? "Nema slobodnog licencnog mjesta za ovaj package ID"
            : LicenseEntitlements.DisplayName(authorized.Tier);
        panel.Children.Add(Feature("Aktivni licencni paket",tierName+" · "+(authorized?.PackageName ?? _draft.PackageName)));
        var actions = Row();
        var sourceBtn=Btn("⇩  Izvezi Android projekt", () => _ = ExportAsync(_draft));
        sourceBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"source");
        actions.Children.Add(sourceBtn);
        var viewSource=Btn("⌘  Pregledaj izvorni kod",()=>OpenSourceViewer(_draft));
        viewSource.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"source");
        actions.Children.Add(viewSource);
        var apkBtn=Btn("▣  Izradi APK", () => _ = BuildAsync("apk"), true);
        apkBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"apk");
        actions.Children.Add(apkBtn);
        var debugBtn=Btn("▷  Debug APK",()=>_ = BuildAsync("apk-debug"));
        debugBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"apk");
        actions.Children.Add(debugBtn);
        var unsignedBtn=Btn("◇  Nepotpisani APK",()=>_ = BuildAsync("apk-unsigned"));
        unsignedBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"apk");
        actions.Children.Add(unsignedBtn);
        var aabBtn=Btn("▣  Izradi AAB", () => _ = BuildAsync("aab"), true);
        aabBtn.IsEnabled=authorized is not null && LicenseEntitlements.Allows(authorized,"aab");
        actions.Children.Add(aabBtn);
        actions.Children.Add(Btn("✕  Zaustavi", CancelBuild, false, true));
        panel.Children.Add(actions);
        panel.Children.Add(Notice("Debug APK automatski koristi razvojni Android ključ i nije za Play. " +
            "Nepotpisani APK/AAB nisu za izravnu objavu. Za produkciju zadržite isti keystore pri svakom ažuriranju."));
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

    private void OpenSourceViewer(ProjectDefinition project)
    {
        if(_busy)return;
        try {
            // The source viewer itself enforces the signed entitlement before generating code.
            // Saving an edit should not consume a previously unused application seat.
            if(!SaveForExport(project,out var saved))return;
            var viewer=new SourceViewerWindow(saved) { Owner=this };
            viewer.ShowDialog();
        }
        catch(Exception e){ShowError("Pregled izvornog koda nije uspio: "+e.Message);}
    }

    private async Task ExportAsync(ProjectDefinition project)
    {
        if (_busy) return;
        try
        {
            if(!LicenseActivationStore.TryAuthorize("windows",project,"source",out _,out var why))
                throw new UnauthorizedAccessException(why);
            if (!SaveForExport(project, out var saved)) return;
            var dialog = new OpenFolderDialog { Title="Odaberite mapu za izvoz Android projekta", Multiselect=false };
            if (dialog.ShowDialog(this) != true) return;
            var output = Path.Combine(dialog.FolderName,
                saved.PackageName.Replace('.', '-') + "-android-" + saved.VersionName);
            if (Directory.Exists(output)) { ShowError("Projekt već postoji na odredištu. Odaberite novu mapu ili promijenite verziju."); return; }
            // Reserve only after a destination has been confirmed and validated.
            LicenseActivationStore.Require("windows",saved,"source");
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

    /// <summary>
    /// No separate installer is needed. After consent the Studio prepares all tools
    /// in its own user folder. Google's SDK terms prevent us bundling it inside EXEs.
    /// </summary>
    private async Task EnsureBuildToolchainAsync(CancellationToken token)
    {
        var java=Environment.GetEnvironmentVariable("JAVA_HOME");
        bool haveJava=ToolchainService.IsLocalJdkInstalled ||
            (!string.IsNullOrWhiteSpace(java) &&
                File.Exists(Path.Combine(java,"bin","java.exe")));
        var sdk=Environment.GetEnvironmentVariable("ANDROID_HOME") ??
            Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "";
        bool haveSdk=ToolchainService.IsLocalAndroidPlatformInstalledFor(_draft.TargetSdk) ||
            (!string.IsNullOrWhiteSpace(sdk) &&
                File.Exists(Path.Combine(sdk,"platforms","android-"+_draft.TargetSdk,"android.jar")));
        bool haveGradle=ToolchainService.IsGradleInstalled;
        if(haveJava&&haveSdk&&haveGradle)return;
        var confirm=MessageBox.Show(
            "Za izradu APK/AAB Studio će pripremiti potrebne razvojne alate " +
            "iz službenih izvora u vlastitoj lokalnoj mapi. Nema potrebe zasebno "+
            "instalirati Android Studio.\n\n" +
            "Preuzimanje zahtijeva internetsku vezu i može zauzeti više stotina MB. " +
            "Preuzete arhive provjeravaju se SHA-256. " +
            "Googleovi SDK uvjeti: https://developer.android.com/studio/terms\n\n" +
            "Odabirom DA potvrđujete da ste pročitali i prihvatili te uvjete. "+
            "Želite li nastaviti?",
            "Priprema Android alata",MessageBoxButton.YesNo,MessageBoxImage.Question);
        if(confirm!=MessageBoxResult.Yes)
            throw new OperationCanceledException("Priprema alata nije odobrena.");
        var progress=new Action<string>(msg=>
            Dispatcher.BeginInvoke(() => AddLog(msg+Environment.NewLine)));
        if(!haveJava)await ToolchainService.InstallJdkAsync(progress,token);
        if(!haveGradle)await ToolchainService.InstallGradleAsync(progress,token);
        if(!haveSdk)await ToolchainService.InstallAndroidSdkAsync(progress,token,_draft.TargetSdk);
        UpdateToolStatus();
    }

    private async Task BuildAsync(string format)
    {
        if (_busy) return;
        if (!SaveDraft()) return;
        // Read-only entitlement check: cancelling the folder picker must not consume a seat.
        if(!LicenseActivationStore.TryAuthorize("windows",_draft,
            format=="aab"?"aab":"apk",out _,out var licenseReason))
        { ShowError(licenseReason); return; }
        // AndroidBuildService performs the binding under the same cross-process seat lock
        // only when an actual build is started (after signing and toolchain preparation).
        var dialog = new OpenFolderDialog { Title = "Odaberite izlaznu mapu za Android paket", Multiselect = false };
        if (dialog.ShowDialog(this) != true) return;
        SigningOptions? signing = null;
        if (format is not ("apk-debug" or "apk-unsigned") &&
            _keyStoreBox is { Text.Length: > 0 })
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
            await EnsureBuildToolchainAsync(_buildCancellation.Token);
            var result = await AndroidBuildService.BuildAsync(_draft.Copy(), dialog.FolderName, format,
                signing, line => Dispatcher.BeginInvoke(() => AddLog(line)), _buildCancellation.Token);
            AddLog("Izrada uspješno završena: " + result.File + Environment.NewLine);
            ShowInfo("Paket izrađen:\n" + result.File +
                (result.Debug ? "\nDebug APK: automatski razvojni potpis. Nije za Google Play." :
                result.Signed ? "\nPotpisano odabranim produkcijskim ključem." :
                "\nPaket NIJE potpisan; prije distribucije potreban je odgovarajući potpis."));
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
        tools.Children.Add(Feature("Android alatni lanac unutar Brendigo Studija",
            "JDK 17: "+(ToolchainService.IsLocalJdkInstalled?"spreman":"nije preuzet")+
            " · Gradle: "+(ToolchainService.IsGradleInstalled?"spreman":"nije preuzet")+
            " · Android SDK 36/37: "+(ToolchainService.IsLocalAndroidPlatformInstalled?"spreman":"nije preuzet")));
        tools.Children.Add(Btn("Pripremi JDK + Gradle + Android SDK unutar Studija",async ()=>{
            const string instructions="Studio može preuzeti JDK 17, Gradle 9.6.1 i Android SDK 36 " +
                "iz službenih izvora, provjeriti SHA-256 i pohraniti ih u svoj korisnički profil.\\n\\n" +
                "Preuzimanje zahtijeva internetsku vezu i više stotina MB prostora. " +
                "Googleovi Android SDK uvjeti primjenjuju se na korištenje SDK-a. " +
                "Nastavkom potvrđujete da ste pročitali i prihvatili Googleove SDK uvjete " +
                "(developer.android.com/studio/terms). Niti jedna druga aplikacija neće se samostalno instalirati.\\n\\n" +
                "Nastaviti?";
            if(MessageBox.Show(instructions,"Priprema Android alata",
                MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            using var cancellation=new CancellationTokenSource(TimeSpan.FromMinutes(90));
            try {
                results.Text="Pripremam provjerene razvojne alate..."+Environment.NewLine;
                var logger=new Action<string>(message=>Dispatcher.BeginInvoke(()=>
                    results.AppendText(message+Environment.NewLine)));
                await ToolchainService.InstallJdkAsync(logger,cancellation.Token);
                await ToolchainService.InstallGradleAsync(logger,cancellation.Token);
                await ToolchainService.InstallAndroidSdkAsync(logger,cancellation.Token,_draft.TargetSdk);
                results.AppendText("JDK, Gradle i Android SDK spremni su za izradu aplikacija.");
                UpdateToolStatus();
                ShowInfo("Alati su uspješno pripremljeni unutar Brendigo Studija.");
            } catch(OperationCanceledException) {ShowError("Priprema je prekinuta.");}
            catch(Exception ex){ShowError("Priprema alata nije uspjela: "+ex.Message);}
        },true));
        tools.Children.Add(Notice("Za prvu izradu potrebno je preuzeti razvojni alatni lanac. " +
            "Studio to može obaviti bez instalacije Android Studija. " +
            "Android SDK koristi Googleove uvjete; za Google Play zasebno je potrebna objava putem vašeg računa."));
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
        var installed=LicenseActivationStore.InstalledMasked("windows");
        settings.Children.Add(Txt("Licence i paketi",17,true,White,new Thickness(0,0,0,10)));
        if(installed.Count==0)settings.Children.Add(Notice("Nema aktivnih licenci."));
        foreach(var entry in installed)
        {
            var c=entry.Claims;
            var card=Stack();
            card.Children.Add(Txt(LicenseEntitlements.DisplayName(c.Tier),15,true));
            var scope=c.Tier is "developer" or "agency" ? "Neograničeno aplikacija" :
                c.PackageName==LicenseEntitlements.AutomaticScope
                    ? AutomaticLicenseSeats.Usage(c,out var used,out var status)
                        ? $"Automatski · iskorišteno {used}/{LicenseEntitlements.ApplicationLimit(c.Tier)} · preostalo {LicenseEntitlements.ApplicationLimit(c.Tier)-used}"
                        : "Evidencija mjesta nije dostupna: "+status
                    : c.PackageName.Replace(",",", ");
            card.Children.Add(Txt(scope+
                (c.ExpiresAt==0?" · doživotno":" · vrijedi do "+
                    DateTimeOffset.FromUnixTimeSeconds(c.ExpiresAt).ToLocalTime().ToString("dd.MM.yyyy"))+
                " · "+(c.Audience=="both"?"Windows + Android":"Windows"),
                12,false,Muted,new Thickness(0,5,0,7)));
            card.Children.Add(Txt("Licencni ključ: "+entry.MaskedToken,11,false,Muted));
            card.Children.Add(Btn("Zamijeni ovu licencu",()=>{
                var window=new LicenseActivationWindow {Owner=this,ReplacingSerial=c.Serial};
                if(window.ShowDialog()==true)Navigate("settings");
            }));
            settings.Children.Add(Card(card,new Thickness(14)));
        }
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
        Heading("Dokumentacija","Cjelovite upute za Brendigo Web2APK Builder.");
        var docs=Stack();
        docs.Children.Add(Feature("01 · Prva upotreba i licence",
            "U Postavkama otvorite Licence. Dodajte BL2 ključ, provjerite paket i maskirani identifikator. "+
            "Nove licence automatski povezuju tri aplikacije u paketu Profesionalac; stare BL2 licence zadržavaju potpisane ID-jeve. Agencija i Developer nemaju limit. Zamjena licence provjerava novi ključ prije uklanjanja starog."));
        docs.Children.Add(Feature("02 · Novo Android izdanje",
            "Odaberite Novi projekt i unesite naziv (2–70 znakova), package ID, izvornu HTTPS web-adresu ili mapu s index.html. "+
            "Package ID nije moguće olako mijenjati nakon objave na Google Playu. Povećajte versionCode za svako ažuriranje."));
        docs.Children.Add(Feature("03 · Zadana ikonica, splash i izgled",
            "Svaki projekt započinje s Brendigo ikonom i splash zaslonom. U Demo paketu oboje je zaključano. "+
            "Osnovno, Profesionalac, Agencija i Developer mogu odabrati vlastitu PNG ikonu od najmanje 192×192, "+
            "prilagoditi boju i isključiti Brendigo splash. Pregledajte rezultat na stvarnom Android uređaju."));
        docs.Children.Add(Feature("04 · WebView, offline HTML i kompatibilnost",
            "HTTPS je zadano obvezan. JavaScript i DOM Storage uključujte samo za pouzdani sadržaj. "+
            "Za offline HTML odaberite mapu s index.html. Vanjske poveznice mogu se otvoriti u pregledniku. "+
            "WebView pregled na Windowsu ne zamjenjuje test na telefonu."));
        docs.Children.Add(Feature("05 · Kamera, mikrofon, datoteke i lokacija",
            "Osjetljive dozvole uključujte samo kada su potrebne. Android od korisnika zasebno traži odobrenje. "+
            "Kamera, mikrofon i lokacija dostupni su od paketa Profesionalac. "+
            "Izbjegavajte široke storage dozvole; za datoteke se koristi sistemski odabir."));
        docs.Children.Add(Feature("06 · Push i oglasi",
            "Profesionalac, Agencija i Developer mogu uključiti povlačenje za osvježavanje i Firebase Push. "+
            "Za Firebase odaberite odgovarajući google-services.json s istim package ID-jem. "+
            "AdMob je funkcija paketa Profesionalac, Agencija i Developer i traži vlastite oglasne identifikatore, UMP privolu i deklaracije."));
        docs.Children.Add(Feature("07 · Android alati bez drugih programa",
            "Otvorite Razvojni alati i odaberite Pripremi JDK + Gradle + Android SDK unutar Studija. "+
            "Pročitajte i prihvatite uvjete, omogućite preuzimanje, a Studio provjerava kontrolne zbrojeve i "+
            "instalira alatni lanac u vlastitu mapu. Preuzimanje zahtijeva internet i dovoljno prostora."));
        docs.Children.Add(Feature("08 · APK, debug i nepotpisani paketi",
            "U šestom koraku birajte Debug APK (razvojno potpisan), Nepotpisani APK ili APK s vlastitim ključem. "+
            "Za AAB se koristi zaseban gumb; bez keystorea rezultat je nepotpisani AAB. "+
            "Debug nije pogodan za Google Play. Nepotpisani paket zahtijeva naknadno potpisivanje."));
        docs.Children.Add(Feature("09 · Sigurni ključ za potpis i ažuriranja",
            "Odaberite postojeću JKS/P12 datoteku ili izradite novi lokalni slučajni ključ. "+
            "Sačuvajte ključ, alias i zaporke u sigurnoj kopiji. Bez istog ključa buduće nadogradnje "+
            "postojeće Android aplikacije možda neće biti moguće. Zaporke se ne pohranjuju u projekt."));
        docs.Children.Add(Feature("10 · Google Play kontrolni popis",
            "U Dozvolama navedite HTTPS politiku privatnosti i provjerite Data safety, vlasništvo nad webom, "+
            "target SDK, sadržaj i oglase. Izrada AAB-a ne jamči odobrenje na Google Playu."));
        docs.Children.Add(Feature("11 · Android Mobile klijent",
            "Android klijent služi za pregled i uređivanje konfiguracija, licence, prava i prijenos JSON projekta. "+
            "Izvozne APK/AAB pakete generira Windows Studio. Mobitel ne zahtijeva korisnički račun ni udaljenu bazu."));
        docs.Children.Add(Feature("12 · Podrška i vlasništvo",
            "Službeni web: apk.com.hr · Tehnička podrška: info@apk.com.hr · "+
            "Razvojni autor: Brendigo (brendigo.com). U prijavi poteškoće navedite verziju i "+
            "anonimizirani Gradle zapisnik, bez zaporki i licencnih ključeva."));
        docs.Children.Add(Btn("Kontaktiraj podršku",()=>
            Process.Start(new ProcessStartInfo{
                FileName="mailto:info@apk.com.hr",UseShellExecute=true}),true));
        PageHost.Children.Add(Card(docs,new Thickness(23)));
    }

    private void About()
    {
        Heading("O programu","Brendigo Web2APK Builder");
        var body=Stack();
        var logo=new Border {Background=Accent,CornerRadius=new CornerRadius(16),
            Width=82,Height=82,HorizontalAlignment=HorizontalAlignment.Left,
            Child=new Image {
                Source=new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Assets/Brendigo.ico")),
                Width=70,Height=70
            }};
        body.Children.Add(logo);
        body.Children.Add(Txt("Brendigo Web2APK Builder",24,true,White,
            new Thickness(0,19,0,8)));
        var version=typeof(MainWindow).Assembly.GetName().Version?.ToString(3)??"0.5.0";
        body.Children.Add(Txt("Verzija "+version+" · nativna Windows aplikacija",13,false,Muted));
        body.Children.Add(Divider());
        body.Children.Add(Feature("Razvojni autor","Brendigo · brendigo.com"));
        body.Children.Add(Feature("Službena stranica programa","apk.com.hr"));
        body.Children.Add(Feature("Korisnička i tehnička podrška","info@apk.com.hr"));
        body.Children.Add(Feature("Privatnost i lokalni rad",
            "Projekti i licence spremaju se na računalu. Nema korisničkih računa ni telemetrije. "+
            "Android SDK preuzima se samo uz vašu potvrdu."));
        body.Children.Add(Btn("Posjeti službenu stranicu",()=>
            Process.Start(new ProcessStartInfo {
                FileName="https://apk.com.hr",UseShellExecute=true }),true));
        body.Children.Add(Btn("Piši podršci",()=>
            Process.Start(new ProcessStartInfo {
                FileName="mailto:info@apk.com.hr",UseShellExecute=true })));
        PageHost.Children.Add(Card(body,new Thickness(23)));
    }

}
