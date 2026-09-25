namespace MuniChien.App.ViewModels;

public class DashboardCardViewModel : ViewModelBase
{
    private string _title;
    private string _value;

    public DashboardCardViewModel(
        string key,
        string title,
        string value,
        DashboardCardSource source = DashboardCardSource.Metric)
    {
        Key = key;
        _title = title;
        _value = value;
        Source = source;
    }

    public string Key { get; }

    public DashboardCardSource Source { get; }

    public bool IsManual => Source == DashboardCardSource.Manual;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }
}