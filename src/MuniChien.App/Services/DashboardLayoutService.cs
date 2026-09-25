using MuniChien.App.ViewModels;
using System.IO;
using System.Text.Json;

namespace MuniChien.App.Services;

public class DashboardLayoutService
{
    private readonly string _filePath;
    private readonly string _customMetricsFilePath;

    public DashboardLayoutService()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MuniChien");

        Directory.CreateDirectory(appDataPath);

        _filePath = Path.Combine(
            appDataPath,
            "dashboard-layout.json");

        _customMetricsFilePath = Path.Combine(
            appDataPath,
            "dashboard-custom-metrics.json");
    }

    public IReadOnlyList<DashboardCardLayoutItem> LoadLayout()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            string json = File.ReadAllText(_filePath);

            // Nouveau format
            List<DashboardCardLayoutItem>? layout =
                JsonSerializer.Deserialize<List<DashboardCardLayoutItem>>(json);

            if (layout is not null)
            {
                return layout;
            }
        }
        catch (JsonException)
        {
            // Le fichier peut encore utiliser l'ancien format.
        }

        try
        {
            string json = File.ReadAllText(_filePath);

            // Ancien format :
            // ["activeDogs", "paymentsToday", ...]
            List<string>? oldOrder =
                JsonSerializer.Deserialize<List<string>>(json);

            if (oldOrder is null)
            {
                return [];
            }

            return oldOrder
                .Select(key => new DashboardCardLayoutItem
                {
                    Key = key,
                    Source = DashboardCardSource.Metric
                })
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public void SaveLayout(
        IEnumerable<DashboardCardViewModel> cards)
    {
        List<DashboardCardLayoutItem> layout =
            cards.Select(card =>
                new DashboardCardLayoutItem
                {
                    Key = card.Key,
                    Source = card.Source,

                    Title = card.IsManual
                        ? card.Title
                        : null,

                    Value = card.IsManual
                        ? card.Value
                        : null
                })
                .ToList();

        string json = JsonSerializer.Serialize(
            layout,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_filePath, json);
    }

    public IReadOnlyList<DashboardCardLayoutItem> LoadCustomMetrics()
    {
        if (!File.Exists(_customMetricsFilePath))
        {
            return [];
        }

        try
        {
            string json =
                File.ReadAllText(_customMetricsFilePath);

            return JsonSerializer
                       .Deserialize<List<DashboardCardLayoutItem>>(json)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void SaveCustomMetrics(
        IEnumerable<DashboardMetricDefinition> metrics)
    {
        List<DashboardCardLayoutItem> customMetrics =
            metrics
                .Where(metric => metric.IsManual)
                .Select(metric =>
                    new DashboardCardLayoutItem
                    {
                        Key = metric.Key,
                        Source = DashboardCardSource.Manual,
                        Title = metric.Title,
                        Value = metric.ManualValue
                    })
                .ToList();

        string json = JsonSerializer.Serialize(
            customMetrics,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(
            _customMetricsFilePath,
            json);
    }
}