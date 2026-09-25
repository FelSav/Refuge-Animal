using System.Collections.ObjectModel;

namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        DashboardCards =
        [
            new("Chiens actifs", "1248"),
            new("Licences à renouveler", "37"),
            new("Soldes impayés", "18"),
            new("Paiements aujourd'hui", "12"),
            new("Chiens sans licence", "64"),
            new("Chiens inactifs", "95"),
            new("Avis à envoyer", "25"),
            new("Paiements ce mois-ci", "798")
        ];
    }

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }
}