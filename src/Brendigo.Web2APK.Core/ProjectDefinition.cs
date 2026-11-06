using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

public sealed class ProjectDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Moja web aplikacija";
    public string PackageName { get; set; } = "com.brendigo.mojaaplikacija";
    public string SourceType { get; set; } = "url";
    public string Url { get; set; } = "https://example.com";
    public string LocalPath { get; set; } = "";
    public string VersionName { get; set; } = "1.0.0";
    public int VersionCode { get; set; } = 1;
    public int MinSdk { get; set; } = 24;
    public int TargetSdk { get; set; } = 36;
    public string Orientation { get; set; } = "portrait";
    public string Theme { get; set; } = "dark";
    public string Accent { get; set; } = "#7253FF";
    public bool JavaScript { get; set; } = true;
    public bool DomStorage { get; set; } = true;
    public bool PullRefresh { get; set; }
    public bool AllowHttp { get; set; }
    public bool OpenExternal { get; set; } = true;
    public bool Camera { get; set; }
    public bool Microphone { get; set; }
    public bool Location { get; set; }
    public bool FileUploads { get; set; } = true;
    public bool Downloads { get; set; } = true;
    public string IconPath { get; set; } = "";
    public int SplashDurationMs { get; set; } = 700;
    public bool AdMobEnabled { get; set; }
    public string AdMobAppId { get; set; } = "";
    public string AdMobBannerId { get; set; } = "";
    public bool FirebasePushEnabled { get; set; }
    public string FirebaseConfigPath { get; set; } = "";

    public bool StatusBar { get; set; } = true;
    public bool Splash { get; set; } = true;
    public bool Fullscreen { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ProjectDefinition Copy() => (ProjectDefinition)MemberwiseClone();
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length < 2 || Name.Length > 70)
            throw new ArgumentException("Naziv mora sadržavati 2–70 znakova.");
        if (!Regex.IsMatch(PackageName, @"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))
            || PackageName.Length > 180)
            throw new ArgumentException("Package ID mora biti oblika com.tvrtka.aplikacija (mala slova).");
        if (!Regex.IsMatch(VersionName, @"^[0-9]+\.[0-9]+(\.[0-9]+)?([.-][a-zA-Z0-9]+)?$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))
            || VersionName.Length > 48)
            throw new ArgumentException("Neispravan naziv verzije.");
        if (VersionCode is < 1 or > 2_099_999_999) throw new ArgumentException("Version code mora biti pozitivan.");
        if (MinSdk is < 24 or > 36 || TargetSdk is < 36 or > 37 || TargetSdk < MinSdk)
            throw new ArgumentException("Podržani su min SDK 24–36 i target SDK 36–37.");
        if (!new[] { "portrait", "landscape", "unspecified" }.Contains(Orientation))
            throw new ArgumentException("Nepoznata orijentacija.");
        if (!new[] { "dark", "light" }.Contains(Theme)) throw new ArgumentException("Nepoznata tema.");
        if (!Regex.IsMatch(Accent, "^#[0-9a-fA-F]{6}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            throw new ArgumentException("Primarna boja mora biti #RRGGBB.");
        if (SplashDurationMs is < 0 or > 3000)
            throw new ArgumentException("Splash trajanje mora biti između 0 i 3000 ms.");
        if (AdMobEnabled)
        {
            if (!Regex.IsMatch(AdMobAppId, @"^ca-app-pub-[0-9]{16}~[0-9]{10}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)) ||
                !Regex.IsMatch(AdMobBannerId, @"^ca-app-pub-[0-9]{16}/[0-9]{10}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                throw new ArgumentException("Za AdMob su potrebni ispravni App ID (~) i Banner Ad Unit ID (/).");
        }
        if (FirebasePushEnabled)
        {
            if (FirebaseConfigPath.Length is 0 or > 1500 || !Path.IsPathFullyQualified(FirebaseConfigPath) ||
                !File.Exists(FirebaseConfigPath) || new FileInfo(FirebaseConfigPath).Length > 2_000_000)
                throw new ArgumentException("Firebase Push zahtijeva lokalnu google-services.json datoteku.");
            try
            {
                using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(FirebaseConfigPath));
                var clients = json.RootElement.GetProperty("client");
                bool match=false;
                foreach (var client in clients.EnumerateArray())
                {
                    if (client.GetProperty("client_info").GetProperty("android_client_info")
                        .GetProperty("package_name").GetString() == PackageName) match=true;
                }
                if (!match) throw new ArgumentException("Firebase google-services.json nema Android klijenta za ovaj package ID.");
            }
            catch (System.Text.Json.JsonException ex)
            { throw new ArgumentException("Firebase konfiguracija nije valjani JSON.",ex); }
            catch (KeyNotFoundException ex)
            { throw new ArgumentException("Firebase konfiguracija nema očekivana polja.",ex); }
        }
        if (IconPath.Length > 0)
        {
            if (IconPath.Length > 1500 || !Path.IsPathFullyQualified(IconPath) || !File.Exists(IconPath))
                throw new ArgumentException("Odabrana PNG ikona ne postoji.");
            var icon = new FileInfo(IconPath);
            if (icon.Length is < 24 or > 5_000_000)
                throw new ArgumentException("Launcher PNG mora imati veličinu od 24 bajta do 5 MB.");
            using var stream = File.OpenRead(IconPath);
            Span<byte> header = stackalloc byte[24];
            stream.ReadExactly(header);
            if (!header[..8].SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}) || 
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header[16..20]) < 192 ||
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header[20..24]) < 192)
                throw new ArgumentException("Launcher ikonica mora biti valjani PNG minimalno 192 × 192 px.");
        }
        if (SourceType == "url")
        {
            if (Url.Length > 2000 || !Uri.TryCreate(Url, UriKind.Absolute, out var uri)
                || uri.UserInfo.Length > 0 || (uri.Scheme != Uri.UriSchemeHttps && !(AllowHttp && uri.Scheme == Uri.UriSchemeHttp))
                || string.IsNullOrWhiteSpace(uri.Host))
                throw new ArgumentException("Potrebna je valjana HTTPS adresa (HTTP samo uz izričitu dozvolu).");
        }
        else if (SourceType == "local")
        {
            if (LocalPath.Length > 1500 || !Path.IsPathFullyQualified(LocalPath) || !File.Exists(Path.Combine(LocalPath, "index.html")))
                throw new ArgumentException("Lokalna mapa mora sadržavati index.html.");
            var sourceDir = new DirectoryInfo(LocalPath);
            if (sourceDir.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new ArgumentException("Simboličke poveznice nisu dopuštene za lokalnu HTML mapu.");
        }
        else throw new ArgumentException("Nepoznat izvor aplikacije.");
    }
}
