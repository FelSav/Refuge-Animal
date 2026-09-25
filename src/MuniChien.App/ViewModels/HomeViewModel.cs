using System.Collections.ObjectModel;

namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        DashboardCards =
        [
            new("activeDogs", "Chiens actifs", "1248"),
            new("licensesToRenew", "Licences à renouveler", "37"),
            new("unpaidBalances", "Soldes impayés", "18"),
            new("paymentsToday", "Paiements aujourd'hui", "12"),
            new("dogsWithoutLicense", "Chiens sans licence", "64"),
            new("inactiveDogs", "Chiens inactifs", "95"),
            new("noticesToSend", "Avis à envoyer", "25"),
            new("paymentsThisMonth", "Paiements ce mois-ci", "798")
        ];
    }

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }
}