using System.IO;
using System.Text.Json;

namespace AudioSwitcher;

public sealed class SettingsStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AudioSwitcher", "settings.json");

    public SettingsData Load()
    {
        try
        {
            return !File.Exists(_path) 
                ? new SettingsData() 
                : JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path)) ?? new SettingsData();
        }
        catch
        {
            return new SettingsData();
        }
    }

    public void Save(SettingsData settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public sealed class SettingsData
{
    public List<string> FavoriteDeviceIds { get; set; } = new();
}
