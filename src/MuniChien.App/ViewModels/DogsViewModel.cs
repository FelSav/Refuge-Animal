using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class DogsViewModel : ViewModelBase
{
    // ==================================================
    // SOURCE / NAVIGATION
    // ==================================================

    private readonly OwnerDogViewModel? _sourceDog;
    private readonly Action<int>? _openOwnerAction;


    // ==================================================
    // ÉTAT
    // ==================================================

    private bool _isEditing;

    private string _validationMessage =
        string.Empty;


    // ==================================================
    // DONNÉES DU CHIEN
    // ==================================================

    private string _dogName =
        "Aucun chien sélectionné";

    private string _breed =
        "—";

    private string _color =
        "—";

    private string _sex =
        "—";

    private string _sterilized =
        "—";

    private string _status =
        "—";

    private int _ageMonths;

    private double _weightKg;

    private string _comments =
        "—";

    private string _deactivationDate =
        string.Empty;

    private string _deactivationReason =
        string.Empty;


    // ==================================================
    // LICENCE ACTUELLE
    // ==================================================

    private string _licenseStatus =
        "—";

    private DateTime? _licenseIssueDate;

    private DateTime? _licenseExpirationDate;


    // ==================================================
    // CHAMPS TEMPORAIRES D'ÉDITION
    // ==================================================

    private string _ageValueInput =
        string.Empty;

    private string _ageUnit =
        "ans";

    private string _weightInput =
        string.Empty;


    // ==================================================
    // VALEURS ORIGINALES
    // POUR ANNULER
    // ==================================================

    private string _originalDogName =
        string.Empty;

    private string _originalBreed =
        string.Empty;

    private string _originalColor =
        string.Empty;

    private string _originalSex =
        string.Empty;

    private string _originalSterilized =
        string.Empty;

    private string _originalStatus =
        string.Empty;

    private int _originalAgeMonths;

    private double _originalWeightKg;

    private string _originalComments =
        string.Empty;

    private string _originalDeactivationDate =
        string.Empty;

    private string _originalDeactivationReason =
        string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public DogsViewModel(
        OwnerDogViewModel? dog = null,
        Action<int>? openOwnerAction = null)
    {
        _sourceDog =
            dog;

        _openOwnerAction =
            openOwnerAction;


        BreedOptions =
        [
            "Berger allemand",
            "Berger australien",
            "Caniche",
            "Chihuahua",
            "Croisé",
            "Golden Retriever",
            "Husky",
            "Labrador",
            "Shih Tzu"
        ];


        ColorOptions =
        [
            "Beige",
            "Blanc",
            "Bleu merle",
            "Brun",
            "Doré",
            "Gris",
            "Noir",
            "Roux"
        ];


        SexOptions =
        [
            "M",
            "F"
        ];


        SterilizedOptions =
        [
            "Oui",
            "Non"
        ];


        StatusOptions =
        [
            "Actif",
            "Inactif"
        ];


        AgeUnitOptions =
        [
            "mois",
            "ans"
        ];


        ModifyCommand =
            new RelayCommand(StartEditing);

        SaveCommand =
            new RelayCommand(SaveChanges);

        CancelCommand =
            new RelayCommand(CancelChanges);

        RenewLicenseCommand =
            new RelayCommand(RenewLicense);

        OpenOwnerCommand =
            new RelayCommand(OpenOwner);


        LoadDog();

        LoadLicense();
    }


    // ==================================================
    // OPTIONS
    // ==================================================

    public IReadOnlyList<string> BreedOptions { get; }

    public IReadOnlyList<string> ColorOptions { get; }

    public IReadOnlyList<string> SexOptions { get; }

    public IReadOnlyList<string> SterilizedOptions { get; }

    public IReadOnlyList<string> StatusOptions { get; }

    public IReadOnlyList<string> AgeUnitOptions { get; }


    // ==================================================
    // ÉTAT
    // ==================================================

    public bool HasSelectedDog =>
        _sourceDog is not null;


    public bool IsEditing
    {
        get => _isEditing;

        private set
        {
            if (_isEditing == value)
            {
                return;
            }

            _isEditing =
                value;

            OnPropertyChanged();
        }
    }


    public string ValidationMessage
    {
        get => _validationMessage;

        private set
        {
            if (_validationMessage == value)
            {
                return;
            }

            _validationMessage =
                value;

            OnPropertyChanged();
        }
    }


    public bool IsInactive =>
        Status == "Inactif";


    // ==================================================
    // IDENTITÉ DU CHIEN
    // ==================================================

    public int DogId =>
        _sourceDog?.DogId ?? 0;


    public string DogName
    {
        get => _dogName;

        set
        {
            if (_dogName == value)
            {
                return;
            }

            _dogName =
                value;

            OnPropertyChanged();
        }
    }


    public string Breed
    {
        get => _breed;

        set
        {
            if (_breed == value)
            {
                return;
            }

            _breed =
                value;

            OnPropertyChanged();
        }
    }


    public string Color
    {
        get => _color;

        set
        {
            if (_color == value)
            {
                return;
            }

            _color =
                value;

            OnPropertyChanged();
        }
    }


    public string Sex
    {
        get => _sex;

        set
        {
            if (_sex == value)
            {
                return;
            }

            _sex =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(SexDisplay));
        }
    }


    public string SexDisplay =>
        Sex switch
        {
            "M" => "Mâle",
            "F" => "Femelle",
            _ => "—"
        };


    public string Sterilized
    {
        get => _sterilized;

        set
        {
            if (_sterilized == value)
            {
                return;
            }

            _sterilized =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // ÂGE
    // ==================================================

    public string AgeDisplay =>
        FormatAge(
            _ageMonths);


    public string AgeValueInput
    {
        get => _ageValueInput;

        set
        {
            if (_ageValueInput == value)
            {
                return;
            }

            _ageValueInput =
                value;

            OnPropertyChanged();
        }
    }


    public string AgeUnit
    {
        get => _ageUnit;

        set
        {
            if (_ageUnit == value)
            {
                return;
            }

            _ageUnit =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // POIDS
    // ==================================================

    public string WeightDisplay =>
        HasSelectedDog
            ? $"{_weightKg:0.##} kg"
            : "—";


    public string WeightInput
    {
        get => _weightInput;

        set
        {
            if (_weightInput == value)
            {
                return;
            }

            _weightInput =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // NAISSANCE
    // ==================================================

    public string BirthDate =>
        "—";


    // ==================================================
    // COMMENTAIRES
    // ==================================================

    public string Comments
    {
        get => _comments;

        set
        {
            if (_comments == value)
            {
                return;
            }

            _comments =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    public int OwnerId =>
        _sourceDog?.OwnerId ?? 0;


    public string OwnerName =>
        _sourceDog?.OwnerName
        ?? "—";


    public string OwnerFileNumber =>
        HasSelectedDog
            ? "#12345"
            : "—";


    public string OwnerAddress =>
        HasSelectedDog
            ? "123, boulevard Exemple, Roberval, QC, G8H 2M9"
            : "—";


    // ==================================================
    // STATUT DU CHIEN
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

            _status =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(IsInactive));

            OnPropertyChanged(
                nameof(DeactivationDateDisplay));

            OnPropertyChanged(
                nameof(DeactivationReasonDisplay));
        }
    }


    public string DeactivationDate
    {
        get => _deactivationDate;

        set
        {
            if (_deactivationDate == value)
            {
                return;
            }

            _deactivationDate =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(DeactivationDateDisplay));
        }
    }


    public string DeactivationReason
    {
        get => _deactivationReason;

        set
        {
            if (_deactivationReason == value)
            {
                return;
            }

            _deactivationReason =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(DeactivationReasonDisplay));
        }
    }


    public string DeactivationDateDisplay =>
        IsInactive &&
        !string.IsNullOrWhiteSpace(
            DeactivationDate)
            ? DeactivationDate
            : "—";


    public string DeactivationReasonDisplay =>
        IsInactive &&
        !string.IsNullOrWhiteSpace(
            DeactivationReason)
            ? DeactivationReason
            : "—";


    // ==================================================
    // LICENCE ACTUELLE
    // ==================================================

    public string LicenseNumber =>
        string.IsNullOrWhiteSpace(
            _sourceDog?.LicenseNumber)
            ? "—"
            : _sourceDog!.LicenseNumber;


    public string LicenseStatus =>
        _licenseStatus;


    public string LicenseIssueDate =>
        _licenseIssueDate.HasValue
            ? _licenseIssueDate.Value.ToString(
                "yyyy-MM-dd")
            : "—";


    public string LicenseExpirationDate =>
        _licenseExpirationDate.HasValue
            ? _licenseExpirationDate.Value.ToString(
                "yyyy-MM-dd")
            : "—";


    // ==================================================
    // HISTORIQUE
    // ==================================================

    public ObservableCollection<DogLicenseHistoryViewModel>
        LicenseHistory
    { get; } =
        [];


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand ModifyCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand RenewLicenseCommand { get; }

    public ICommand OpenOwnerCommand { get; }


    // ==================================================
    // CHARGEMENT DU CHIEN
    // ==================================================

    private void LoadDog()
    {
        if (_sourceDog is null)
        {
            DogName =
                "Aucun chien sélectionné";

            Breed =
                "—";

            Color =
                "—";

            Sex =
                "—";

            Sterilized =
                "—";

            Status =
                "—";

            _ageMonths =
                0;

            _weightKg =
                0;

            Comments =
                "—";

            return;
        }


        DogName =
            _sourceDog.DogName;

        Breed =
            _sourceDog.Breed;

        Color =
            _sourceDog.Color;

        Sex =
            _sourceDog.Sex;

        Sterilized =
            _sourceDog.Sterilized;

        Status =
            _sourceDog.Status;

        _ageMonths =
            _sourceDog.AgeMonths;

        _weightKg =
            _sourceDog.WeightKg;

        Comments =
            "Chien calme et sociable.";


        PrepareAgeInput();

        PrepareWeightInput();
    }


    // ==================================================
    // CHARGEMENT DE LA LICENCE
    // ==================================================

    private void LoadLicense()
    {
        if (!HasSelectedDog ||
            LicenseNumber == "—")
        {
            _licenseStatus =
                "—";

            _licenseIssueDate =
                null;

            _licenseExpirationDate =
                null;

            return;
        }


        // Données temporaires en attendant l'API.
        _licenseStatus =
            "Valide";

        _licenseIssueDate =
            new DateTime(
                2026,
                1,
                15);

        _licenseExpirationDate =
            new DateTime(
                2027,
                1,
                15);


        LicenseHistory.Add(
            new DogLicenseHistoryViewModel
            {
                LicenseNumber =
                    LicenseNumber,

                IssueDate =
                    LicenseIssueDate,

                ExpirationDate =
                    LicenseExpirationDate,

                Status =
                    LicenseStatus
            });
    }


    // ==================================================
    // MODIFIER
    // ==================================================

    private void StartEditing()
    {
        if (!HasSelectedDog)
        {
            return;
        }


        SaveOriginalValues();

        PrepareAgeInput();

        PrepareWeightInput();

        ValidationMessage =
            string.Empty;

        IsEditing =
            true;
    }


    // ==================================================
    // ENREGISTRER
    // ==================================================

    private void SaveChanges()
    {
        if (!HasSelectedDog)
        {
            return;
        }


        ValidationMessage =
            string.Empty;


        // NOM

        if (string.IsNullOrWhiteSpace(
                DogName))
        {
            ValidationMessage =
                "Le nom du chien est obligatoire.";

            return;
        }


        DogName =
            DogName.Trim();


        // RACE

        if (!TryGetCanonicalValue(
                Breed,
                BreedOptions,
                out string canonicalBreed))
        {
            ValidationMessage =
                "Veuillez choisir une race existante.";

            return;
        }


        Breed =
            canonicalBreed;


        // COULEUR

        if (!TryGetCanonicalValue(
                Color,
                ColorOptions,
                out string canonicalColor))
        {
            ValidationMessage =
                "Veuillez choisir une couleur existante.";

            return;
        }


        Color =
            canonicalColor;


        // ÂGE

        if (!int.TryParse(
                AgeValueInput.Trim(),
                out int ageValue) ||
            ageValue < 0)
        {
            ValidationMessage =
                "L'âge doit être un nombre valide.";

            return;
        }


        _ageMonths =
            AgeUnit == "ans"
                ? ageValue * 12
                : ageValue;


        // POIDS

        if (!TryParseWeight(
                WeightInput,
                out double parsedWeight) ||
            parsedWeight <= 0)
        {
            ValidationMessage =
                "Le poids doit être un nombre supérieur à 0.";

            return;
        }


        _weightKg =
            parsedWeight;


        // STATUT INACTIF

        if (Status == "Inactif")
        {
            if (string.IsNullOrWhiteSpace(
                    DeactivationDate))
            {
                ValidationMessage =
                    "Une date d'inactivation est requise pour un chien inactif.";

                return;
            }


            if (!DateTime.TryParseExact(
                    DeactivationDate.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                ValidationMessage =
                    "La date d'inactivation doit respecter le format AAAA-MM-JJ.";

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    DeactivationReason))
            {
                ValidationMessage =
                    "Une raison d'inactivation est requise.";

                return;
            }


            DeactivationDate =
                DeactivationDate.Trim();

            DeactivationReason =
                DeactivationReason.Trim();
        }
        else
        {
            DeactivationDate =
                string.Empty;

            DeactivationReason =
                string.Empty;
        }


        Comments =
            Comments.Trim();


        OnPropertyChanged(
            nameof(AgeDisplay));

        OnPropertyChanged(
            nameof(WeightDisplay));

        OnPropertyChanged(
            nameof(DeactivationDateDisplay));

        OnPropertyChanged(
            nameof(DeactivationReasonDisplay));


        PrepareAgeInput();

        PrepareWeightInput();


        IsEditing =
            false;


        // Plus tard :
        // appel API.
    }


    // ==================================================
    // ANNULER
    // ==================================================

    private void CancelChanges()
    {
        DogName =
            _originalDogName;

        Breed =
            _originalBreed;

        Color =
            _originalColor;

        Sex =
            _originalSex;

        Sterilized =
            _originalSterilized;

        Status =
            _originalStatus;

        _ageMonths =
            _originalAgeMonths;

        _weightKg =
            _originalWeightKg;

        Comments =
            _originalComments;

        DeactivationDate =
            _originalDeactivationDate;

        DeactivationReason =
            _originalDeactivationReason;


        PrepareAgeInput();

        PrepareWeightInput();


        ValidationMessage =
            string.Empty;


        OnPropertyChanged(
            nameof(AgeDisplay));

        OnPropertyChanged(
            nameof(WeightDisplay));


        IsEditing =
            false;
    }


    // ==================================================
    // SAUVEGARDE POUR ANNULER
    // ==================================================

    private void SaveOriginalValues()
    {
        _originalDogName =
            DogName;

        _originalBreed =
            Breed;

        _originalColor =
            Color;

        _originalSex =
            Sex;

        _originalSterilized =
            Sterilized;

        _originalStatus =
            Status;

        _originalAgeMonths =
            _ageMonths;

        _originalWeightKg =
            _weightKg;

        _originalComments =
            Comments;

        _originalDeactivationDate =
            DeactivationDate;

        _originalDeactivationReason =
            DeactivationReason;
    }


    // ==================================================
    // RENOUVELLEMENT DE LICENCE
    // ==================================================

    private void RenewLicense()
    {
        if (!HasSelectedDog)
        {
            return;
        }


        ValidationMessage =
            string.Empty;


        if (LicenseNumber == "—")
        {
            ValidationMessage =
                "Ce chien ne possède aucune licence à renouveler.";

            return;
        }


        if (!_licenseExpirationDate.HasValue)
        {
            ValidationMessage =
                "Les informations de la licence actuelle sont incomplètes.";

            return;
        }


        DateTime renewalDate =
            DateTime.Today;


        // Évite un double renouvellement accidentel
        // pendant la même journée.

        if (_licenseIssueDate.HasValue &&
            _licenseIssueDate.Value.Date ==
            renewalDate.Date)
        {
            ValidationMessage =
                "Cette licence a déjà été renouvelée aujourd'hui.";

            return;
        }


        DateTime newExpirationDate =
            CalculateLicenseExpiration(
                renewalDate);


        bool confirmed =
            LicenseRenewalDialogService.Show(
                DogName,
                OwnerName,
                LicenseNumber,
                _licenseExpirationDate.Value,
                renewalDate,
                newExpirationDate);


        if (!confirmed)
        {
            return;
        }


        ArchiveCurrentLicensePeriod();


        _licenseIssueDate =
            renewalDate;

        _licenseExpirationDate =
            newExpirationDate;

        _licenseStatus =
            "Valide";


        OnPropertyChanged(
            nameof(LicenseIssueDate));

        OnPropertyChanged(
            nameof(LicenseExpirationDate));

        OnPropertyChanged(
            nameof(LicenseStatus));


        LicenseHistory.Add(
            new DogLicenseHistoryViewModel
            {
                LicenseNumber =
                    LicenseNumber,

                IssueDate =
                    LicenseIssueDate,

                ExpirationDate =
                    LicenseExpirationDate,

                Status =
                    "Valide"
            });


        // Plus tard :
        // POST / licences / renew
        // et création du paiement associé via l'API.
    }


    // ==================================================
    // ARCHIVER LA PÉRIODE ACTUELLE
    // ==================================================

    private void ArchiveCurrentLicensePeriod()
    {
        for (int i =
                 LicenseHistory.Count - 1;
             i >= 0;
             i--)
        {
            DogLicenseHistoryViewModel entry =
                LicenseHistory[i];


            if (entry.LicenseNumber ==
                    LicenseNumber &&
                entry.Status ==
                    "Valide")
            {
                LicenseHistory[i] =
                    new DogLicenseHistoryViewModel
                    {
                        LicenseNumber =
                            entry.LicenseNumber,

                        IssueDate =
                            entry.IssueDate,

                        ExpirationDate =
                            entry.ExpirationDate,

                        Status =
                            "Renouvelée"
                    };

                return;
            }
        }


        if (_licenseIssueDate.HasValue &&
            _licenseExpirationDate.HasValue)
        {
            LicenseHistory.Add(
                new DogLicenseHistoryViewModel
                {
                    LicenseNumber =
                        LicenseNumber,

                    IssueDate =
                        _licenseIssueDate.Value
                            .ToString(
                                "yyyy-MM-dd"),

                    ExpirationDate =
                        _licenseExpirationDate.Value
                            .ToString(
                                "yyyy-MM-dd"),

                    Status =
                        "Renouvelée"
                });
        }
    }


    // ==================================================
    // RÈGLE MÉTIER :
    // UNE LICENCE EST VALIDE 1 AN À PARTIR
    // DE LA DATE DU RENOUVELLEMENT.
    // ==================================================

    private static DateTime CalculateLicenseExpiration(
        DateTime renewalDate)
    {
        return renewalDate
            .Date
            .AddYears(1);
    }


    // ==================================================
    // PRÉPARATION ÂGE
    // ==================================================

    private void PrepareAgeInput()
    {
        if (_ageMonths > 0 &&
            _ageMonths % 12 == 0)
        {
            AgeUnit =
                "ans";

            AgeValueInput =
                (_ageMonths / 12)
                .ToString(
                    CultureInfo.InvariantCulture);
        }
        else
        {
            AgeUnit =
                "mois";

            AgeValueInput =
                _ageMonths
                .ToString(
                    CultureInfo.InvariantCulture);
        }
    }


    // ==================================================
    // PRÉPARATION POIDS
    // ==================================================

    private void PrepareWeightInput()
    {
        WeightInput =
            _weightKg.ToString(
                "0.##",
                CultureInfo.CurrentCulture);
    }


    // ==================================================
    // NORMALISATION
    // ==================================================

    private static bool TryGetCanonicalValue(
        string input,
        IEnumerable<string> options,
        out string canonicalValue)
    {
        string normalized =
            input.Trim();


        string? match =
            options.FirstOrDefault(
                option =>
                    option.Equals(
                        normalized,
                        StringComparison.OrdinalIgnoreCase));


        if (match is null)
        {
            canonicalValue =
                string.Empty;

            return false;
        }


        canonicalValue =
            match;

        return true;
    }


    // ==================================================
    // POIDS
    // ==================================================

    private static bool TryParseWeight(
        string input,
        out double weight)
    {
        string normalized =
            input
                .Trim()
                .Replace(',', '.');


        return double.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out weight);
    }


    // ==================================================
    // AFFICHAGE ÂGE
    // ==================================================

    private static string FormatAge(
        int ageMonths)
    {
        if (ageMonths <= 0)
        {
            return "—";
        }


        if (ageMonths < 12)
        {
            return $"{ageMonths} mois";
        }


        int years =
            ageMonths / 12;

        int months =
            ageMonths % 12;


        if (months == 0)
        {
            return years == 1
                ? "1 an"
                : $"{years} ans";
        }


        return years == 1
            ? $"1 an {months} mois"
            : $"{years} ans {months} mois";
    }


    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    private void OpenOwner()
    {
        if (OwnerId <= 0)
        {
            return;
        }


        _openOwnerAction?.Invoke(
            OwnerId);
    }
}