namespace MuniChien.App.ViewModels;

public class DashboardCardViewModel
{
    public DashboardCardViewModel(
        string key,
        string title,
        string value)
    {
        Key = key;
        Title = title;
        Value = value;
    }

    public string Key { get; }

    public string Title { get; }

    public string Value { get; }
}