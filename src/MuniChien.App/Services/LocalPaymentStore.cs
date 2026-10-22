using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;


// ==================================================
// PROPRIÉTAIRE DISPONIBLE DANS PAIEMENTS
// ==================================================

public class PaymentOwnerOption
{
    public int OwnerId { get; init; }

    public string FileNumber { get; init; } =
        string.Empty;

    public string OwnerName { get; init; } =
        string.Empty;

    public string Municipality { get; init; } =
        string.Empty;


    public string DisplayName =>
        $"{OwnerName} — {Municipality}";


    public override string ToString()
    {
        return DisplayName;
    }
}

// ==================================================
// RÉSUMÉ DU COMPTE
// ==================================================

public class PaymentAccountSnapshot
{
    public decimal AmountDue { get; init; }

    public decimal LateFees { get; init; }

    public decimal TotalPayments { get; init; }

    public decimal Balance { get; init; }
}


// ==================================================
// STORE LOCAL
// ==================================================

public static class LocalPaymentStore
{
    private static readonly List<PaymentListItemViewModel>
        _payments =
        [];


    private static readonly Dictionary<int, decimal>
        _amountDueByOwner =
        [];


    private static readonly Dictionary<int, decimal>
        _lateFeesByOwner =
        [];


    // Pour éviter toute double facturation de la même licence locale.
    private static readonly HashSet<string> _billedLicenseNumbers =
        new(StringComparer.OrdinalIgnoreCase);

    private static int _nextReceiptNumber =
        1843;


    // ==================================================
    // INITIALISATION
    // ==================================================

    static LocalPaymentStore()
    {
        LoadExampleAccounts();

        LoadExamplePayments();
    }


    // ==================================================
    // PROPRIÉTAIRES
    // ==================================================

    public static IReadOnlyList<PaymentOwnerOption>
        GetOwnerOptions()
    {
        // Source unique : le même répertoire que Recherche et Propriétaires.
        // Les nouveaux propriétaires sans licence disposent d'un compte à zéro.
        return LocalAnimalDirectoryStore.GetOwners()
            .Select(owner => new PaymentOwnerOption
            {
                OwnerId = owner.OwnerId,
                FileNumber = owner.FileNumber,
                OwnerName = owner.FullName,
                Municipality = owner.Municipality
            })
            .OrderBy(owner => owner.OwnerName)
            .ToList();
    }


    // ==================================================
    // HISTORIQUE
    // ==================================================

    public static IReadOnlyList<PaymentListItemViewModel>
        GetPaymentsForOwner(
            int ownerId)
    {
        return _payments
            .Where(
                payment =>
                    payment.OwnerId ==
                    ownerId)
            .OrderByDescending(
                payment =>
                    payment.PaymentDate)
            .ThenByDescending(
                payment =>
                    payment.ReceiptNumber)
            .ToList();
    }


    // ==================================================
    // RÉSUMÉ
    // ==================================================

    public static PaymentAccountSnapshot
        GetAccountSnapshot(
            int ownerId)
    {
        decimal amountDue =
            _amountDueByOwner.GetValueOrDefault(
                ownerId,
                0m);


        decimal lateFees =
            _lateFeesByOwner.GetValueOrDefault(
                ownerId,
                0m);


        decimal totalPayments =
            _payments
                .Where(
                    payment =>
                        payment.OwnerId ==
                        ownerId)
                .Sum(
                    payment =>
                        payment.Amount);


        decimal balance =
            amountDue
            + lateFees
            - totalPayments;


        return new PaymentAccountSnapshot
        {
            AmountDue =
                amountDue,

            LateFees =
                lateFees,

            TotalPayments =
                totalPayments,

            Balance =
                balance
        };
    }


    // ==================================================
    // FACTURATION EXPLICITE D'UNE NOUVELLE LICENCE
    // ==================================================

