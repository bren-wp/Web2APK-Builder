using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>
/// Signed BL2 automatic licenses bind package IDs at first export/build.
/// Windows DPAPI prevents undetected local edits but cannot prevent deletion,
/// rollback or offline reuse on another computer. A server is needed for that.
/// </summary>
public static class AutomaticLicenseSeats
{
    private sealed class Ledger
    {
        public string Serial { get; set; } = "";
        public List<string> Packages { get; set; } = [];
    }

    private static string DefaultFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Brendigo","Web2APK Builder","seat-bindings");

    private static bool ValidId(string value) =>
        value.Length is > 3 and <= 180 &&
        Regex.IsMatch(value,@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$",
            RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1));

    public static bool TryUse(BrendigoLicense.Claims claim,string packageId,bool reserve,
        out int used,out string reason,string? directory=null) =>
        Inspect(claim,packageId,reserve,out used,out reason,directory);

    public static bool Usage(BrendigoLicense.Claims claim,out int used,out string reason,
        string? directory=null) =>
        Inspect(claim,null,false,out used,out reason,directory);

    /// <summary>Read-only view of exact bound package IDs, without consuming a seat.</summary>
    public static bool ReadBindings(BrendigoLicense.Claims claim,
        out IReadOnlyList<string> packageIds,out string reason,string? directory=null)
    {
        var result=new List<string>();
        bool valid=Inspect(claim,null,false,out _,out reason,directory,result);
        packageIds=valid?result.AsReadOnly():Array.Empty<string>();
        return valid;
    }

    private static bool Inspect(BrendigoLicense.Claims claim,string? packageId,bool reserve,
        out int used,out string reason,string? directory,List<string>? bindings=null)
    {
        used=0;reason="";
        if(claim.PackageName!=LicenseEntitlements.AutomaticScope ||
            claim.Tier is not (LicenseEntitlements.Demo or LicenseEntitlements.Basic or LicenseEntitlements.Professional) ||
            (packageId is not null && !ValidId(packageId)))
        {
            reason="Automatski licencni opseg ili package ID nije valjan.";
            return false;
        }
        if(!OperatingSystem.IsWindows())
        {
            reason="Lokalno povezivanje mjesta zahtijeva Windows DPAPI.";
            return false;
        }

        var serialHash=SHA256.HashData(Encoding.UTF8.GetBytes(claim.Serial));
        var folder=directory ?? DefaultFolder;
        var path=Path.Combine(folder,Convert.ToHexStringLower(serialHash)+".dpapi");
        using var mutex=new Mutex(false,@"Local\Brendigo-Web2APK-Seats-"+Convert.ToHexString(serialHash));
        bool acquired=false;
        try
        {
            try { acquired=mutex.WaitOne(TimeSpan.FromSeconds(15)); }
            catch(AbandonedMutexException) { acquired=true; }
            if(!acquired)
            {
                reason="Evidencija licencnih mjesta trenutno je zauzeta.";
                return false;
            }
            var ledger=new Ledger {Serial=claim.Serial};
            if(File.Exists(path))
            {
                if(new FileInfo(path).Length is < 1 or > 65536)
                    throw new InvalidDataException("Evidencija licencnih mjesta ima nevaljanu veličinu.");
                var decrypted=ProtectedData.Unprotect(File.ReadAllBytes(path),serialHash,
                    DataProtectionScope.CurrentUser);
                ledger=JsonSerializer.Deserialize<Ledger>(decrypted)
                    ?? throw new InvalidDataException("Evidencija licencnih mjesta je prazna.");
                if(ledger.Serial!=claim.Serial || ledger.Packages is null ||
                    ledger.Packages.Count>LicenseEntitlements.ApplicationLimit(claim.Tier) ||
                    ledger.Packages.Any(p=>p is null || !ValidId(p)) ||
                    ledger.Packages.Distinct(StringComparer.Ordinal).Count()!=ledger.Packages.Count)
                    throw new InvalidDataException("Evidencija licencnih mjesta nije valjana.");
            }
            used=ledger.Packages.Count;
            bindings?.AddRange(ledger.Packages);
            if(packageId is null || ledger.Packages.Contains(packageId,StringComparer.Ordinal))
                return true;
            if(used>=LicenseEntitlements.ApplicationLimit(claim.Tier))
            {
                reason="Sva licencna mjesta su iskorištena ("+used+"/"+LicenseEntitlements.ApplicationLimit(claim.Tier)+
                    "). Brisanje projekta ne oslobađa prethodno zauzeto mjesto.";
                return false;
            }
            if(!reserve)return true;
            Directory.CreateDirectory(folder);
            ledger.Packages.Add(packageId);
            var protectedBytes=ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(ledger),
                serialHash,DataProtectionScope.CurrentUser);
            var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
            try { File.WriteAllBytes(temp,protectedBytes); File.Move(temp,path,overwrite:true); }
            finally { if(File.Exists(temp))File.Delete(temp); }
            used=ledger.Packages.Count;
            return true;
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or
            CryptographicException or JsonException or InvalidDataException or
            PlatformNotSupportedException or ArgumentException)
        {
            reason="Evidencija licencnih mjesta nije dostupna. Podaci se ne poništavaju automatski. "+
                ex.Message;
            return false;
        }
        finally { if(acquired)mutex.ReleaseMutex(); }
    }
}
