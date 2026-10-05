namespace MuniChien.App.ViewModels;

public class DashboardMetricDefinition
{
    public DashboardMetricDefinition(
        string key,
        string title,
        DashboardCardSource source = DashboardCardSource.Metric,
        string? manualValue = null)
    {
        Key = key;
        Title = title;
        Source = source;
        ManualValue = manualValue;
    }

    public string Key { get; }

    public string Title { get; }

    public DashboardCardSource Source { get; }

    public string? ManualValue { get; }

    public bool IsManual =>
        Source == DashboardCardSource.Manual;
}