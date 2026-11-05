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
public sealed record BuildResult(string File, bool Signed, string SourceDirectory);

public static class AndroidBuildService
{
    public static async Task<string> ProbeAsync(string executable, params string[] args)
    {
        try
        {
            var output = new StringBuilder();
            var start = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardError = true, RedirectStandardOutput = true };
            start.ArgumentList.Add("/d"); start.ArgumentList.Add("/c"); start.ArgumentList.Add(executable);
            foreach (var a in args) start.ArgumentList.Add(a);
            using var process = new Process { StartInfo = start };
            process.Start();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
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
        if (format is not ("apk" or "aab")) throw new ArgumentException("Nepoznat Android format.");
        project.Validate();
        signing?.Validate();
        Directory.CreateDirectory(destination);
        var work = Path.Combine(Path.GetTempPath(), "Brendigo-Web2APK", project.Id.ToString("N"), Guid.NewGuid().ToString("N"));
        try
        {
            AndroidProjectGenerator.Generate(project, work);
            var task = format == "apk" ? ":app:assembleRelease" : ":app:bundleRelease";
            var start = new ProcessStartInfo("cmd.exe") { WorkingDirectory = work, UseShellExecute = false,
                CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            foreach (var part in new[] { "/d", "/c", "gradle", "--no-daemon", task }) start.ArgumentList.Add(part);
            if (signing is { Enabled: true })
            {
                start.Environment["BRENDIGO_KEYSTORE"] = signing.KeyStore;
                start.Environment["BRENDIGO_STORE_PASSWORD"] = signing.StorePassword;
                start.Environment["BRENDIGO_KEY_ALIAS"] = signing.Alias;
                start.Environment["BRENDIGO_KEY_PASSWORD"] = signing.KeyPassword;
            }
            using var process = new Process { StartInfo = start, EnableRaisingEvents = true };
            process.OutputDataReceived += (_, e) => { if (e.Data != null) log(e.Data + Environment.NewLine); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) log(e.Data + Environment.NewLine); };
            if (!process.Start()) throw new IOException("Gradle se ne može pokrenuti.");
            process.BeginOutputReadLine(); process.BeginErrorReadLine();
            using var registration = cancel.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) {} });
            await process.WaitForExitAsync(cancel);
            if (process.ExitCode != 0) throw new IOException("Gradle izrada nije uspjela (kod " + process.ExitCode + "). Provjerite zapisnik.");
            var artifactDir = Path.Combine(work, "app", "build", "outputs", format == "apk" ? "apk" : "bundle", "release");
            if (!Directory.Exists(artifactDir)) throw new IOException("Gradle nije proizveo očekivanu izlaznu mapu.");
            var artifact = Directory.GetFiles(artifactDir, "*." + format).FirstOrDefault()
                ?? throw new IOException("Nije pronađen " + format.ToUpperInvariant() + " nakon izrade.");
            var target = Path.Combine(destination, project.PackageName + "-" + project.VersionName +
                ((signing is { Enabled: true }) ? "" : "-unsigned") + "." + format);
            File.Copy(artifact, target, overwrite: false);
            return new BuildResult(target, signing is { Enabled: true }, work);
        }
        finally
        {
            try { if (Directory.Exists(work)) Directory.Delete(work, recursive: true); } catch (IOException) { /* leave build log for OS cleanup */ }
        }
    }
}
