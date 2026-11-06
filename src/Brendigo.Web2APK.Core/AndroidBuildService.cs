using System.Diagnostics;
using System.Text;

namespace Brendigo.Web2APK.Core;

public sealed record SigningOptions(string KeyStore, string Alias, string StorePassword, string KeyPassword)
{
    public bool Enabled => !string.IsNullOrWhiteSpace(KeyStore);
    public void Validate()
    {
        if (!Enabled) return;
        if (!File.Exists(KeyStore) || string.IsNullOrWhiteSpace(Alias) ||
            string.IsNullOrEmpty(StorePassword) || string.IsNullOrEmpty(KeyPassword))
            throw new ArgumentException("Za potpisivanje odaberi keystore, alias i obje lozinke.");
    }
}
public sealed record BuildResult(string File, bool Signed, string SourceDirectory, bool Debug=false);

public static class AndroidBuildService
{
    private static readonly string[] SigningKeys =
    [
        "BRENDIGO_KEYSTORE", "BRENDIGO_STORE_PASSWORD",
        "BRENDIGO_KEY_ALIAS", "BRENDIGO_KEY_PASSWORD"
    ];

    // Do not allow a user or parent process environment to silently sign an
    // artifact explicitly requested as unsigned/debug.
    public static void ConfigureSigningEnvironment(ProcessStartInfo info, SigningOptions? signing)
    {
        foreach (var key in SigningKeys) info.Environment.Remove(key);
        if (signing is not { Enabled: true }) return;
        info.Environment["BRENDIGO_KEYSTORE"] = signing.KeyStore;
        info.Environment["BRENDIGO_STORE_PASSWORD"] = signing.StorePassword;
        info.Environment["BRENDIGO_KEY_ALIAS"] = signing.Alias;
        info.Environment["BRENDIGO_KEY_PASSWORD"] = signing.KeyPassword;
    }

    // Gradle plugins can echo environment values in failure output; never show
    // the signing passwords in the in-app build log.
    public static string RedactSigningSecrets(string line, SigningOptions? signing)
    {
        if (signing is not { Enabled: true }) return line;
        foreach (var secret in new[] { signing.StorePassword, signing.KeyPassword }
            .Where(value => !string.IsNullOrEmpty(value))
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(value => value.Length))
            line = line.Replace(secret, "[skriveno]", StringComparison.Ordinal);
        return line;
    }

