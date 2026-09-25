namespace MuniChien.App.ViewModels;

public class DashboardCardViewModel
{
    public DashboardCardViewModel(string title, string value)
    {
        Title = title;
        Value = value;
    }

    public string Title { get; }

    public string Value { get; }
}