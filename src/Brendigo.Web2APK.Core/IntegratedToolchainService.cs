using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>Opt-in integrated toolchain stored only in the Brendigo user profile.</summary>
public static partial class ToolchainService
{
    public static string LocalJdkHome => Path.Combine(Home,"jdk-17");
    public static string LocalJavaExecutable => Path.Combine(LocalJdkHome,"bin","java.exe");
    public static bool IsLocalJdkInstalled => File.Exists(LocalJavaExecutable);
    public static string LocalSdkRoot => Path.Combine(Home,"android-sdk");
    public static string SdkManagerExecutable => Path.Combine(LocalSdkRoot,"cmdline-tools","latest","bin","sdkmanager.bat");
    public static string LocalAdbExecutable => Path.Combine(LocalSdkRoot,"platform-tools","adb.exe");
    public static bool IsLocalSdkInstalled => File.Exists(SdkManagerExecutable);
    public static bool IsLocalAndroidPlatformInstalled => IsLocalAndroidPlatformInstalledFor(36);
    public static bool IsLocalAndroidPlatformInstalledFor(int api) =>
        (api is 36 or 37) && File.Exists(Path.Combine(LocalSdkRoot,"platforms",
            "android-"+api,"android.jar"));
    public static bool IsLocalAdbInstalled => File.Exists(LocalAdbExecutable);

    public static void ApplyEnvironment(ProcessStartInfo info)
    {
        if(IsLocalJdkInstalled)info.Environment["JAVA_HOME"]=LocalJdkHome;
        if(IsLocalSdkInstalled) {
            info.Environment["ANDROID_HOME"]=LocalSdkRoot;
            info.Environment["ANDROID_SDK_ROOT"]=LocalSdkRoot;
        }
    }

