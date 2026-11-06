using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Brendigo.Web2APK.Core;

/// <summary>
/// Local JSON project database. Writes are serialized across running Builder processes,
/// replaced atomically and keep the previous valid version as a recovery backup.
/// </summary>
public sealed class ProjectStore
{
    private const long MaxDatabaseBytes = 8L * 1024 * 1024;
    private const int MaxProjects = 4000;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true, PropertyNameCaseInsensitive = true
    };
    private readonly string _path;
    private readonly string _mutexName;

    public ProjectStore(string directory)
    {
        Directory.CreateDirectory(directory);
        _path = Path.GetFullPath(Path.Combine(directory,"projects.json"));
        var hash=SHA256.HashData(Encoding.UTF8.GetBytes(
            OperatingSystem.IsWindows() ? _path.ToUpperInvariant() : _path));
        // The Local namespace shares the same lock between all processes in this user session.
        _mutexName=(OperatingSystem.IsWindows() ? @"Local\" : "")+
            "Brendigo-Web2APK-Projects-"+Convert.ToHexString(hash);
    }

    public IReadOnlyList<ProjectDefinition> List()
    {
        if(!File.Exists(_path))return [];
        try
        {
            var bytes=new FileInfo(_path).Length;
            if(bytes is < 2 or > MaxDatabaseBytes)
                throw new InvalidDataException("Datoteka projekata premašuje dopuštenu veličinu od 8 MB ili je prazna.");
            var values=JsonSerializer.Deserialize<List<ProjectDefinition>>(
                File.ReadAllText(_path,Encoding.UTF8),Options)
                ?? throw new InvalidDataException("Datoteka projekata je prazna.");
            if(values.Count>MaxProjects ||
                values.Any(p=>p is null || p.Id==Guid.Empty ||
                    string.IsNullOrWhiteSpace(p.Name)) ||
                values.Select(p=>p.Id).Distinct().Count()!=values.Count)
                throw new InvalidDataException("Datoteka sadrži nevaljane ili duplicirane projekte.");
            return values.OrderByDescending(p=>p.UpdatedAt).ToArray();
        }
        catch(Exception e) when(e is JsonException or InvalidDataException)
        {
            // Never silently reset a damaged database. The previous snapshot is at .bak.
            throw new InvalidDataException(
                "Datoteka projekata nije ispravna. Sačuvajte projects.json i projects.json.bak prije popravka. "+
                e.Message,e);
        }
    }

    public ProjectDefinition Save(ProjectDefinition input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        if(input.Id==Guid.Empty)throw new ArgumentException("Projekt mora imati valjan identifikator.");
        return WithWriteLock(()=>
        {
            var values=List().ToList();
            var found=values.FindIndex(p=>p.Id==input.Id);
            var now=DateTime.UtcNow;
            var copy=input.Copy();
            copy.Name=copy.Name.Trim();
            copy.UpdatedAt=now;
            if(found<0)
            {
                if(values.Count>=MaxProjects)
                    throw new IOException("Dosegnuto je ograničenje broja lokalnih projekata.");
                copy.CreatedAt=now;
                values.Add(copy);
            }
            else
            {
                copy.CreatedAt=values[found].CreatedAt;
                values[found]=copy;
            }
            Write(values);
            return copy.Copy();
        });
    }

    public void Delete(Guid id) => WithWriteLock(()=>
    {
        var values=List().Where(p=>p.Id!=id).ToList();
        Write(values);
        return true;
    });

    private T WithWriteLock<T>(Func<T> action)
    {
        using var mutex=new Mutex(false,_mutexName);
        bool owns=false;
        try
        {
            try {owns=mutex.WaitOne(TimeSpan.FromSeconds(20));}
            catch(AbandonedMutexException){owns=true;}
            if(!owns)throw new IOException(
                "Spremanje je zauzeto u drugoj instanci aplikacije. Pokušajte ponovno.");
            return action();
        }
        finally {if(owns)mutex.ReleaseMutex();}
    }

    private void Write(List<ProjectDefinition> values)
    {
        var json=JsonSerializer.Serialize(values,Options);
        if(Encoding.UTF8.GetByteCount(json)>MaxDatabaseBytes)
            throw new IOException("Datoteka projekata premašila bi dopuštenu veličinu od 8 MB.");
        var temp=_path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            File.WriteAllText(temp,json,new UTF8Encoding(false));
            if(File.Exists(_path))
                File.Replace(temp,_path,_path+".bak",ignoreMetadataErrors:true);
            else File.Move(temp,_path);
        }
        finally {if(File.Exists(temp))File.Delete(temp);}
    }
}
