using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Brendigo.Web2APK.Core;

/// <summary>Signed BL2 entitlements. No issuing secret is included in clients.</summary>
public static class BrendigoLicense
{
    public const string PublicKeyBase64 = "MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAlz/Lh4EZM5h29mbVGielXikIkmh0qrlCVnhwhOodnSSo+AP4KrDCCTFC0kxhJ85a9jaV3i5vHrRXHfV1BnE7pl0yLv43AiwCKIp6kblBp8mg94LtGvxWdC/fsQymdINRI0hi5ACADRTHnjka3PXUUJS7bLVysbSEaOUSCQ7PNJZFhIYfcuddOMz018BEDxMnINtkR0+M0WaKRSDYuHKD0M88Yl4ezztpExMMRg2KmlYBB+Rk5hHyGFXhq8rMhlImI9MoNZwrf+7zM+b/JzElvtZM1ptEENEs8fS+9uBz9fSeZiGo3uYl3FXOrVdsS4UzNxUyYc7L2H0EqTMMBeCsY8Kvo+3Y4fCl3lsYLolWcRj2NUsQey9UYWgOhZnFHZRhsH7YkFSEUaDVZdrcUn/k6s0vgFd9rSFJAnmDEjpJRFznOdSPAupn375Rd98vjkDYlDUoAGsA3q6UEU9+KYuIeVzTPKAwBp/rWnO+sB8IY8o2WOnRax7GiJAdiTvMux/NAgMBAAE=";
    public const string Prefix = "BL2";

    public sealed record Claims(string Serial, string Audience, long IssuedAt, long ExpiresAt,
        string Tier, string PackageName);

    public static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
    public static byte[] Decode(string value)
    {
        if(value.Length is < 1 or > 2048 ||
            !Regex.IsMatch(value,@"^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            throw new FormatException("Neispravan format licence.");
        var result=value.Replace('-','+').Replace('_','/');
        return Convert.FromBase64String(result.PadRight((result.Length+3)/4*4,'='));
    }

    public static string Issue(RSA issuer, string serial, string audience, long issuedAt, long expiresAt,
        string tier = "developer", string packageName = "*")
    {
        if(!OwnerMatches(issuer)) throw new CryptographicException("Nepoznat vlasnički potpisni ključ.");
        if(!Regex.IsMatch(serial,@"^[0-9a-f]{32}$",RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1)) ||
            audience is not ("both" or "windows" or "android") ||
            issuedAt <= 0 ||
            (tier==LicenseEntitlements.Agency
                ? expiresAt<=issuedAt || expiresAt>issuedAt+370L*86400L
                : expiresAt!=0) || !LicenseEntitlements.ValidTier(tier) ||
            !LicenseEntitlements.ValidPackage(tier,packageName))
            throw new ArgumentException("Neispravan paket, package ID, trajanje ili serijski broj.");
        var payload=Encoding.UTF8.GetBytes(
            "{\"v\":2,\"s\":\""+serial+"\",\"a\":\""+audience+"\",\"t\":\""+tier+
            "\",\"p\":\""+packageName+"\",\"iat\":"+issuedAt.ToString(CultureInfo.InvariantCulture)+
            ",\"exp\":"+expiresAt.ToString(CultureInfo.InvariantCulture)+"}");
        var signature=issuer.SignData(payload,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        return Prefix+"."+Encode(payload)+"."+Encode(signature);
    }

    public static bool TryVerify(string? license, string platform, out Claims? claims)
    {
        claims=null;
        if (string.IsNullOrWhiteSpace(license) || license.Length>4096 ||
            platform is not ("windows" or "android")) return false;
        try
        {
            var parts=license.Trim().Split('.');
            if(parts.Length!=3 || parts[0]!=Prefix) return false;
            var payload=Decode(parts[1]);
            var signature=Decode(parts[2]);
            if(payload.Length>1024 || signature.Length!=384) return false;
            using var rsa=RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKeyBase64),out _);
            if(!rsa.VerifyData(payload,signature,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1))return false;
            using var doc=JsonDocument.Parse(payload);
            var root=doc.RootElement;
            if(root.GetProperty("v").GetInt32()!=2)return false;
            var serial=root.GetProperty("s").GetString();
            var audience=root.GetProperty("a").GetString();
            var tier=root.GetProperty("t").GetString();
            var package=root.GetProperty("p").GetString();
            var issued=root.GetProperty("iat").GetInt64();
            var expires=root.GetProperty("exp").GetInt64();
            var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if(serial is null || !Regex.IsMatch(serial,@"^[0-9a-f]{32}$",RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1)) ||
                audience is not ("both" or "windows" or "android") ||
                tier is null || package is null || !LicenseEntitlements.ValidTier(tier) ||
                !LicenseEntitlements.ValidPackage(tier,package) ||
                (tier==LicenseEntitlements.Agency
                    ? expires<=issued || expires>issued+370L*86400L || expires<=now
                    : expires!=0) || issued<=0 || issued>now+300 ||
                (audience!="both" && audience!=platform))return false;
            claims=new(serial,audience,issued,expires,tier,package);
            return true;
        }
        catch(Exception e) when(e is FormatException or CryptographicException or JsonException
            or InvalidOperationException or KeyNotFoundException or ArgumentException or OverflowException)
        {return false;}
    }

    public static bool OwnerMatches(RSA issuer)
    {
        var expected=Convert.FromBase64String(PublicKeyBase64);
        return CryptographicOperations.FixedTimeEquals(expected,issuer.ExportSubjectPublicKeyInfo());
    }
}