    public static async Task<string> InstallJdkAsync(Action<string> progress,CancellationToken cancel)
    {
        if(IsLocalJdkInstalled)return LocalJdkHome;
        Directory.CreateDirectory(Home);
        var stage=Path.Combine(Home,"jdk-stage-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            using var http=new HttpClient { Timeout=TimeSpan.FromMinutes(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("BrendigoWeb2APK/0.5.0");
            var api="https://api.adoptium.net/v3/assets/latest/17/hotspot?architecture=x64&image_type=jdk&os=windows&vendor=eclipse";
            progress("Tražim službenu Eclipse Temurin 17 Windows x64 arhivu...");
            using var response=await http.GetAsync(api,cancel);
            response.EnsureSuccessStatusCode();
            await using var data=await response.Content.ReadAsStreamAsync(cancel);
            using var json=await JsonDocument.ParseAsync(data,cancellationToken:cancel);
            var package=json.RootElement[0].GetProperty("binary").GetProperty("package");
            var link=package.GetProperty("link").GetString()??
                throw new InvalidDataException("JDK nema URL.");
            var checksum=package.GetProperty("checksum").GetString()??
                throw new InvalidDataException("JDK nema SHA-256.");
            var url=new Uri(link);
            if(url.Scheme!=Uri.UriSchemeHttps ||
                !(url.Host.Equals("github.com",StringComparison.OrdinalIgnoreCase) ||
                  url.Host.EndsWith(".github.com",StringComparison.OrdinalIgnoreCase) ||
                  url.Host.Equals("api.adoptium.net",StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("Neočekivani JDK izvor.");
            var zip=Path.Combine(stage,"temurin.zip");
            await DownloadCheckedAsync(http,url,checksum,zip,420_000_000,progress,cancel);
            progress("Raspakiravam verificirani JDK...");
            var extract=Path.Combine(stage,"extract");
            ZipFile.ExtractToDirectory(zip,extract);
            var java=Directory.EnumerateFiles(extract,"java.exe",SearchOption.AllDirectories)
                .FirstOrDefault(p=>p.EndsWith(Path.Combine("bin","java.exe"),StringComparison.OrdinalIgnoreCase));
            if(java is null)throw new InvalidDataException("U JDK arhivi nedostaje java.exe.");
            var jdk=Directory.GetParent(Directory.GetParent(java)!.FullName)!.FullName;
            if(Directory.Exists(LocalJdkHome))Directory.Delete(LocalJdkHome,true);
            Directory.Move(jdk,LocalJdkHome);
            progress("Temurin JDK 17 spreman bez zasebne instalacije.");
            return LocalJdkHome;
        }
        finally { try{Directory.Delete(stage,true);}catch(IOException){} }
    }

    private const string AndroidToolsUrl=
        "https://dl.google.com/android/repository/commandlinetools-win-15859902_latest.zip";
    // Official Android developer checksum for the Windows command-line tools archive.
    private const string AndroidToolsSha256=
        "90ae805d20434428bffcb699c290860f19bb5f66a67e6b330067e3de801fb04a";

    /// <summary>Call only after explicit user agreement with Android SDK license terms.</summary>
    public static async Task<string> InstallAndroidSdkAsync(Action<string> progress,CancellationToken cancel,int targetSdk=36)
    {
        if(targetSdk is not (36 or 37))
            throw new ArgumentException("Podržane su platforme Android SDK 36 i 37.");
        if(!IsLocalJdkInstalled && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAVA_HOME")))
            throw new InvalidOperationException("Najprije instalirajte JDK 17 kroz Studio.");
        Directory.CreateDirectory(Home);
        var stage=Path.Combine(Home,"sdk-stage-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            if(!IsLocalSdkInstalled)
            {
                using var http=new HttpClient { Timeout=TimeSpan.FromMinutes(30) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("BrendigoWeb2APK/0.5.0");
                var zip=Path.Combine(stage,"android-commandline.zip");
                progress("Preuzimam službene Android SDK command-line alate...");
                await DownloadCheckedAsync(http,new Uri(AndroidToolsUrl),AndroidToolsSha256,
                    zip,240_000_000,progress,cancel);
                var extract=Path.Combine(stage,"extract");
                ZipFile.ExtractToDirectory(zip,extract);
                var source=Path.Combine(extract,"cmdline-tools");
                if(!File.Exists(Path.Combine(source,"bin","sdkmanager.bat")))
                    throw new InvalidDataException("Android SDK arhiva nema ispravnu strukturu.");
                var target=Path.Combine(LocalSdkRoot,"cmdline-tools","latest");
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if(!Directory.Exists(target))Directory.Move(source,target);
            }
            progress("Prihvaćam Android SDK licence prema korisnikovoj izričitoj potvrdi...");
            await RunSdkManagerAsync(["--licenses"],true,progress,cancel);
            progress("Instaliram Android SDK "+targetSdk+", build-tools i lokalni ADB...");
            await RunSdkManagerAsync(
                ["platform-tools","platforms;android-"+targetSdk,"build-tools;36.0.0"],
                false,progress,cancel);
            if(!IsLocalAndroidPlatformInstalledFor(targetSdk)||!IsLocalAdbInstalled)
                throw new IOException("Android SDK platforma ili ADB nisu instalirani.");
            progress("Android SDK je spreman unutar Brendigo Studija.");
            return LocalSdkRoot;
        }
        finally { try{Directory.Delete(stage,true);}catch(IOException){} }
    }

    private static async Task RunSdkManagerAsync(string[] arguments,bool accept,
        Action<string> progress,CancellationToken cancel)
    {
        var start=new ProcessStartInfo("cmd.exe") {
            UseShellExecute=false,CreateNoWindow=true,
            RedirectStandardInput=accept,RedirectStandardOutput=true,
            RedirectStandardError=true
        };
        start.ArgumentList.Add("/d");
        start.ArgumentList.Add("/c");
        start.ArgumentList.Add(SdkManagerExecutable);
        start.ArgumentList.Add("--sdk_root="+LocalSdkRoot);
        foreach(var arg in arguments)start.ArgumentList.Add(arg);
        ApplyEnvironment(start);
        using var child=new Process { StartInfo=start };
        if(!child.Start())throw new IOException("SDK Manager se ne može pokrenuti.");
        var output=child.StandardOutput.ReadToEndAsync(cancel);
        var errors=child.StandardError.ReadToEndAsync(cancel);
        if(accept) {
            for(var i=0;i<40;i++)await child.StandardInput.WriteLineAsync("y".AsMemory(),cancel);
            child.StandardInput.Close();
        }
        using var registration=cancel.Register(()=>{
            try{if(!child.HasExited)child.Kill(true);}catch(InvalidOperationException){}
        });
        await child.WaitForExitAsync(cancel);
        var outText=await output;
        var errText=await errors;
        if(child.ExitCode!=0)
            throw new IOException("Android SDK Manager nije uspio (kod "+child.ExitCode+"): "+
                (errText.Length>0?errText:outText)[..Math.Min(400,(errText.Length>0?errText:outText).Length)]);
        progress("SDK Manager uspješno je završio zadatak.");
    }

    private static async Task DownloadCheckedAsync(HttpClient http,Uri url,string checksum,
        string filename,long limit,Action<string> progress,CancellationToken cancel)
    {
        if(!Regex.IsMatch(checksum,"^[0-9a-fA-F]{64}$",RegexOptions.CultureInvariant))
            throw new InvalidDataException("Neispravan SHA-256.");
        using var response=await http.GetAsync(url,HttpCompletionOption.ResponseHeadersRead,cancel);
        response.EnsureSuccessStatusCode();
        if(response.Content.Headers.ContentLength>limit)
            throw new InvalidDataException("Arhiva prelazi sigurnosnu granicu.");
        await using(var target=File.Create(filename))
        await using(var stream=await response.Content.ReadAsStreamAsync(cancel)) {
            var buffer=new byte[131072];
            long transferred=0;
            int n;
            while((n=await stream.ReadAsync(buffer,cancel))>0) {
                transferred+=n;
                if(transferred>limit)throw new InvalidDataException("Prevelika preuzeta arhiva.");
                await target.WriteAsync(buffer.AsMemory(0,n),cancel);
            }
        }
        await using var file=File.OpenRead(filename);
        var sum=Convert.ToHexString(await SHA256.HashDataAsync(file,cancel));
        if(!sum.Equals(checksum,StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("SHA-256 provjera preuzimanja nije uspjela.");
        progress("SHA-256 je potvrđen.");
    }
}
