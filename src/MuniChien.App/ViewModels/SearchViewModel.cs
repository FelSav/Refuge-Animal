using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using MuniChien.App.Navigation;
using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

/// <summary>
/// Recherche en démonstration locale : chaque recherche relit le même répertoire
/// que les pages Propriétaires et Chiens (aucune seconde liste d'exemples).
/// Le service peut être remplacé par une source API lors d'une étape ultérieure.
/// </summary>
public sealed class SearchViewModel : ViewModelBase
{
    private static readonly CompareInfo FrenchComparison = CultureInfo.GetCultureInfo("fr-CA").CompareInfo;
    private const CompareOptions TextComparison = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private readonly IAnimalDirectoryService _directory;
    private readonly Action<OwnerDogViewModel>? _openDog;
    private readonly Action<int>? _openOwner;

    private string _globalText = string.Empty;
    private string _ownerName = string.Empty;
    private string _dogName = string.Empty;
    private string _fileNumber = string.Empty;
    private string _phone = string.Empty;
    private string _licenseNumber = string.Empty;
    private string _municipality = string.Empty;
    private string _breed = string.Empty;
    private string _color = string.Empty;
    private string _status = string.Empty;
    private string _sex = string.Empty;
    private string _sterilized = string.Empty;
    private string _age = string.Empty;
    private string _weight = string.Empty;
    private string _address = string.Empty;
    private SearchResultViewModel? _selectedDogResult;
    private SearchOwnerResultViewModel? _selectedOwnerResult;
    private int _selectedResultsTabIndex;
    private DateTime? _lastRefreshedAt;

