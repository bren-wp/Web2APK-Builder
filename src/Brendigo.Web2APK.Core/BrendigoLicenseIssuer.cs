using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Brendigo.Web2APK.Core;

/// <summary>Owner-only issuer; streams licenses so batch size is not tied to RAM.</summary>
public static class BrendigoLicenseIssuer
{
    public static RSA OpenIssuer(string encryptedPemPath, string passphrase)
    {
        if (string.IsNullOrWhiteSpace(passphrase) || !File.Exists(encryptedPemPath) ||
            new FileInfo(encryptedPemPath).Length > 32_768)
            throw new ArgumentException("Odaberite valjani šifrirani privatni ključ i lozinku.");
        var rsa=RSA.Create();
        try {
            rsa.ImportFromEncryptedPem(File.ReadAllText(encryptedPemPath),passphrase);
            if (!BrendigoLicense.OwnerMatches(rsa))
                throw new CryptographicException("Privatni ključ ne pripada ovom izdanju Brendigo aplikacije.");
            return rsa;
        } catch { rsa.Dispose(); throw; }
    }

    public static async Task<string> ExportCsvAsync(RSA issuer, string outputFile,long count,
        string audience, string tier, string packageName, Action<long,long>? progress,
        CancellationToken cancellationToken)
    {
        if(!BrendigoLicense.OwnerMatches(issuer))throw new CryptographicException("Nepoznat potpisni ključ.");
        if(count<1 || audience is not ("both" or "windows" or "android") ||
            !LicenseEntitlements.ValidTier(tier) || !LicenseEntitlements.ValidPackage(tier,packageName))
            throw new ArgumentException("Neispravna količina, izdanje ili trajanje.");
        var output=Path.GetFullPath(outputFile);
        var folder=Path.GetDirectoryName(output)!;
        Directory.CreateDirectory(folder);
        var drive=new DriveInfo(Path.GetPathRoot(output)!);
        // Conservative estimate of ~770 UTF-8 bytes per license + CSV metadata.
        var estimated=count > long.MaxValue/800 ? long.MaxValue : count*950;
        if(drive.IsReady && estimated>drive.AvailableFreeSpace)
            throw new IOException("Nema dovoljno prostora za cijeli izvoz. Potreban slobodan prostor: ~"+
                Math.Ceiling(estimated/1e9).ToString("0.0",CultureInfo.InvariantCulture)+" GB.");
        if(File.Exists(output))throw new IOException("Odaberite novi naziv datoteke (postojeća neće biti prebrisana).");
        var temp=output+"."+Guid.NewGuid().ToString("N")+".partial";
        var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        const long exp=0;
        var seed=RandomNumberGenerator.GetBytes(8);
        try {
            {
            await using var file=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None,
                1024*1024,FileOptions.Asynchronous|FileOptions.SequentialScan);
            await using var writer=new StreamWriter(file,new UTF8Encoding(false),1024*1024,leaveOpen:true);
            await writer.WriteLineAsync("SERIAL;PAKET;PACKAGE_ID;PLATFORMA;CIJENA_EUR;LICENCA");
            for(long i=0;i<count;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                byte[] id=new byte[16];
                Array.Copy(seed,id,seed.Length);
                BinaryPrimitives.WriteUInt64BigEndian(id.AsSpan(8),checked((ulong)i));
                var serial=Convert.ToHexStringLower(id);
                var token=BrendigoLicense.Issue(issuer,serial,audience,now,exp,tier,packageName);
                await writer.WriteLineAsync(serial+";"+tier+";"+packageName+";"+audience+";"+LicenseEntitlements.PriceEur(tier)+";"+token);
                if(i%100==0 || i==count-1) progress?.Invoke(i+1,count);
            }
            await writer.FlushAsync(cancellationToken);
            file.Flush(true);
            }
            File.Move(temp,output);
            progress?.Invoke(count,count);
            return output;
        }
        finally {
            if(File.Exists(temp))File.Delete(temp);
        }
    }
}
