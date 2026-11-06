namespace Brendigo.Web2APK.Core;

/// <summary>
/// Technical and publisher-responsibility checks, NOT Google Play approval.
/// Facts reflect ordinary Android mobile app requirements in November 2026.
/// </summary>
public static class GooglePlayReadiness
{
    public sealed record Finding(string Code, string Severity, string Message);
    public static IReadOnlyList<Finding> Review(ProjectDefinition project, bool signedForRelease=false)
    {
        ArgumentNullException.ThrowIfNull(project);
        var results = new List<Finding>();
        void Add(string code,string severity,string message) => results.Add(new(code,severity,message));
        if (project.TargetSdk < 36)
            Add("TARGET_API","FAIL","Od 31. 8. 2026. standardne nove aplikacije i ažuriranja za Google Play moraju ciljati API 36 ili noviji.");
        else Add("TARGET_API","PASS","Android target SDK 36+ zadovoljava trenutačni minimalni zahtjev za standardne mobilne aplikacije.");
        if (project.MinSdk < 24) Add("MIN_API","FAIL","Za ovaj proizvod potreban je min SDK 24 ili noviji.");
        if (project.AllowHttp)
            Add("HTTP","WARN","Omogućen HTTP. Razmotrite HTTPS; osjetljivim podacima ne smije se pristupati nesigurno.");
        if (string.IsNullOrWhiteSpace(project.PrivacyPolicyUrl))
            Add("PRIVACY","WARN","Dodajte javno dostupnu HTTPS politiku privatnosti: poveznica mora biti dostupna u aplikaciji i u Play Consoleu.");
        else Add("PRIVACY","PASS","U konfiguraciji je HTTPS poveznica na politiku privatnosti. Sadržaj i dostupnost treba ručno provjeriti.");
        if (!project.WebsitePublishingPermissionConfirmed)
            Add("OWNERSHIP","WARN","Potvrdite pravo na prikaz web-stranice u Android aplikaciji. Google Play ne dopušta neovlaštene WebView aplikacije.");
        if (!project.DataSafetyReviewed)
            Add("DATA_SAFETY","WARN","Ručno ispunite Play Data safety i deklaracije dozvola, uključujući podatke koje obrađuju SDK-ovi i web-stranica.");
        if (project.AdMobEnabled)
            Add("ADS","WARN","Prijavite oglase u Play Consoleu, podesite AdMob/UMP pristanak, privatnost i Googleove SDK deklaracije.");
        if (project.FirebasePushEnabled)
            Add("PUSH","WARN","Provjerite FCM dostavu, token, obavijesti i Android 13+ korisničku dozvolu.");
        if (project.Camera || project.Microphone || project.Location)
            Add("RUNTIME_PERMISSIONS","WARN","Testirajte svaku osjetljivu dozvolu, opravdanost namjene i ponašanje nakon odbijanja.");
        if (!signedForRelease)
            Add("SIGNATURE","WARN","Za objavu AAB mora biti potpisan trajnim upload ključem. Nepotpisani i debug paketi nisu produkcijski.");
        Add("PAGE_SIZE","MANUAL","Provjerite 16 KB page-size kompatibilnost svih .so biblioteka i usklađenost AAB/APK poravnanja.");
        Add("WEBVIEW_VALUE","MANUAL","Google Play zahtijeva vrijednost i funkcionalnost, ne repetitivan sadržaj ni neovlašten WebView omot.");
        Add("DEVICE_TEST","MANUAL","Provedite test na stvarnom Android 16 uređaju, 16 KB emulatoru te manjim i većim zaslonima.");
        return results;
    }

    public static string AsText(ProjectDefinition project,bool signedForRelease=false)
    {
        var lines=new List<string> {
            "BRENDIGO WEB2APK BUILDER — GOOGLE PLAY PREDOBJAVNA PROVJERA",
            "Stanje: studeni 2026. Ova provjera NE JAMČI odobrenje Google Playa.",
            "Projekt: "+project.Name,
            "Package: "+project.PackageName,
            ""
        };
        lines.AddRange(Review(project,signedForRelease).Select(f=>"["+f.Severity+"] "+f.Code+" — "+f.Message));
        lines.Add("");
        lines.Add("Službeni izvori:");
        lines.Add("https://support.google.com/googleplay/android-developer/answer/11926878");
        lines.Add("https://support.google.com/googleplay/android-developer/answer/10144311");
        lines.Add("https://support.google.com/googleplay/android-developer/answer/9899034");
        lines.Add("https://developer.android.com/guide/practices/page-sizes");
        lines.Add("https://developer.android.com/studio/publish/app-signing");
        return string.Join(Environment.NewLine,lines);
    }
}
