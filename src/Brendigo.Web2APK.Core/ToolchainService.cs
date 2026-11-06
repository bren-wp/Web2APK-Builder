using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>Opt-in verified Gradle installation in the current Windows user's profile.</summary>
public static partial class ToolchainService
{
    public const string GradleVersion = "9.6.1";
    private static readonly string Home = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Brendigo", "Web2APK Builder", "toolchains");
    public static string GradleExecutable => Path.Combine(Home, "gradle-" + GradleVersion, "bin", "gradle.bat");
    public static bool IsGradleInstalled => File.Exists(GradleExecutable);

    public static async Task<string> InstallGradleAsync(Action<string> progress, CancellationToken token)
    {
        if (IsGradleInstalled) return GradleExecutable;
        Directory.CreateDirectory(Home);
        var uri = "https://services.gradle.org/distributions/gradle-" + GradleVersion + "-bin.zip";
        var stage = Path.Combine(Home, "stage-" + Guid.NewGuid().ToString("N"));
        var zip = Path.Combine(stage, "distribution.zip");
        var extract = Path.Combine(stage, "extract");
        Directory.CreateDirectory(stage);
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("BrendigoWeb2APK/0.3.0");
            progress("Preuzimam službeni Gradle SHA-256 kontrolni zbroj...");
            var checksum = (await http.GetStringAsync(uri + ".sha256", token)).Trim().Split(' ', '\n', '\r')[0].ToLowerInvariant();
            if (!Regex.IsMatch(checksum, "^[0-9a-f]{64}$", RegexOptions.CultureInvariant))
                throw new InvalidDataException("Službeni checksum nije valjan.");
            progress("Preuzimam službeni Gradle " + GradleVersion + " (~135 MB)...");
            using (var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token))
            {
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength is > 250_000_000)
                    throw new InvalidDataException("Preuzimanje je veće od očekivane veličine.");
                await using var dest = File.Create(zip);
                await using var source = await response.Content.ReadAsStreamAsync(token);
                var buffer = new byte[131072];
                long size=0;
                while (true)
                {
                    int n=await source.ReadAsync(buffer,token);
                    if(n==0)break;
                    size+=n;
                    if(size>250_000_000) throw new InvalidDataException("Preuzimanje prekoračuje sigurnosnu granicu.");
                    await dest.WriteAsync(buffer.AsMemory(0,n),token);
                }
            }
            progress("Provjeravam SHA-256 potpis preuzete arhive...");
            await using (var file = File.OpenRead(zip))
            {
                var digest = Convert.ToHexString(await SHA256.HashDataAsync(file,token)).ToLowerInvariant();
                if(!string.Equals(digest,checksum,StringComparison.Ordinal))
                    throw new InvalidDataException("SHA-256 provjera nije uspjela. Paket nije instaliran.");
            }
            progress("Raspakiravam verificirani Gradle...");
            ZipFile.ExtractToDirectory(zip,extract);
            var extracted = Path.Combine(extract,"gradle-"+GradleVersion);
            if(!File.Exists(Path.Combine(extracted,"bin","gradle.bat")))
                throw new InvalidDataException("Gradle arhiva nema očekivanu strukturu.");
            var final = Path.Combine(Home,"gradle-"+GradleVersion);
            if(!Directory.Exists(final)) Directory.Move(extracted,final);
            progress("Gradle uspješno instaliran lokalno.");
            return GradleExecutable;
        }
        finally
        {
            try { if (Directory.Exists(stage)) Directory.Delete(stage,true); } catch(IOException) {}
        }
    }
}
