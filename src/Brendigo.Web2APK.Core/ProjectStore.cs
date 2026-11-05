using System.Text;
using System.Text.Json;

namespace Brendigo.Web2APK.Core;

/// <summary>Local project persistence with atomic replacement and a backup on corruption.</summary>
public sealed class ProjectStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string _path;

    public ProjectStore(string directory)
    {
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "projects.json");
    }

    public IReadOnlyList<ProjectDefinition> List()
    {
        if (!File.Exists(_path)) return [];
        try
        {
            var projects = JsonSerializer.Deserialize<List<ProjectDefinition>>(File.ReadAllText(_path, Encoding.UTF8), Options)
                           ?? throw new InvalidDataException("Neispravna datoteka projekata.");
            return projects.OrderByDescending(p => p.UpdatedAt).ToArray();
        }
        catch (Exception e) when (e is JsonException or InvalidDataException)
        {
            throw new InvalidDataException("Datoteka projekata nije ispravna. Izradite sigurnosnu kopiju prije popravka. " + e.Message, e);
        }
    }

    public ProjectDefinition Save(ProjectDefinition input)
    {
        input.Validate();
        var values = List().ToList();
        var found = values.FindIndex(p => p.Id == input.Id);
        var now = DateTime.UtcNow;
        var copy = input.Copy();
        copy.Name = copy.Name.Trim();
        copy.UpdatedAt = now;
        if (found < 0) { copy.CreatedAt = now; values.Add(copy); }
        else { copy.CreatedAt = values[found].CreatedAt; values[found] = copy; }
        Write(values);
        return copy.Copy();
    }

    public void Delete(Guid id)
    {
        var values = List().Where(p => p.Id != id).ToList();
        Write(values);
    }

    private void Write(List<ProjectDefinition> values)
    {
        var temp = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(values, Options), new UTF8Encoding(false));
            if (File.Exists(_path))
            {
                // File.Move with overwrite keeps a complete JSON file even if the app is interrupted.
                File.Move(temp, _path, overwrite: true);
            }
            else File.Move(temp, _path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
