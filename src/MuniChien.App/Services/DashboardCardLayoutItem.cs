using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public class DashboardCardLayoutItem
{
    public string Key { get; set; } = string.Empty;

    public DashboardCardSource Source { get; set; }

    public string? Title { get; set; }

    public string? Value { get; set; }
}