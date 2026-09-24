using MuniChien.App.Navigation;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    private string _pageTitle = "Accueil";

    public string PageTitle
    {
        get => _pageTitle;

        private set
        {
            _pageTitle = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        _currentViewModel = new HomeViewModel();

        ShowHomeCommand = new RelayCommand(ShowHome);
        ShowSearchCommand = new RelayCommand(ShowSearch);
        ShowLicensesCommand = new RelayCommand(ShowLicenses);
        ShowPaymentsCommand = new RelayCommand(ShowPayments);
        ShowReportsCommand = new RelayCommand(ShowReports);
        ShowAdministrationCommand = new RelayCommand(ShowAdministration);
        ShowOwnersCommand = new RelayCommand(ShowOwners);
        ShowDogsCommand = new RelayCommand(ShowDogs);
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
    public ICommand ShowLicensesCommand { get; }
    public ICommand ShowPaymentsCommand { get; }
    public ICommand ShowReportsCommand { get; }
    public ICommand ShowAdministrationCommand { get; }
    public ICommand ShowOwnersCommand { get; }
    public ICommand ShowDogsCommand { get; }

    private void ShowHome()
    {
        CurrentViewModel = new HomeViewModel();
        PageTitle = "Accueil";
    }

    private void ShowSearch()
    {
        CurrentViewModel = new SearchViewModel();
        PageTitle = "Recherche";
    }

    private void ShowOwners()
    {
        CurrentViewModel = new OwnersViewModel();
        PageTitle = "Propriétaires";
    }

    private void ShowDogs()
    {
        CurrentViewModel = new DogsViewModel();
        PageTitle = "Chiens";
    }

    private void ShowLicenses()
    {
        CurrentViewModel = new LicensesViewModel();
        PageTitle = "Licences";
    }

    private void ShowPayments()
    {
        CurrentViewModel = new PaymentsViewModel();
        PageTitle = "Paiements";
    }

    private void ShowReports()
    {
        CurrentViewModel = new ReportsViewModel();
        PageTitle = "Rapports";
    }

    private void ShowAdministration()
    {
        CurrentViewModel = new AdministrationViewModel();
        PageTitle = "Administration";
    }
}