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

    public static LicenseListItemViewModel?
        RenewLicense(
            string licenseNumber,
            DateTime renewalDate,
            bool extendToNextYear = false)
    {
        LicenseListItemViewModel? license =
            GetCurrentLicense(
                licenseNumber);


        if (license is null)
        {
            return null;
        }


        DateTime newExpiration =
            CalculateExpiration(
                renewalDate,
                extendToNextYear);
        // ----------------------------------------------
        // ARCHIVER L'ANCIENNE PÉRIODE
        // ----------------------------------------------

        if (!_history.TryGetValue(
                license.LicenseNumber,
                out List<DogLicenseHistoryViewModel>? history))
        {
            history =
                [];

            _history[license.LicenseNumber] =
                history;
        }


        if (history.Count > 0)
        {
            DogLicenseHistoryViewModel previous =
                history[^1];


            history[^1] =
                new DogLicenseHistoryViewModel
                {
                    LicenseNumber =
                        previous.LicenseNumber,

                    IssueDate =
                        previous.IssueDate,

                    ExpirationDate =
                        previous.ExpirationDate,

                    Status =
                        "Renouvelée"
                };
        }
        else
        {
            history.Add(
                new DogLicenseHistoryViewModel
                {
                    LicenseNumber =
                        license.LicenseNumber,

                    IssueDate =
                        license.IssueDate.ToString(
                            "yyyy-MM-dd"),

                    ExpirationDate =
                        license.ExpirationDate.ToString(
                            "yyyy-MM-dd"),

                    Status =
                        "Renouvelée"
                });
        }


        // ----------------------------------------------
        // NOUVELLE PÉRIODE
        // ----------------------------------------------

        license.IssueDate =
            renewalDate.Date;

        license.ExpirationDate =
            newExpiration;

        license.Status =
            "Valide";


        history.Add(
            new DogLicenseHistoryViewModel
            {
                LicenseNumber =
                    license.LicenseNumber,

                IssueDate =
                    license.IssueDate.ToString(
                        "yyyy-MM-dd"),

                ExpirationDate =
                    license.ExpirationDate.ToString(
                        "yyyy-MM-dd"),

                Status =
                    "Valide"
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