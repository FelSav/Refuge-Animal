using MuniChien.App.Navigation;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        _currentViewModel = new HomeViewModel();

        ShowHomeCommand = new RelayCommand(ShowHome);
        ShowSearchCommand = new RelayCommand(ShowSearch);
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;

        set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowHomeCommand { get; }

    public ICommand ShowSearchCommand { get; }

    private void ShowHome()
    {
        CurrentViewModel = new HomeViewModel();
    }

    private void ShowSearch()
    {
        CurrentViewModel = new SearchViewModel();
    }
}