using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>Feature permissions are enforced before export and build, not merely in the UI.</summary>
public static class LicenseEntitlements
{
    public const string Demo = "demo";
    public const string Basic = "basic";
    public const string Professional = "pro";
    public const string Developer = "developer";
    public const string Agency = "agency";
    // Package IDs are bound on the first actual export/build for new signed licenses.
    public const string AutomaticScope = "@";

    public static bool ValidTier(string? tier) => tier is Demo or Basic or Professional or Agency or Developer;
    private static bool SinglePackage(string name) =>
        name.Length is > 3 and <= 180 &&
        Regex.IsMatch(name,@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$",
            RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1));

    public static bool ValidPackage(string tier,string? package)
    {
        if(tier is Developer or Agency)return package=="*";
        if(package is null)return false;
        if(package==AutomaticScope)return tier is Demo or Basic or Professional;
        if(tier!=Professional)return SinglePackage(package);
        var ids=package.Split(',',StringSplitOptions.None);
        return ids.Length is >=1 and <=3 && ids.All(SinglePackage) &&
            ids.Distinct(StringComparer.Ordinal).Count()==ids.Length;
    }
    public static bool PackageMatches(BrendigoLicense.Claims claim,string package) =>
        claim.Tier is Developer or Agency ||
        (claim.Tier==Professional
            ? claim.PackageName.Split(',').Contains(package,StringComparer.Ordinal)
            : string.Equals(claim.PackageName,package,StringComparison.Ordinal));
    public static int ApplicationLimit(string tier) => tier switch
    {
        Developer or Agency=>int.MaxValue,Professional=>3,_=>1
    };
    public static int AgencyPriceEur(int months) => months switch
    {
        1=>39,12=>390,_=>throw new ArgumentException("Agencija vrijedi 1 ili 12 mjeseci.")
    };

    public static string DisplayName(string tier) => tier switch
    {
        Demo => "Demo",
        Basic => "Osnovno",
        Professional => "Profesionalac · tri aplikacije",
        Agency => "Agencija · pretplata",
        Developer => "Developer · vlasnik",
        _ => "Nepoznat paket"
    };

    public static int PriceEur(string tier) => tier switch
    {
        Demo => 0, Basic => 49, Professional => 69, Agency=>39, Developer => 0,
        _ => throw new ArgumentException("Nepoznat paket.")
    };

    public static bool Allows(BrendigoLicense.Claims c, string feature) => feature switch
    {
        "apk" => ValidTier(c.Tier),
        "aab" or "source" or "branding" or "custom_icon" or "splash" => c.Tier is Basic or Professional or Agency or Developer,
        "pull_refresh" or "firebase_push" => c.Tier is Professional or Agency or Developer,
        "admob" or "camera" or "microphone" or "location" or "advanced" => c.Tier is Professional or Agency or Developer,
        "unlimited_projects" or "resale" => c.Tier is Agency or Developer,
        _ => false
    };

    public static bool Check(BrendigoLicense.Claims c, ProjectDefinition project,
        string operation, out string reason)
    {
        reason="";
        if(c.Tier==Agency && c.ExpiresAt<=DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            reason="Agencijska pretplata nije aktivna. Obnovite licencu prije novog izvoza.";
            return false;
        }
        if(!PackageMatches(c,project.PackageName))
        {
            reason="Licenca "+DisplayName(c.Tier)+" vrijedi isključivo za "+c.PackageName+
                ". Profesionalac vrijedi za najviše tri unaprijed navedena package ID-ja.";
            return false;
        }
        if(!Allows(c,operation))
        {
            reason="Paket "+DisplayName(c.Tier)+" ne podržava "+operation.ToUpperInvariant()+
                ". Odaberite odgovarajući paket licence.";
            return false;
        }
        if(project.AdMobEnabled && !Allows(c,"admob"))
            reason="AdMob je dostupan od paketa Profesionalac.";
        else if(project.FirebasePushEnabled && !Allows(c,"firebase_push"))
            reason="Firebase Push dostupan je od paketa Profesionalac.";
        else if(project.PullRefresh && !Allows(c,"pull_refresh"))
            reason="Povucite za osvježavanje dostupno je od paketa Profesionalac.";
        else if(!string.IsNullOrWhiteSpace(project.IconPath) && !Allows(c,"custom_icon"))
            reason="Vlastita ikona dostupna je od paketa Osnovno.";
        else if((project.Camera || project.Microphone || project.Location) && !Allows(c,"advanced"))
            reason="Kamera, mikrofon i lokacija dostupni su od paketa Profesionalac.";
        else if(c.Tier==Demo && (project.Theme!="dark" ||
            !string.Equals(project.Accent,"#7253FF",StringComparison.OrdinalIgnoreCase) ||
            project.SplashDurationMs!=700 || !project.Splash || !project.BrendigoSplashBranding))
            reason="Demo koristi zadane boje i splash. Prilagođeni izgled dostupan je od paketa Osnovno.";
        if(reason.Length>0)return false;
        return true;
    }
}
