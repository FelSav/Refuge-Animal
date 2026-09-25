using System.IO;
using System.Text.Json;

namespace MuniChien.App.Services;

public class DashboardLayoutService
{
    private readonly string _filePath;

    public DashboardLayoutService()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuniChien");

        Directory.CreateDirectory(appDataPath);

        _filePath = Path.Combine(
            appDataPath,
            "dashboard-layout.json");
    }

    public IReadOnlyList<string> LoadOrder()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            string json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<string>>(json)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void SaveOrder(IEnumerable<string> keys)
    {
        string json = JsonSerializer.Serialize(
            keys,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_filePath, json);
    }
}