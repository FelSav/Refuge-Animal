using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public static class LocalLicenseStore
{
    private static readonly List<LicenseListItemViewModel>
        _licenses =
        [];


    private static readonly Dictionary<string, List<DogLicenseHistoryViewModel>>
        _history =
        new(StringComparer.OrdinalIgnoreCase);


    // ==================================================
    // INITIALISATION
    // ==================================================

    static LocalLicenseStore()
    {
        LoadExampleLicenses();
    }


    // ==================================================
    // LICENCES
    // ==================================================

    public static IReadOnlyList<LicenseListItemViewModel>
        Licenses =>
        _licenses;

    // Une seule licence initiale par chien, avec numéro provisoire non officiel.
    // La facturation est effectuée une fois, au moment de la création.
    private static int _nextDemoLicenseNumber = 1;

    public static LicenseListItemViewModel CreateLicense(
        int dogId, DateTime issueDate, decimal amountToBill)
    {
        if (issueDate.Date > DateTime.Today)
            throw new ArgumentException("La date d'émission ne peut pas être dans le futur.");
        if (issueDate.Year < 2000)
            throw new ArgumentException("La date d'émission est invalide.");
        if (amountToBill <= 0 || amountToBill > 100000m)
            throw new ArgumentException("Saisissez un montant de licence positif et raisonnable.");

        OwnerDogViewModel dog = LocalAnimalDirectoryStore.GetDog(dogId)
            ?? throw new InvalidOperationException("Le chien n'existe plus dans le répertoire.");
        OwnerDirectoryItem owner = LocalAnimalDirectoryStore.GetOwner(dog.OwnerId)
            ?? throw new InvalidOperationException("Le propriétaire n'existe plus dans le répertoire.");
        if (_licenses.Any(x => x.DogId == dogId) ||
            (!string.IsNullOrWhiteSpace(dog.LicenseNumber) &&
             GetCurrentLicense(dog.LicenseNumber) is not null))
            throw new InvalidOperationException("Ce chien possède déjà une licence.");

        string number;
        do
        {
            number = $"DEMO-{_nextDemoLicenseNumber++:D5}";
        } while (GetCurrentLicense(number) is not null);

        DateTime expiration = CalculateExpiration(issueDate.Date);
        string status = expiration < DateTime.Today ? "Expirée" : "Valide";
        var license = new LicenseListItemViewModel
        {
            DogId = dog.DogId, OwnerId = owner.OwnerId,
            LicenseNumber = number,
            DogName = dog.DogName, OwnerName = owner.FullName,
            Municipality = owner.Municipality,
            IssueDate = issueDate.Date,
            ExpirationDate = expiration,
            Status = status, Breed = dog.Breed, AgeMonths = dog.AgeMonths,
            WeightKg = dog.WeightKg, Color = dog.Color, Sex = dog.Sex,
            Sterilized = dog.Sterilized
        };

        // Le paiement est un montant explicitement saisi, jamais un tarif inféré.
        // Validation des références en amont : aucun changement sur les exemples existants.
        LocalPaymentStore.AddLicenseCharge(owner.OwnerId, number, amountToBill);
        _licenses.Add(license);
        _history[number] =
        [
            new DogLicenseHistoryViewModel
            {
                LicenseNumber = number,
                IssueDate = issueDate.Date.ToString("yyyy-MM-dd"),
                ExpirationDate = license.ExpirationDate.ToString("yyyy-MM-dd"),
                Status = status
            }
        ];
        LocalAnimalDirectoryStore.SaveDog(new OwnerDogViewModel
        {
            DogId = dog.DogId, OwnerId = dog.OwnerId,
            DogName = dog.DogName, OwnerName = dog.OwnerName,
            Breed = dog.Breed, AgeMonths = dog.AgeMonths,
            WeightKg = dog.WeightKg, Color = dog.Color, Sex = dog.Sex,
            Status = dog.Status, Sterilized = dog.Sterilized,
            LicenseNumber = number
        }, LocalAnimalDirectoryStore.GetDogDetails(dogId) ?? new DogDirectoryDetails());
        return license;
    }


    public static LicenseListItemViewModel?
        GetCurrentLicense(
            string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(
                licenseNumber))
        {
            return null;
        }


        return _licenses.FirstOrDefault(
            license =>
                license.LicenseNumber.Equals(
                    licenseNumber,
                    StringComparison.OrdinalIgnoreCase));
    }


    // ==================================================
    // HISTORIQUE
    // ==================================================

    public static IReadOnlyList<DogLicenseHistoryViewModel>
        GetHistory(
            string licenseNumber)
    {
        if (!_history.TryGetValue(
                licenseNumber,
                out List<DogLicenseHistoryViewModel>? history))
        {
            return [];
        }


        return history;
    }


    // ==================================================
    // RÈGLE MÉTIER
    //
    // Toute licence expire le 31 décembre de son année.
    // Exception autorisée explicitement par le refuge :
    // expiration au 31 décembre de l'année suivante.
    // ==================================================

    public static DateTime CalculateExpiration(
        DateTime renewalDate,
        bool extendToNextYear = false)
    {
        int expirationYear = checked(
            renewalDate.Year + (extendToNextYear ? 1 : 0));

        return new DateTime(expirationYear, 12, 31);
    }


    // ==================================================
    // RENOUVELLEMENT
    // ==================================================

    // Un renouvellement doit faire avancer la période de couverture.
    // L'opération comprend UNE charge identifiée par licence + année.
    // La prolongation exceptionnelle n'ajoute pas une seconde charge.
    public static LicenseListItemViewModel? RenewLicense(
        string licenseNumber, DateTime renewalDate, int coverageYear,
        decimal amountToBill, bool extendToNextYear = false)
    {
        LicenseListItemViewModel? license = GetCurrentLicense(licenseNumber);
        if (license is null)
            return null;

        if (renewalDate.Date > DateTime.Today || renewalDate.Year < 2000)
            throw new ArgumentException("La date de renouvellement est invalide.");
        if (license.IssueDate.Date == renewalDate.Date)
            throw new InvalidOperationException("Cette licence a déjà été créée ou renouvelée aujourd'hui.");
        if (amountToBill <= 0m || amountToBill > 100000m || decimal.Round(amountToBill, 2) != amountToBill)
            throw new ArgumentException("Entrez un montant positif avec au plus deux décimales.");

        // Une année à la fois : pour une licence expirée, reprendre à l'année courante.
        // Pour une licence valide, avancer après sa dernière année couverte.
        int expectedYear = Math.Max(renewalDate.Year, license.ExpirationDate.Year + 1);
        if (coverageYear != expectedYear)
            throw new InvalidOperationException(
                $"Le renouvellement doit viser l'année {expectedYear} pour éviter une période en double.");
        if (coverageYear > 9998 || (extendToNextYear && coverageYear > 9998))
            throw new ArgumentOutOfRangeException(nameof(coverageYear));

        int expirationYear = checked(coverageYear + (extendToNextYear ? 1 : 0));
        DateTime newExpiration = new(expirationYear, 12, 31);
        if (newExpiration <= license.ExpirationDate)
            throw new InvalidOperationException("La nouvelle période doit prolonger la licence.");

        // Facturer avant de modifier la licence : une clé identique est refusée
        // et l'historique reste inchangé en cas d'échec de facturation.
        LocalPaymentStore.AddLicenseRenewalCharge(
            license.OwnerId, license.LicenseNumber, coverageYear, amountToBill);

        if (!_history.TryGetValue(license.LicenseNumber, out var history))
        {
            history = [];
            _history[license.LicenseNumber] = history;
        }

        if (history.Count > 0)
        {
            DogLicenseHistoryViewModel previous = history[^1];
            history[^1] = new DogLicenseHistoryViewModel
            {
                LicenseNumber = previous.LicenseNumber,
                IssueDate = previous.IssueDate,
                ExpirationDate = previous.ExpirationDate,
                Status = "Renouvelée"
            };
        }
        else
        {
            history.Add(new DogLicenseHistoryViewModel
            {
                LicenseNumber = license.LicenseNumber,
                IssueDate = license.IssueDate.ToString("yyyy-MM-dd"),
                ExpirationDate = license.ExpirationDate.ToString("yyyy-MM-dd"),
                Status = "Renouvelée"
            });
        }

        license.IssueDate = renewalDate.Date;
        license.ExpirationDate = newExpiration;
        license.Status = "Valide";
        history.Add(new DogLicenseHistoryViewModel
        {
            LicenseNumber = license.LicenseNumber,
            IssueDate = license.IssueDate.ToString("yyyy-MM-dd"),
            ExpirationDate = license.ExpirationDate.ToString("yyyy-MM-dd"),
            Status = "Valide"
        });
        return license;
    }


    // ==================================================
    // DONNÉES TEMPORAIRES
    // ==================================================

    private static void LoadExampleLicenses()
    {
        _licenses.Clear();
        _history.Clear();


        AddLicense(
            1,
            1,
            "10452",
            "Lucky",
            "Jean Tremblay",
            "Roberval",
            new DateTime(2026, 1, 15),
            new DateTime(2026, 12, 31),
            "Valide",
            "Labrador",
            72,
            28.4,
            "Brun",
            "M",
            "Oui");


        AddLicense(
            2,
            1,
            "10453",
            "Jack",
            "Jean Tremblay",
            "Roberval",
            new DateTime(2026, 3, 18),
            new DateTime(2026, 12, 31),
            "Valide",
            "Husky",
            24,
            20.4,
            "Blanc",
            "F",
            "Non");


        AddLicense(
            3,
            2,
            "09871",
            "Maya",
            "Pierre Savard",
            "Chambord",
            new DateTime(2024, 8, 12),
            new DateTime(2024, 12, 31),
            "Expirée",
            "Berger allemand",
            60,
            31.2,
            "Noir",
            "F",
            "Oui");


        AddLicense(
            4,
            3,
            "10501",
            "Charlie",
            "Sophie Gagnon",
            "Saint-Félicien",
            new DateTime(2026, 1, 25),
            new DateTime(2026, 12, 31),
            "À renouveler",
            "Golden Retriever",
            48,
            27.8,
            "Doré",
            "M",
            "Oui");


        AddLicense(
            5,
            4,
            "10519",
            "Luna",
            "Marc Bouchard",
            "Roberval",
            new DateTime(2026, 2, 4),
            new DateTime(2026, 12, 31),
            "À renouveler",
            "Caniche",
            36,
            9.7,
            "Blanc",
            "F",
            "Oui");


        AddLicense(
            6,
            5,
            "10603",
            "Rocky",
            "Julie Fortin",
            "Dolbeau - Mistassini",
            new DateTime(2026, 7, 8),
            new DateTime(2026, 12, 31),
            "Valide",
            "Berger australien",
            30,
            22.5,
            "Bleu merle",
            "M",
            "Non");


        AddLicense(
            7,
            6,
            "09644",
            "Bella",
            "Luc Tremblay",
            "Saint-Prime",
            new DateTime(2024, 9, 10),
            new DateTime(2024, 12, 31),
            "Expirée",
            "Shih Tzu",
            84,
            6.2,
            "Beige",
            "F",
            "Oui");


        AddLicense(
            8,
            7,
            "10625",
            "Max",
            "Nathalie Simard",
            "Roberval",
            new DateTime(2026, 4, 22),
            new DateTime(2026, 12, 31),
            "Valide",
            "Croisé",
            54,
            18.6,
            "Roux",
            "M",
            "Oui");
    }


    private static void AddLicense(
        int dogId,
        int ownerId,
        string licenseNumber,
        string dogName,
        string ownerName,
        string municipality,
        DateTime issueDate,
        DateTime expirationDate,
        string status,
        string breed,
        int ageMonths,
        double weightKg,
        string color,
        string sex,
        string sterilized)
    {
        LicenseListItemViewModel license =
            new()
            {
                DogId =
                    dogId,

                OwnerId =
                    ownerId,

                LicenseNumber =
                    licenseNumber,

                DogName =
                    dogName,

                OwnerName =
                    ownerName,

                Municipality =
                    municipality,

                IssueDate =
                    issueDate,

                ExpirationDate =
                    expirationDate,

                Status =
                    status,

                Breed =
                    breed,

                AgeMonths =
                    ageMonths,

                WeightKg =
                    weightKg,

                Color =
                    color,

                Sex =
                    sex,

                Sterilized =
                    sterilized
            };


        _licenses.Add(
            license);


        _history[licenseNumber] =
        [
            new DogLicenseHistoryViewModel
            {
                LicenseNumber =
                    licenseNumber,

                IssueDate =
                    issueDate.ToString(
                        "yyyy-MM-dd"),

                ExpirationDate =
                    expirationDate.ToString(
                        "yyyy-MM-dd"),

                Status =
                    status
            }
        ];
    }
}