    public static void AddLicenseCharge(int ownerId, string licenseNumber, decimal amount)
    {
        if (LocalAnimalDirectoryStore.GetOwner(ownerId) is null)
            throw new InvalidOperationException("Propriétaire introuvable.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("Numéro de licence requis.");
        if (amount <= 0m || amount > 100000m)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (!_billedLicenseNumbers.Add(licenseNumber))
            throw new InvalidOperationException("Cette licence a déjà été facturée.");
        _amountDueByOwner[ownerId] =
            _amountDueByOwner.GetValueOrDefault(ownerId, 0m) + amount;
    }


    // ==================================================
    // FACTURATION D'UN RENOUVELLEMENT
    // ==================================================

    public static void AddLicenseRenewalCharge(
        int ownerId, string licenseNumber, int coverageYear, decimal amount)
    {
        if (LocalAnimalDirectoryStore.GetOwner(ownerId) is null)
            throw new InvalidOperationException("Propriétaire introuvable.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("Numéro de licence requis.");
        if (coverageYear < 2000 || coverageYear > 9999)
            throw new ArgumentOutOfRangeException(nameof(coverageYear));
        if (amount <= 0m || amount > 100000m || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Montant de renouvellement invalide.");

        // Une clé distincte par licence et année de couverture :
        // ni le bouton double-cliqué ni un deuxième essai ne facturent à nouveau.
        string billingKey = $"RENOUVELLEMENT|{licenseNumber.Trim()}|{coverageYear}";
        if (!_billedLicenseNumbers.Add(billingKey))
            throw new InvalidOperationException(
                "Ce renouvellement a déjà été facturé pour cette année.");

        _amountDueByOwner[ownerId] =
            _amountDueByOwner.GetValueOrDefault(ownerId, 0m) + amount;
    }


    // ==================================================
    // AJOUTER UN PAIEMENT
    // ==================================================

    public static PaymentListItemViewModel
        AddPayment(
            int ownerId,
            DateTime paymentDate,
            decimal amount,
            bool isMunicipalityPayment,
            bool isCheque,
            string licenseNumber = "")
    {
        PaymentListItemViewModel payment =
            new()
            {
                OwnerId =
                    ownerId,

                ReceiptNumber =
                    $"#{_nextReceiptNumber++}",

                PaymentDate =
                    paymentDate.Date,

                Amount =
                    amount,

                IsMunicipalityPayment =
                    isMunicipalityPayment,

                IsCheque =
                    isCheque,

                LicenseNumber =
                    licenseNumber
            };


        _payments.Add(
            payment);


        return payment;
    }


    // ==================================================
    // NUMÉRO DE DOSSIER TEMPORAIRE
    //
    // Plus tard, il viendra directement de l'API.
    // ==================================================


    private static string BuildFileNumber(
        int ownerId)
    {
        if (ownerId == 1)
        {
            return "12345";
        }


        return (12344 + ownerId)
            .ToString();
    }


    // ==================================================
    // DONNÉES TEMPORAIRES DES COMPTES
    // ==================================================

    private static void LoadExampleAccounts()
    {
        _amountDueByOwner.Clear();

        _lateFeesByOwner.Clear();


        // Jean Tremblay
        _amountDueByOwner[1] =
            75m;

        _lateFeesByOwner[1] =
            10m;


        // Pierre Savard
        _amountDueByOwner[2] =
            60m;

        _lateFeesByOwner[2] =
            5m;


        // Sophie Gagnon
        _amountDueByOwner[3] =
            80m;

        _lateFeesByOwner[3] =
            0m;


        // Marc Bouchard
        _amountDueByOwner[4] =
            55m;

        _lateFeesByOwner[4] =
            0m;


        // Julie Fortin
        _amountDueByOwner[5] =
            65m;

        _lateFeesByOwner[5] =
            0m;


        // Luc Tremblay
        _amountDueByOwner[6] =
            70m;

        _lateFeesByOwner[6] =
            10m;


        // Nathalie Simard
        _amountDueByOwner[7] =
            50m;

        _lateFeesByOwner[7] =
            0m;
    }


    // ==================================================
    // DONNÉES TEMPORAIRES DES PAIEMENTS
    // ==================================================

    private static void LoadExamplePayments()
    {
        _payments.Clear();


        // Jean Tremblay :
        // total des paiements = 25 $
        // donc 75 + 10 - 25 = 60 $

        _payments.Add(
            new PaymentListItemViewModel
            {
                OwnerId = 1,

                ReceiptNumber = "#1842",

                PaymentDate =
                    new DateTime(
                        2026,
                        9,
                        4),

                Amount = 15m,

                IsMunicipalityPayment = false,

                IsCheque = true
            });


        _payments.Add(
            new PaymentListItemViewModel
            {
                OwnerId = 1,

                ReceiptNumber = "#1841",

                PaymentDate =
                    new DateTime(
                        2026,
                        9,
                        1),

                Amount = 10m,

                IsMunicipalityPayment = true,

                IsCheque = false
            });


        // Pierre Savard

        _payments.Add(
            new PaymentListItemViewModel
            {
                OwnerId = 2,

                ReceiptNumber = "#1839",

                PaymentDate =
                    new DateTime(
                        2026,
                        8,
                        20),

                Amount = 25m,

                IsMunicipalityPayment = false,

                IsCheque = true
            });
    }
}