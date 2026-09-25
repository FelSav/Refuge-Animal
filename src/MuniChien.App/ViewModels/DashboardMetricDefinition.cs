namespace MuniChien.App.ViewModels;

public class DashboardMetricDefinition
{
    public DashboardMetricDefinition(string key, string title)
    {
        Key = key;
        Title = title;
    }

    public string Key { get; }

    public string Title { get; }
}