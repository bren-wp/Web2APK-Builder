using System.Security.Cryptography;
using System.Text;

namespace Brendigo.Web2APK.Core;

/// <summary>Portfolio of signed offline licenses. Stored tokens are not secret.</summary>
public static class LicenseActivationStore
{
    private static string Folder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Brendigo","Web2APK Builder","licenses");

    // Older single-license path is read for compatibility, but only valid BL2 signatures activate.
    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Brendigo","Web2APK Builder","activation.license");

    private static IEnumerable<string> Tokens()
    {
        if(File.Exists(FilePath))
        {
            string? legacy=Read(FilePath);
            if(legacy is not null) yield return legacy;
        }
        if(!Directory.Exists(Folder))yield break;
        foreach(var path in Directory.EnumerateFiles(Folder,"*.license"))
        {
            var token=Read(path);
            if(token is not null)yield return token;
        }
    }
    private static string? Read(string path)
    {
        try {
            if(new FileInfo(path).Length is < 1 or > 4096) return null;
            return File.ReadAllText(path);
        } catch(IOException) {return null;}
        catch(UnauthorizedAccessException) {return null;}
    }
    public sealed record VisibleLicense(BrendigoLicense.Claims Claims,string MaskedToken);
    public static string MaskToken(string token) {
        if(string.IsNullOrWhiteSpace(token))return "********";
        var cleaned=token.Trim();
        return cleaned.Length<=18 ? "********" : cleaned[..Math.Min(10,cleaned.Length)]+
            "***************"+cleaned[^Math.Min(8,cleaned.Length)..];
    }
    public static IReadOnlyList<VisibleLicense> InstalledMasked(string platform) {
        if(platform=="windows" && !WindowsEdition.LicenseRequired)
            return Array.Empty<VisibleLicense>();
        var result=new List<VisibleLicense>();
        foreach(var token in Tokens())
            if(BrendigoLicense.TryVerify(token,platform,out var claim) && claim is not null)
                result.Add(new VisibleLicense(claim,MaskToken(token)));
        return result;
    }
    private static IEnumerable<string> LicensePaths() {
        if(File.Exists(FilePath))yield return FilePath;
        if(Directory.Exists(Folder))
            foreach(var path in Directory.EnumerateFiles(Folder,"*.license"))
                yield return path;
    }
    public static bool Remove(string serial,string platform) {
        bool removed=false;
        foreach(var path in LicensePaths()) {
            var token=Read(path);
            if(token is null || !BrendigoLicense.TryVerify(token,platform,out var c) ||
                c?.Serial!=serial)continue;
            File.Delete(path);
            removed=true;
        }
        return removed;
    }
    public static bool Replace(string oldSerial,string newToken,string platform) {
        if(!BrendigoLicense.TryVerify(newToken,platform,out var newClaim) || newClaim is null)
            return false;
        if(!Installed(platform).Any(c=>c.Serial==oldSerial))return false;
        if(!Activate(newToken,platform))return false;
        if(oldSerial!=newClaim.Serial)Remove(oldSerial,platform);
        return true;
    }

    public static bool IsActivated(string platform)
    {
        if(platform=="windows" && !WindowsEdition.LicenseRequired)return true;
        foreach(var token in Tokens())
            if(BrendigoLicense.TryVerify(token,platform,out _))return true;
        return false;
    }
    public static IReadOnlyList<BrendigoLicense.Claims> Installed(string platform)
    {
        if(platform=="windows" && !WindowsEdition.LicenseRequired)
            return [WindowsEdition.Capabilities];
        var entries=new List<BrendigoLicense.Claims>();
        foreach(var token in Tokens())
            if(BrendigoLicense.TryVerify(token,platform,out var license) && license is not null)
                entries.Add(license);
        return entries;
    }
    public static bool Activate(string token,string platform)
    {
        if(!BrendigoLicense.TryVerify(token,platform,out var claims) || claims is null)return false;
        Directory.CreateDirectory(Folder);
        var id=Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(claims.Serial)));
        var target=Path.Combine(Folder,id+".license");
        var temp=target+"."+Guid.NewGuid().ToString("N")+".tmp";
        try {
            File.WriteAllText(temp,token.Trim());
            File.Move(temp,target,true);
        } finally {if(File.Exists(temp))File.Delete(temp);}
        return true;
    }

    /// <summary>Preview effective license scope without reserving a seat.</summary>
    public static bool ScopeMatches(BrendigoLicense.Claims claim,string packageId) =>
        claim.PackageName==LicenseEntitlements.AutomaticScope
            ? AutomaticLicenseSeats.TryUse(claim,packageId,false,out _,out _)
            : LicenseEntitlements.PackageMatches(claim,packageId);

    public static bool TryAuthorize(string platform,ProjectDefinition project,string action,
        out BrendigoLicense.Claims? matched,out string reason)
    {
        if(platform=="windows" && !WindowsEdition.LicenseRequired)
        {
            matched=WindowsEdition.Capabilities;
            var allowed=LicenseEntitlements.Check(matched,project,action,out reason);
            if(!allowed)matched=null;
            return allowed;
        }
        matched=null;
        reason="Nije instalirana odgovarajuća licenca za ovu Android aplikaciju.";
        foreach(var c in Installed(platform).OrderByDescending(c=>c.Tier switch
        {
            LicenseEntitlements.Developer=>5,LicenseEntitlements.Agency=>4,
            LicenseEntitlements.Professional=>3,
            LicenseEntitlements.Basic=>2,_=>1
        }))
        {
            if(c.PackageName==LicenseEntitlements.AutomaticScope)
            {
                if(!AutomaticLicenseSeats.TryUse(c,project.PackageName,false,out _,out var seatReason))
                {
                    reason=seatReason;continue;
                }
                // Signed automatic scope is checked against the locally recorded package binding.
                var effective=c with {PackageName=project.PackageName};
                if(LicenseEntitlements.Check(effective,project,action,out var problem))
                {
                    matched=c;reason="";return true;
                }
                reason=problem;
            }
            else if(LicenseEntitlements.Check(c,project,action,out var problem))
            {
                matched=c;reason="";return true;
            }
            else reason=problem;
        }
        return false;
    }

    public static void Require(string platform,ProjectDefinition project,string action)
    {
        if(!TryAuthorize(platform,project,action,out var claim,out var reason))
            throw new UnauthorizedAccessException(reason);
        if(platform=="windows" && !WindowsEdition.LicenseRequired)return;
        if(claim is {PackageName: LicenseEntitlements.AutomaticScope} &&
            !AutomaticLicenseSeats.TryUse(claim,project.PackageName,true,out _,out reason))
            throw new UnauthorizedAccessException(reason);
    }
}
