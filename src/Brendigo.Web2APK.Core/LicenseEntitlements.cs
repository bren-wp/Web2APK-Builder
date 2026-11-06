using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>Feature permissions are enforced before export and build, not merely in the UI.</summary>
public static class LicenseEntitlements
{
    public const string Demo = "demo";
    public const string Basic = "basic";
    public const string Professional = "pro";
    public const string Developer = "developer";

    public static bool ValidTier(string? tier) => tier is Demo or Basic or Professional or Developer;
    public static bool ValidPackage(string tier,string? package)
    {
        if(tier==Developer)return package=="*";
        return package is {Length: <= 180} &&
            Regex.IsMatch(package,@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$",
                RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1));
    }

    public static string DisplayName(string tier) => tier switch
    {
        Demo => "Demo",
        Basic => "Osnovno",
        Professional => "Profesionalac",
        Developer => "Developer · vlasnik",
        _ => "Nepoznat paket"
    };

    public static int PriceEur(string tier) => tier switch
    {
        Demo => 0, Basic => 49, Professional => 69, Developer => 0,
        _ => throw new ArgumentException("Nepoznat paket.")
    };

    public static bool Allows(BrendigoLicense.Claims c, string feature) => feature switch
    {
        "apk" => ValidTier(c.Tier),
        "aab" or "source" or "branding" or "custom_icon" or "splash" => c.Tier is Basic or Professional or Developer,
        "pull_refresh" or "firebase_push" => c.Tier is Professional or Developer,
        "admob" or "camera" or "microphone" or "location" or "advanced" => c.Tier==Developer,
        "unlimited_projects" => c.Tier==Developer,
        _ => false
    };

    public static bool Check(BrendigoLicense.Claims c, ProjectDefinition project,
        string operation, out string reason)
    {
        reason="";
        if(c.Tier!=Developer && !string.Equals(c.PackageName,project.PackageName,StringComparison.Ordinal))
        {
            reason="Licenca "+DisplayName(c.Tier)+" vrijedi isključivo za "+c.PackageName+
                ". Za novu aplikaciju unesite licencu za njezin package ID.";
            return false;
        }
        if(!Allows(c,operation))
        {
            reason="Paket "+DisplayName(c.Tier)+" ne podržava "+operation.ToUpperInvariant()+
                ". Odaberite odgovarajući paket licence.";
            return false;
        }
        if(project.AdMobEnabled && !Allows(c,"admob"))
            reason="AdMob je dostupan samo uz Developer licencu.";
        else if(project.FirebasePushEnabled && !Allows(c,"firebase_push"))
            reason="Firebase Push dostupan je u paketu Profesionalac ili Developer.";
        else if(project.PullRefresh && !Allows(c,"pull_refresh"))
            reason="Povucite za osvježavanje dostupno je u paketu Profesionalac ili Developer.";
        else if(!string.IsNullOrWhiteSpace(project.IconPath) && !Allows(c,"custom_icon"))
            reason="Vlastita ikona dostupna je od paketa Osnovno.";
        else if((project.Camera || project.Microphone || project.Location) && c.Tier!=Developer)
            reason="Kamera, mikrofon i lokacija dostupni su u Developer paketu.";
        else if(c.Tier==Demo && (project.Theme!="dark" ||
            !string.Equals(project.Accent,"#7253FF",StringComparison.OrdinalIgnoreCase) ||
            project.SplashDurationMs!=700 || !project.Splash))
            reason="Demo koristi zadane boje i splash. Prilagođeni izgled dostupan je od paketa Osnovno.";
        if(reason.Length>0)return false;
        return true;
    }
}