    public static async Task<string> ProbeAsync(string executable, params string[] args)
    {
        try
        {
            var output = new StringBuilder();
            var managed = executable.Equals("gradle",StringComparison.OrdinalIgnoreCase) &&
                ToolchainService.IsGradleInstalled;
            var javaPath = ToolchainService.IsLocalJdkInstalled ?
                ToolchainService.LocalJdkHome : Environment.GetEnvironmentVariable("JAVA_HOME");
            var javaExe = !string.IsNullOrWhiteSpace(javaPath) &&
                File.Exists(Path.Combine(javaPath,"bin","java.exe")) ?
                Path.Combine(javaPath,"bin","java.exe") : "java";
            var ownJava=executable.Equals("java",StringComparison.OrdinalIgnoreCase) &&
                ToolchainService.IsLocalJdkInstalled;
            var ownAdb=executable.Equals("adb",StringComparison.OrdinalIgnoreCase) &&
                ToolchainService.IsLocalAdbInstalled;
            var start = new ProcessStartInfo(managed?javaExe:
                ownJava?ToolchainService.LocalJavaExecutable:
                ownAdb?ToolchainService.LocalAdbExecutable:"cmd.exe") {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardError = true, RedirectStandardOutput = true };
            if (managed)
            {
                start.ArgumentList.Add("-cp");
                start.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ToolchainService.GradleExecutable)!)!,
                    "lib","gradle-launcher-"+ToolchainService.GradleVersion+".jar"));
                start.ArgumentList.Add("org.gradle.launcher.GradleMain");
            }
            else if(!ownJava&&!ownAdb) {
                start.ArgumentList.Add("/d"); start.ArgumentList.Add("/c"); start.ArgumentList.Add(executable);
            }
            ToolchainService.ApplyEnvironment(start);
            foreach (var a in args) start.ArgumentList.Add(a);
            using var process = new Process { StartInfo = start };
            process.Start();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            using var killOnTimeout = cts.Token.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { }
            });
            var std = process.StandardOutput.ReadToEndAsync(cts.Token);
            var err = process.StandardError.ReadToEndAsync(cts.Token);
            await process.WaitForExitAsync(cts.Token);
            output.Append(await std); output.Append(await err);
            return output.ToString().Trim() is { Length: > 0 } s ? s[..Math.Min(s.Length, 1200)] : "(nema izlaza)";
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or OperationCanceledException or IOException)
        { return "Alat nije dostupan: " + executable + " (" + e.Message + ")"; }
    }

    public static async Task<BuildResult> BuildAsync(ProjectDefinition project, string destination, string format,
        SigningOptions? signing, Action<string> log, CancellationToken cancel)
    {
        if (format is not ("apk" or "aab" or "apk-debug" or "apk-unsigned"))
            throw new ArgumentException("Nepoznat Android format.");
        var isDebug=format=="apk-debug";
        var explicitUnsigned=format=="apk-unsigned";
        if ((isDebug || explicitUnsigned) && signing is { Enabled: true })
            throw new ArgumentException("Debug/unsigned izvoz ne prihvaća produkcijski potpisni ključ.");
        var licenseAction=format=="aab"?"aab":"apk";
        project.Validate();
        LicenseActivationStore.Require("windows",project,licenseAction);
        signing?.Validate();
        Directory.CreateDirectory(destination);
        var work = Path.Combine(Path.GetTempPath(), "Brendigo-Web2APK", project.Id.ToString("N"), Guid.NewGuid().ToString("N"));
        try
        {
            AndroidProjectGenerator.Generate(project, work);
            var task = format=="aab"?":app:bundleRelease":
                isDebug?":app:assembleDebug":":app:assembleRelease";
            var managed = ToolchainService.IsGradleInstalled;
            var jdk = ToolchainService.IsLocalJdkInstalled ?
                ToolchainService.LocalJdkHome : Environment.GetEnvironmentVariable("JAVA_HOME");
            var javaExecutable = !string.IsNullOrWhiteSpace(jdk) && File.Exists(Path.Combine(jdk,"bin","java.exe"))
                ? Path.Combine(jdk,"bin","java.exe") : "java";
            var start = new ProcessStartInfo(managed ? javaExecutable : "cmd.exe") {
                WorkingDirectory = work, UseShellExecute = false,
                CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            if (managed)
            {
                var lib = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ToolchainService.GradleExecutable)!)!,
                    "lib","gradle-launcher-"+ToolchainService.GradleVersion+".jar");
                start.ArgumentList.Add("-cp"); start.ArgumentList.Add(lib);
                start.ArgumentList.Add("org.gradle.launcher.GradleMain");
                start.ArgumentList.Add("--no-daemon"); start.ArgumentList.Add(task);
            }
            else foreach (var part in new[] { "/d", "/c", "gradle", "--no-daemon", task })
                start.ArgumentList.Add(part);
            ToolchainService.ApplyEnvironment(start);
            ConfigureSigningEnvironment(start, signing);
            using var process = new Process { StartInfo = start, EnableRaisingEvents = true };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null) log(RedactSigningSecrets(e.Data, signing) + Environment.NewLine);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null) log(RedactSigningSecrets(e.Data, signing) + Environment.NewLine);
            };
            if (!process.Start()) throw new IOException("Gradle se ne može pokrenuti.");
            process.BeginOutputReadLine(); process.BeginErrorReadLine();
            using var registration = cancel.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) {} });
            await process.WaitForExitAsync(cancel);
            if (process.ExitCode != 0) throw new IOException("Gradle izrada nije uspjela (kod " + process.ExitCode + "). Provjerite zapisnik.");
            var extension=licenseAction;
            var artifactDir=Path.Combine(work,"app","build","outputs",
                extension=="apk"?"apk":"bundle",isDebug?"debug":"release");
            if(!Directory.Exists(artifactDir))
                throw new IOException("Gradle nije proizveo očekivanu izlaznu mapu.");
            var artifact=Directory.GetFiles(artifactDir,"*."+extension).FirstOrDefault()
                ?? throw new IOException("Nije pronađen "+extension.ToUpperInvariant()+" nakon izrade.");
            var suffix=isDebug?"-debug":signing is { Enabled:true }?"-signed":"-unsigned";
            var target=Path.Combine(destination,project.PackageName+"-"+project.VersionName+suffix+"."+extension);
            File.Copy(artifact,target,overwrite:false);
            return new BuildResult(target,isDebug || signing is { Enabled:true },
                "Privremena mapa briše se po završetku izrade.",isDebug);
        }
        finally
        {
            try { if (Directory.Exists(work)) Directory.Delete(work, recursive: true); } catch (IOException) { /* leave build log for OS cleanup */ }
        }
    }
}
