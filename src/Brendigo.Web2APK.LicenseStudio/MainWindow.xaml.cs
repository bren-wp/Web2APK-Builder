using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendigo.Web2APK.LicenseStudio;

public partial class MainWindow : Window
{
    private string _audience="both";
    private string _tier="demo";
    private CancellationTokenSource? _cancel;
    public MainWindow()
    {
        InitializeComponent();
#if UI_CAPTURE
        if(Environment.GetCommandLineArgs().Contains("--capture-native"))
            Loaded += CaptureNativeStudioOnLoaded;
#endif
    }
    private void BrowseKey_Click(object sender,RoutedEventArgs e)
    {
        var d=new OpenFileDialog {Title="Brendigo vlasnički šifrirani ključ",
            Filter="Šifrirani PEM (*.pem)|*.pem",CheckFileExists=true};
        if(d.ShowDialog(this)==true) KeyPath.Text=d.FileName;
    }
    private void SelectAudience(string value)
    {
        _audience=value;
        foreach(var pair in new[]{(BothButton,"both"),(WindowsButton,"windows"),(AndroidButton,"android")})
            pair.Item1.Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                pair.Item2==value?"#6650D6":"#24365F"));
    }
    private void Both_Click(object s,RoutedEventArgs e)=>SelectAudience("both");
    private void Windows_Click(object s,RoutedEventArgs e)=>SelectAudience("windows");
    private void Android_Click(object s,RoutedEventArgs e)=>SelectAudience("android");
    private void SelectTier(string tier)
    {
        _tier=tier;
        foreach(var item in new[]{(DemoButton,"demo"),(BasicButton,"basic"),
            (ProButton,"pro"),(AgencyButton,"agency"),(DeveloperButton,"developer")})
            item.Item1.Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                item.Item2==tier?"#6650D6":"#24365F"));
        AgencyDuration.IsEnabled=tier==LicenseEntitlements.Agency;
        ScopeHint.Text=tier switch
        {
            LicenseEntitlements.Professional =>
                "Do tri različite aplikacije. Package ID povezuje se pri prvom izvozu ili izradi.",
            LicenseEntitlements.Agency or LicenseEntitlements.Developer =>
                "Neograničen broj aplikacija. Nije potrebno unositi package ID.",
            _ => "Jedna aplikacija. Package ID povezuje se pri prvom izvozu ili izradi."
        };
        if(tier is LicenseEntitlements.Agency or LicenseEntitlements.Developer)
        {
            SelectAudience("both");
            WindowsButton.IsEnabled=false;
            AndroidButton.IsEnabled=false;
        }
        else
        {
            WindowsButton.IsEnabled=true;
            AndroidButton.IsEnabled=true;
        }
        TierDetails.Text=tier switch
        {
            "demo"=>"Demo · 0 € · APK bez reklama · jedna aplikacija · doživotno.",
            "basic"=>"Osnovno · 49 € · APK/AAB, vlastita ikona, splash i brending · jedna aplikacija · doživotno.",
            "pro"=>"Profesionalac · 69 € · tri aplikacije, AdMob, napredne dozvole, Push, osvježavanje i izvorni kod · doživotno.",
            "agency"=>"Agencija · 39 €/mj. ili 390 €/god. · sve opcije, neograničeno klijentskih aplikacija i komercijalna isporuka.",
            _=>"Developer · samo vlasnik · neograničene aplikacije i sve funkcije."
        };
    }
    private void Demo_Click(object s,RoutedEventArgs e)=>SelectTier("demo");
    private void Basic_Click(object s,RoutedEventArgs e)=>SelectTier("basic");
    private void Pro_Click(object s,RoutedEventArgs e)=>SelectTier("pro");
    private void Agency_Click(object s,RoutedEventArgs e)=>SelectTier("agency");
    private void Developer_Click(object s,RoutedEventArgs e)=>SelectTier("developer");
    private void Cancel_Click(object s,RoutedEventArgs e)=>_cancel?.Cancel();
    private async void Export_Click(object sender,RoutedEventArgs e)
    {
        if(_cancel is not null)return;
        if(!long.TryParse(CountBox.Text,NumberStyles.None,CultureInfo.InvariantCulture,out var count)||count<1)
        { MessageBox.Show(this,"Unesite pozitivnu količinu licenci.",
            "Provjerite unos",MessageBoxButton.OK,MessageBoxImage.Warning);return; }
        if(string.IsNullOrWhiteSpace(KeyPath.Text) || OwnerPassword.Password.Length<1)
        { MessageBox.Show(this,"Odaberite šifrirani vlasnički ključ i njegovu lozinku.",
            "Vlasnički ključ",MessageBoxButton.OK,MessageBoxImage.Warning);return; }
        var dialog=new SaveFileDialog {Title="Spremi izvezene licence",DefaultExt=".csv",
            Filter="CSV dokument (*.csv)|*.csv",FileName="Brendigo-Licence-"+DateTime.Now.ToString("yyyyMMdd-HHmm")+".csv",
            OverwritePrompt=true};
        if(dialog.ShowDialog(this)!=true)return;
        if(File.Exists(dialog.FileName))
        { MessageBox.Show(this,"Zbog sigurnosti se postojeća datoteka ne prepisuje. Odaberite novi naziv.",
            "Zaštita izvoza",MessageBoxButton.OK,MessageBoxImage.Warning);return; }
        // Large batches use a streaming writer; no list of tokens is ever held in memory.
        _cancel=new CancellationTokenSource();
        ExportButton.IsEnabled=false;CancelButton.IsEnabled=true;
        var password=OwnerPassword.Password;
        OwnerPassword.Clear();
        var filename=dialog.FileName;
        var keyPath=KeyPath.Text;
        var audience=_audience;
        var tier=_tier;
        var package=tier is LicenseEntitlements.Agency or LicenseEntitlements.Developer
            ? "*" : LicenseEntitlements.AutomaticScope;
        var agencyMonths=AgencyDuration.SelectedIndex==1?12:1;
        try {
            StatusText.Text="Otvaram vlasnički ključ…";
            using var rsa=await Task.Run(()=>BrendigoLicenseIssuer.OpenIssuer(keyPath,password),_cancel.Token);
            StatusText.Text="Izdavanje licenci…";
            var progress=new Progress<(long done,long total)>(p=>{
                Progress.Value=(double)p.done/p.total*100;
                StatusText.Text=$"Izdano {p.done:N0} / {p.total:N0} licenci";
            });
            var output=await Task.Run(()=>BrendigoLicenseIssuer.ExportCsvAsync(
                rsa,filename,count,audience,tier,package,
                (done,total)=>((IProgress<(long,long)>)progress).Report((done,total)),
                _cancel.Token,agencyMonths),_cancel.Token);
            Progress.Value=100;
            StatusText.Text="Gotovo: "+count.ToString("N0")+" · "+LicenseEntitlements.DisplayName(tier);
            MessageBox.Show(this,"Licence su izvezene u:\n"+output+"\n\nPrivatni ključ ostao je izvan aplikacije za korisnike.",
                "Izvoz dovršen",MessageBoxButton.OK,MessageBoxImage.Information);
        } catch(OperationCanceledException) {StatusText.Text="Izvoz je prekinut. Privremena datoteka uklonjena."; }
        catch(Exception ex) {
            StatusText.Text="Izvoz nije uspio.";
            MessageBox.Show(this,ex.Message,"Pogreška izdavanja",MessageBoxButton.OK,MessageBoxImage.Error);
        } finally {
            _cancel.Dispose();_cancel=null;
            ExportButton.IsEnabled=true;CancelButton.IsEnabled=false;
        }
    }
}
