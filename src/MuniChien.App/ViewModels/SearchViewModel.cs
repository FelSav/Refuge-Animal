using System.Collections.ObjectModel;
using System.Windows.Input;
using MuniChien.App.Navigation;

namespace MuniChien.App.ViewModels;

public class SearchViewModel : ViewModelBase
{
    // ==================================================
    // DONNÉES
    // ==================================================

    private readonly List<SearchResultViewModel> _allResults = [];


    // ==================================================
    // FILTRES
    // ==================================================

    private string _ownerName = string.Empty;
    private string _dogName = string.Empty;
    private string _fileNumber = string.Empty;
    private string _phone = string.Empty;
    private string _licenseNumber = string.Empty;

    private string _breed = string.Empty;
    private string _color = string.Empty;
    private string _status = string.Empty;

    private string _age = string.Empty;
    private string _weight = string.Empty;
    private string _address = string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public SearchViewModel()
    {
        BreedOptions =
        [
            "",
            "Labrador",
            "Husky",
            "Berger australien",
            "Golden Retriever",
            "Berger allemand",
            "Caniche",
            "Croisé"
        ];

        ColorOptions =
        [
            "",
            "Noir",
            "Blanc",
            "Brun",
            "Doré",
            "Bleu merle",
            "Gris",
            "Roux"
        ];

        StatusOptions =
        [
            "",
            "Actif",
            "Inactif"
        ];

        SearchCommand =
            new RelayCommand(Search);

        ClearFiltersCommand =
            new RelayCommand(ClearFilters);

        LoadExampleData();

        ApplyFilters();
    }


    // ==================================================
    // LISTES DE CHOIX
    // ==================================================

    public IReadOnlyList<string> BreedOptions { get; }

    public IReadOnlyList<string> ColorOptions { get; }

    public IReadOnlyList<string> StatusOptions { get; }


    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    public string OwnerName
    {
        get => _ownerName;

        set
        {
            if (_ownerName == value)
            {
                return;
            }

            _ownerName = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // CHIEN
    // ==================================================

    public string DogName
    {
        get => _dogName;

        set
        {
            if (_dogName == value)
            {
                return;
            }

            _dogName = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // DOSSIER
    // ==================================================

    public string FileNumber
    {
        get => _fileNumber;

        set
        {
            if (_fileNumber == value)
            {
                return;
            }

            _fileNumber = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // TÉLÉPHONE
    // ==================================================

    public string Phone
    {
        get => _phone;

        set
        {
            if (_phone == value)
            {
                return;
            }

            _phone = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // LICENCE
    // ==================================================

    public string LicenseNumber
    {
        get => _licenseNumber;

        set
        {
            if (_licenseNumber == value)
            {
                return;
            }

            _licenseNumber = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // RACE
    // ==================================================

    public string Breed
    {
        get => _breed;

        set
        {
            if (_breed == value)
            {
                return;
            }

            _breed = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // COULEUR
    // ==================================================

    public string Color
    {
        get => _color;

        set
        {
            if (_color == value)
            {
                return;
            }

            _color = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // STATUT
    // ==================================================

    public string Status
    {
        get => _status;

        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // ÂGE
    // ==================================================

    public string Age
    {
        get => _age;

        set
        {
            if (_age == value)
            {
                return;
            }

            _age = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // POIDS
    // ==================================================

    public string Weight
    {
        get => _weight;

        set
        {
            if (_weight == value)
            {
                return;
            }

            _weight = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // ADRESSE
    // ==================================================

    public string Address
    {
        get => _address;

        set
        {
            if (_address == value)
            {
                return;
            }

            _address = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // RÉSULTATS
    // ==================================================

    public ObservableCollection<SearchResultViewModel> Results { get; } = [];

    public int ResultCount =>
        Results.Count;


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand SearchCommand { get; }

    public ICommand ClearFiltersCommand { get; }


    // ==================================================
    // RECHERCHE
    // ==================================================

    private void Search()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        IEnumerable<SearchResultViewModel> query =
            _allResults;

        query = FilterContains(
            query,
            result => result.OwnerName,
            OwnerName);

        query = FilterContains(
            query,
            result => result.DogName,
            DogName);

        query = FilterContains(
            query,
            result => result.FileNumber,
            FileNumber);

        query = FilterContains(
            query,
            result => result.Phone,
            Phone);

        query = FilterContains(
            query,
            result => result.LicenseNumber,
            LicenseNumber);

        query = FilterContains(
            query,
            result => result.Breed,
            Breed);

        query = FilterContains(
            query,
            result => result.Color,
            Color);

        query = FilterContains(
            query,
            result => result.Status,
            Status);

        query = FilterContains(
            query,
            result => result.Age,
            Age);

        query = FilterContains(
            query,
            result => result.Weight,
            Weight);

        query = FilterContains(
            query,
            result => result.Address,
            Address);

        Results.Clear();

        foreach (SearchResultViewModel result in query)
        {
            Results.Add(result);
        }

        OnPropertyChanged(nameof(ResultCount));
    }

    private static IEnumerable<SearchResultViewModel> FilterContains(
        IEnumerable<SearchResultViewModel> source,
        Func<SearchResultViewModel, string> selector,
        string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return source;
        }

        string trimmedFilter =
            filter.Trim();

        return source.Where(result =>
            selector(result).Contains(
                trimmedFilter,
                StringComparison.OrdinalIgnoreCase));
    }


    // ==================================================
    // EFFACER LES FILTRES
    // ==================================================

    private void ClearFilters()
    {
        OwnerName = string.Empty;
        DogName = string.Empty;
        FileNumber = string.Empty;
        Phone = string.Empty;
        LicenseNumber = string.Empty;

        Breed = string.Empty;
        Color = string.Empty;
        Status = string.Empty;

        Age = string.Empty;
        Weight = string.Empty;
        Address = string.Empty;

        ApplyFilters();
    }


    // ==================================================
    // DONNÉES TEMPORAIRES
    // PLUS TARD : API DE MAËL
    // ==================================================

    private void LoadExampleData()
    {
        _allResults.Clear();

        _allResults.Add(
            new SearchResultViewModel
            {
                DogId = 1,
                DogName = "Lucky",
                OwnerName = "Jean Tremblay",
                Phone = "(418) 123-4567",
                FileNumber = "D-000124",
                LicenseNumber = "10452",
                Breed = "Labrador",
                Color = "Brun",
                Age = "6 ans",
                Weight = "28.4 kg",
                Address = "120 boulevard Marcotte",
                Sex = "M",
                Status = "Actif",
                Sterilized = "Oui"
            });

        _allResults.Add(
            new SearchResultViewModel
            {
                DogId = 2,
                DogName = "Jack",
                OwnerName = "Pierre Savard",
                Phone = "(418) 555-0142",
                FileNumber = "D-000238",
                LicenseNumber = "10453",
                Breed = "Husky",
                Color = "Blanc",
                Age = "2 ans",
                Weight = "20.4 kg",
                Address = "42 rue Saint-Joseph",
                Sex = "F",
                Status = "Actif",
                Sterilized = "Non"
            });

        _allResults.Add(
            new SearchResultViewModel
            {
                DogId = 3,
                DogName = "Luna",
                OwnerName = "Marie Gagnon",
                Phone = "(418) 555-2222",
                FileNumber = "D-000301",
                LicenseNumber = "10454",
                Breed = "Berger australien",
                Color = "Bleu merle",
                Age = "4 ans",
                Weight = "22.1 kg",
                Address = "875 avenue Roberval",
                Sex = "F",
                Status = "Inactif",
                Sterilized = "Oui"
            });
    }
}