using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Brendigo.Web2APK.Core;

/// <summary>Creates a reusable local Android signing key. Secrets are never sent to a server.</summary>
public static class LocalSigningKeyService
{
    public sealed record GeneratedKey(string KeyStorePath, string Alias, string Password);

    public static async Task<GeneratedKey> GenerateAsync(string folder, CancellationToken token = default)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "brendigo-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") +
            "-" + Guid.NewGuid().ToString("N")[..8] + ".p12");
        const string alias = "brendigo";
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        var executable = !string.IsNullOrWhiteSpace(javaHome) &&
            File.Exists(Path.Combine(javaHome, "bin", "keytool.exe")) ?
            Path.Combine(javaHome,"bin","keytool.exe") : "keytool.exe";

        var psi = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardError = true, RedirectStandardOutput = true
        };
        foreach (var argument in new [] { "-genkeypair", "-keyalg", "RSA", "-keysize", "3072",
            "-sigalg","SHA256withRSA", "-validity", "10000",
            "-keystore",path,"-storetype","PKCS12","-alias",alias,
            "-storepass:env","BRENDIGO_GENERATED_STOREPASS",
            "-keypass:env","BRENDIGO_GENERATED_STOREPASS",
            "-dname","CN=Brendigo Web2APK, OU=Android Signing, O=Brendigo, C=HR",
            "-noprompt" })
            psi.ArgumentList.Add(argument);
        psi.Environment["BRENDIGO_GENERATED_STOREPASS"] = password;
        try
        {
            using var process=new Process { StartInfo=psi };
            process.Start();
            using var register=token.Register(() => { try { if(!process.HasExited)process.Kill(true); } catch(InvalidOperationException){} });
            var err=process.StandardError.ReadToEndAsync(token);
            var stdout=process.StandardOutput.ReadToEndAsync(token);
            await process.WaitForExitAsync(token);
            var failure=await err;
            _ = await stdout;
            if (process.ExitCode!=0 || !File.Exists(path))
                throw new IOException("Lokalni keytool nije izradio ključ: "+failure[..Math.Min(failure.Length,600)]);
            return new GeneratedKey(path,alias,password);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }
}