    public SearchViewModel(
        Action<OwnerDogViewModel>? openDog = null,
        Action<int>? openOwner = null,
        IAnimalDirectoryService? directoryService = null)
    {
        _openDog = openDog;
        _openOwner = openOwner;
        _directory = directoryService ?? new LocalAnimalDirectoryService();

        MunicipalityOptions = ["", .. MunicipalityCatalog.All];
        StatusOptions = ["", "Actif", "Inactif"];
        SexOptions = ["", "M", "F"];
        SterilizedOptions = ["", "Oui", "Non"];
        BreedOptions = [];
        ColorOptions = [];

        SearchCommand = new RelayCommand(ApplyFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        OpenDogCommand = new RelayCommand(OpenSelectedDog);
        OpenDogOwnerCommand = new RelayCommand(OpenSelectedDogOwner);
        OpenOwnerCommand = new RelayCommand(OpenSelectedOwner);

        ApplyFilters();
    }

    public DataLoadState LoadState { get; } = new();

    public ObservableCollection<SearchResultViewModel> Results { get; } = [];
    public ObservableCollection<SearchOwnerResultViewModel> OwnerResults { get; } = [];
    public ObservableCollection<string> BreedOptions { get; }
    public ObservableCollection<string> ColorOptions { get; }
    public IReadOnlyList<string> MunicipalityOptions { get; }
    public IReadOnlyList<string> StatusOptions { get; }
    public IReadOnlyList<string> SexOptions { get; }
    public IReadOnlyList<string> SterilizedOptions { get; }

    public ICommand SearchCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand OpenDogCommand { get; }
    public ICommand OpenDogOwnerCommand { get; }
    public ICommand OpenOwnerCommand { get; }

    public string GlobalText { get => _globalText; set => SetText(ref _globalText, value); }
    public string OwnerName { get => _ownerName; set => SetText(ref _ownerName, value); }
    public string DogName { get => _dogName; set => SetText(ref _dogName, value); }
    public string FileNumber { get => _fileNumber; set => SetText(ref _fileNumber, value); }
    public string Phone { get => _phone; set => SetText(ref _phone, value); }
    public string LicenseNumber { get => _licenseNumber; set => SetText(ref _licenseNumber, value); }
    public string Municipality { get => _municipality; set => SetText(ref _municipality, value); }
    public string Breed { get => _breed; set => SetText(ref _breed, value); }
    public string Color { get => _color; set => SetText(ref _color, value); }
    public string Status { get => _status; set => SetText(ref _status, value); }
    public string Sex { get => _sex; set => SetText(ref _sex, value); }
    public string Sterilized { get => _sterilized; set => SetText(ref _sterilized, value); }
    public string Age { get => _age; set => SetText(ref _age, value); }
    public string Weight { get => _weight; set => SetText(ref _weight, value); }
    public string Address { get => _address; set => SetText(ref _address, value); }

    public SearchResultViewModel? SelectedDogResult
    {
        get => _selectedDogResult;
        set
        {
            if (ReferenceEquals(_selectedDogResult, value)) return;
            _selectedDogResult = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedDog));
        }
    }

    public SearchOwnerResultViewModel? SelectedOwnerResult
    {
        get => _selectedOwnerResult;
        set
        {
            if (ReferenceEquals(_selectedOwnerResult, value)) return;
            _selectedOwnerResult = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedOwner));
        }
    }

    public bool HasSelectedDog => SelectedDogResult is not null;
    public bool HasSelectedOwner => SelectedOwnerResult is not null;
    public int ResultCount => Results.Count;
    public int OwnerResultCount => OwnerResults.Count;
    public bool NoDogResults => LoadState.HasResults && Results.Count == 0;
    public bool NoOwnerResults => LoadState.HasResults && OwnerResults.Count == 0;
    public string LastUpdatedText => LoadState.HasError
        ? "Lecture des dossiers indisponible"
        : _lastRefreshedAt is null
            ? "Données locales de démonstration"
            : $"Données locales de démonstration · Actualisé à {_lastRefreshedAt:HH:mm}";

    public int SelectedResultsTabIndex
    {
        get => _selectedResultsTabIndex;
        set
        {
            if (_selectedResultsTabIndex == value) return;
            _selectedResultsTabIndex = value;
            OnPropertyChanged();
        }
    }

    private void SetText(ref string field, string? value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        value ??= string.Empty;
        if (field == value) return;
        field = value;
        OnPropertyChanged(name);
    }

    private void ApplyFilters()
    {
        // Les services locaux répondent immédiatement. Cet état sera visible pendant
        // l'attente réelle des futures requêtes API, sans délai artificiel en démo.
        LoadState.Begin();
        try
        {
            LoadLocalResults();
            var allEmpty = ResultCount == 0 && OwnerResultCount == 0;
            LoadState.Complete(!allEmpty,
                _lastQuerySourceEmpty
                    ? "Aucun dossier n'est disponible dans le répertoire local."
                    : "Aucun dossier ne correspond aux filtres. Essayez de les effacer.");
            _lastRefreshedAt = DateTime.Now;
        }
        catch (Exception ex)
        {
            // Ne jamais présenter d'anciens résultats comme une lecture réussie.
            Results.Clear();
            OwnerResults.Clear();
            SelectedDogResult = null;
            SelectedOwnerResult = null;
            System.Diagnostics.Debug.WriteLine($"Échec de la recherche locale : {ex.GetType().Name}");
            LoadState.Fail("Impossible de charger les dossiers pour le moment. Réessayez; aucune donnée n'a été modifiée.");
        }
        finally
        {
            OnPropertyChanged(nameof(ResultCount));
            OnPropertyChanged(nameof(OwnerResultCount));
            OnPropertyChanged(nameof(NoDogResults));
            OnPropertyChanged(nameof(NoOwnerResults));
            OnPropertyChanged(nameof(LastUpdatedText));
        }
    }

    private bool _lastQuerySourceEmpty;

    private void LoadLocalResults()
    {
        // Relire les données : une modification sur une autre page devient visible
        // en rouvrant Recherche ou en cliquant « Actualiser la recherche ».
        var owners = _directory.GetOwners();
        var dogs = _directory.GetDogs();
        _lastQuerySourceEmpty = owners.Count == 0 && dogs.Count == 0;
        var dogsByOwner = dogs.ToLookup(x => x.OwnerId);
        var ownersById = owners.ToDictionary(x => x.OwnerId);

        UpdateOptions(BreedOptions, dogs.Select(x => x.Breed));
        UpdateOptions(ColorOptions, dogs.Select(x => x.Color));

        var matchingDogs = dogs.Where(d =>
        {
            if (!ownersById.TryGetValue(d.OwnerId, out var owner)) return false;
            return MatchesOwnerFields(owner) && MatchesDogFields(d) && MatchesGlobal(owner, d);
        }).ToList();

        var matchingDogIds = matchingDogs.Select(d => d.DogId).ToHashSet();
        bool dogFiltersActive = HasDogSpecificFilters();
        var matchingOwners = owners.Where(o =>
            MatchesOwnerFields(o)
            && (!dogFiltersActive || dogsByOwner[o.OwnerId].Any(d => matchingDogIds.Contains(d.DogId)))
            && (IsBlank(GlobalText) || MatchesGlobalOwner(o)
                || dogsByOwner[o.OwnerId].Any(d => matchingDogIds.Contains(d.DogId))))
            .OrderBy(o => o.FullName)
            .ThenBy(o => o.FileNumber)
            .ToList();

        Results.Clear();
        foreach (var dog in matchingDogs.OrderBy(d => d.DogName).ThenBy(d => d.OwnerName))
        {
            var owner = ownersById[dog.OwnerId];
            Results.Add(new SearchResultViewModel
            {
                DogId = dog.DogId,
                OwnerId = dog.OwnerId,
                DogName = dog.DogName,
                OwnerName = owner.FullName,
                Municipality = owner.Municipality,
                FileNumber = owner.FileNumber,
                Phone = owner.Phone,
                CellPhone = owner.CellPhone,
                Address = owner.Address,
                LicenseNumber = dog.LicenseNumber,
                Breed = dog.Breed,
                Color = dog.Color,
                Age = dog.AgeDisplay,
                Weight = dog.WeightDisplay,
                Sex = dog.Sex,
                Status = dog.Status,
                Sterilized = dog.Sterilized
            });
        }

        OwnerResults.Clear();
        foreach (var owner in matchingOwners)
        {
            OwnerResults.Add(new SearchOwnerResultViewModel
            {
                OwnerId = owner.OwnerId,
                FileNumber = owner.FileNumber,
                OwnerName = owner.FullName,
                Municipality = owner.Municipality,
                Phone = owner.Phone,
                CellPhone = owner.CellPhone,
                Email = owner.Email,
                Address = owner.Address,
                Status = owner.Status,
                DogCount = dogsByOwner[owner.OwnerId].Count()
            });
        }

        SelectedDogResult = null;
        SelectedOwnerResult = null;
    }

    private bool MatchesOwnerFields(OwnerDirectoryItem owner) =>
        Contains(owner.FullName, OwnerName)
        && Contains(owner.FileNumber, FileNumber)
        && PhoneMatches(owner.Phone, owner.CellPhone, Phone)
        && Contains(owner.Address, Address)
        && (IsBlank(Municipality) || string.Equals(owner.Municipality, Municipality, StringComparison.OrdinalIgnoreCase));

    private bool MatchesDogFields(OwnerDogViewModel dog) =>
        Contains(dog.DogName, DogName)
        && Contains(dog.LicenseNumber, LicenseNumber)
        && Contains(dog.Breed, Breed)
        && Contains(dog.Color, Color)
        && (IsBlank(Status) || string.Equals(dog.Status, Status, StringComparison.OrdinalIgnoreCase))
        && (IsBlank(Sex) || string.Equals(dog.Sex, Sex, StringComparison.OrdinalIgnoreCase))
        && (IsBlank(Sterilized) || string.Equals(dog.Sterilized, Sterilized, StringComparison.OrdinalIgnoreCase))
        && (Contains(dog.AgeDisplay, Age) || Contains(dog.AgeMonths.ToString(CultureInfo.InvariantCulture), Age))
        && (Contains(dog.WeightDisplay, Weight) || Contains(dog.WeightKg.ToString(CultureInfo.InvariantCulture), Weight));

    private bool MatchesGlobal(OwnerDirectoryItem owner, OwnerDogViewModel dog) =>
        IsBlank(GlobalText) || MatchesGlobalOwner(owner)
        || Contains(dog.DogName, GlobalText)
        || Contains(dog.LicenseNumber, GlobalText)
        || Contains(dog.Breed, GlobalText)
        || Contains(dog.Color, GlobalText);

    private bool MatchesGlobalOwner(OwnerDirectoryItem owner) =>
        Contains(owner.FullName, GlobalText)
        || Contains(owner.FileNumber, GlobalText)
        || Contains(owner.Municipality, GlobalText)
        || Contains(owner.Address, GlobalText)
        || Contains(owner.Email, GlobalText)
        || PhoneMatches(owner.Phone, owner.CellPhone, GlobalText);

    private bool HasDogSpecificFilters() =>
        !IsBlank(DogName) || !IsBlank(LicenseNumber) || !IsBlank(Breed)
        || !IsBlank(Color) || !IsBlank(Status) || !IsBlank(Sex)
        || !IsBlank(Sterilized) || !IsBlank(Age) || !IsBlank(Weight);

    private static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);

    private static bool Contains(string? source, string? text) => IsBlank(text)
        || FrenchComparison.IndexOf(source ?? "", text!.Trim(), TextComparison) >= 0;

    private static bool PhoneMatches(string? phone, string? cell, string? text)
    {
        if (IsBlank(text)) return true;
        var requested = new string(text!.Where(char.IsDigit).ToArray());
        if (requested.Length == 0) return Contains(phone, text) || Contains(cell, text);
        static string Digits(string? s) => new((s ?? "").Where(char.IsDigit).ToArray());
        return Digits(phone).Contains(requested, StringComparison.Ordinal)
            || Digits(cell).Contains(requested, StringComparison.Ordinal);
    }

    private static void UpdateOptions(ObservableCollection<string> options, IEnumerable<string> values)
    {
        var desired = values.Where(x => !IsBlank(x)).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).Prepend("").ToArray();
        if (options.SequenceEqual(desired, StringComparer.Ordinal)) return;
        options.Clear();
        foreach (var item in desired) options.Add(item);
    }

    private void ClearFilters()
    {
        GlobalText = "";
        OwnerName = "";
        DogName = "";
        FileNumber = "";
        Phone = "";
        LicenseNumber = "";
        Municipality = "";
        Breed = "";
        Color = "";
        Status = "";
        Sex = "";
        Sterilized = "";
        Age = "";
        Weight = "";
        Address = "";
        ApplyFilters();
    }

    private void OpenSelectedDog()
    {
        if (SelectedDogResult is null || _openDog is null) return;
        // Rechercher la version à jour du chien plutôt que garder une copie de la grille.
        var dog = _directory.GetDog(SelectedDogResult.DogId);
        if (dog is not null) _openDog(dog);
    }

    private void OpenSelectedDogOwner()
    {
        if (SelectedDogResult is not null) _openOwner?.Invoke(SelectedDogResult.OwnerId);
    }

    private void OpenSelectedOwner()
    {
        if (SelectedOwnerResult is not null) _openOwner?.Invoke(SelectedOwnerResult.OwnerId);
    }
}
