using MuniChien.App.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace MuniChien.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly CancellationTokenSource _stop = new();
    private readonly DispatcherTimer _apiRetryTimer = new() { Interval = TimeSpan.FromSeconds(45) };

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        _apiRetryTimer.Tick += async (_, _) => await ProbeIfApiAsync();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _apiRetryTimer.Start();
        await ProbeIfApiAsync();
    }

    private async Task ProbeIfApiAsync()
    {
        if (!_stop.IsCancellationRequested && _viewModel.ApiConnection.IsApiMode &&
            !_viewModel.ApiConnection.IsChecking)
        {
            await _viewModel.ApiConnection.ProbeAsync(_stop.Token);
        }
    }

    private void MainWindow_Activated(object sender, EventArgs e)
    {
        _viewModel.RefreshNotifications();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _apiRetryTimer.Stop();
        _stop.Cancel();
    }
}
