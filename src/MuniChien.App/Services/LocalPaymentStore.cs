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
        return LocalLicenseStore.Licenses
            .GroupBy(
                license =>
                    new
                    {
                        license.OwnerId,
                        license.OwnerName
                    })
            .Select(
                group =>
                    new PaymentOwnerOption
                    {
                        OwnerId =
                            group.Key.OwnerId,

                        FileNumber =
                            BuildFileNumber(
                                group.Key.OwnerId),

                        OwnerName =
                            group.Key.OwnerName,

                        Municipality =
                            group
                                .Select(
                                    license =>
                                        license.Municipality)
                                .FirstOrDefault()
                            ?? "—"
                    })
            .OrderBy(
                owner =>
                    owner.OwnerName)
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