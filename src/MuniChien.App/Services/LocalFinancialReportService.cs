using MuniChien.App.ViewModels;
using System.Globalization;

namespace MuniChien.App.Services;

/// <summary>
/// Génération des rapports FINANCIERS de démonstration.
/// Toutes les sommes proviennent de IPaymentService : aucun tarif n'est déduit
/// des licences et aucun historique de facturation annuel n'est inventé.
/// </summary>
public sealed class LocalFinancialReportService
{
    private static readonly CultureInfo FrenchCanadian =
        CultureInfo.GetCultureInfo("fr-CA");

    private readonly IPaymentService _paymentService;

    public LocalFinancialReportService(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public FinancialOverview GetOverview(string municipality)
    {
        var owners = GetOwners(municipality);
        decimal balance = owners.Sum(x => Math.Max(0m, x.Account.Balance));
        decimal lateFees = owners.Sum(x => x.Account.LateFees);
        decimal received = owners.Sum(x => x.Payments.Sum(p => p.Amount));

        return new FinancialOverview(received, balance, lateFees);
    }

    public FinancialReportResult GenerateUnpaidBalances(string municipality)
    {
        var entries = GetOwners(municipality)
            .Where(x => x.Account.Balance > 0m)
            .OrderByDescending(x => x.Account.Balance)
            .ThenBy(x => x.Owner.OwnerName)
            .ToList();

        var rows = entries.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Owner.FileNumber,
            Value2 = x.Owner.OwnerName,
            Value3 = x.Owner.Municipality,
            Value4 = Money(x.Account.AmountDue + x.Account.LateFees),
            Value5 = Money(x.Account.TotalPayments),
            Value6 = Money(x.Account.Balance)
        }).ToList();

        return new FinancialReportResult(
            "Soldes impayés",
            "Situation actuelle des comptes locaux. Le montant facturé inclut les frais de retard; les paiements couvrent le compte entier.",
            "Situation actuelle — filtre de période non applicable",
            ["Dossier", "Propriétaire", "Municipalité", "Facturé + frais", "Payé", "Solde"],
            rows,
            $"{entries.Count} dossier(s) avec solde · Total restant : {Money(entries.Sum(x => x.Account.Balance))}");
    }

    public FinancialReportResult GenerateLateFees(string municipality)
    {
        var entries = GetOwners(municipality)
            .Where(x => x.Account.LateFees > 0m)
            .OrderByDescending(x => x.Account.LateFees)
            .ThenBy(x => x.Owner.OwnerName)
            .ToList();

        var rows = entries.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Owner.FileNumber,
            Value2 = x.Owner.OwnerName,
            Value3 = x.Owner.Municipality,
            Value4 = Money(x.Account.LateFees),
            Value5 = Money(x.Account.TotalPayments),
            Value6 = Money(x.Account.Balance)
        }).ToList();

        return new FinancialReportResult(
            "Frais de retard",
            "Frais enregistrés dans les comptes locaux. Les paiements et soldes portent sur le compte complet, pas seulement sur les pénalités.",
            "Situation actuelle — filtre de période non applicable",
            ["Dossier", "Propriétaire", "Municipalité", "Frais", "Payé (compte)", "Solde (compte)"],
            rows,
            $"{entries.Count} dossier(s) avec frais · Frais inscrits : {Money(entries.Sum(x => x.Account.LateFees))}");
    }

    public FinancialReportResult GeneratePayments(string municipality, string period)
    {
        var transactions = GetTransactions(municipality)
            .Where(x => MatchesPeriod(x.Payment.PaymentDate, period))
            .OrderByDescending(x => x.Payment.PaymentDate)
            .ThenByDescending(x => x.Payment.ReceiptNumber)
            .ToList();

        return new FinancialReportResult(
            "Historique des paiements",
            "Paiements enregistrés dans la démonstration locale (chèque : oui/non selon la saisie).",
            period,
            ["Date", "Reçu", "Propriétaire", "Municipalité", "Montant", "Chèque"],
            PaymentRows(transactions),
            $"{transactions.Count} paiement(s) · Total : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    public FinancialReportResult GenerateRevenue(string municipality, string period)
    {
        var transactions = GetTransactions(municipality)
            .Where(x => MatchesPeriod(x.Payment.PaymentDate, period))
            .ToList();

        var rows = transactions
            .GroupBy(x => new
            {
                Year = x.Payment.PaymentDate.Year,
                Month = x.Payment.PaymentDate.Month,
                Municipality = x.Owner.Municipality
            })
            .OrderByDescending(x => x.Key.Year)
            .ThenByDescending(x => x.Key.Month)
            .ThenBy(x => x.Key.Municipality)
            .Select(x => new ReportPreviewRowViewModel
            {
                Value1 = $"{x.Key.Year:D4}-{x.Key.Month:D2}",
                Value2 = x.Key.Municipality,
                Value3 = x.Count().ToString(CultureInfo.InvariantCulture),
                Value4 = x.Select(p => p.Owner.OwnerId).Distinct().Count()
                    .ToString(CultureInfo.InvariantCulture),
                Value5 = Money(x.Sum(p => p.Payment.Amount))
            })
            .ToList();

        return new FinancialReportResult(
            "Revenus enregistrés",
            "Sommaire des paiements locaux groupés par mois et municipalité. Il ne s'agit pas d'un bilan comptable officiel.",
            period,
            ["Mois", "Municipalité", "Paiements", "Propriétaires", "Montant", ""],
            rows,
            $"{transactions.Count} paiement(s) · Revenus enregistrés : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    public FinancialReportResult GenerateDailyPayments(string municipality, DateTime day)
    {
        var transactions = GetTransactions(municipality)
            .Where(x => x.Payment.PaymentDate.Date == day.Date)
            .OrderBy(x => x.Owner.OwnerName)
            .ThenBy(x => x.Payment.ReceiptNumber)
            .ToList();

        return new FinancialReportResult(
            "Paiements journaliers",
            "Transactions enregistrées à la date sélectionnée dans les données locales.",
            $"Journée du {day:yyyy-MM-dd}",
            ["Date", "Reçu", "Propriétaire", "Municipalité", "Montant", "Chèque"],
            PaymentRows(transactions),
            $"{transactions.Count} paiement(s) · Total du jour : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    private static List<ReportPreviewRowViewModel> PaymentRows(
        IEnumerable<FinancialTransaction> transactions)
    {
        return transactions.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Payment.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Value2 = x.Payment.ReceiptNumber,
            Value3 = x.Owner.OwnerName,
            Value4 = x.Owner.Municipality,
            Value5 = Money(x.Payment.Amount),
            Value6 = x.Payment.IsCheque ? "Oui" : "Non"
        }).ToList();
    }

    private List<FinancialOwner> GetOwners(string municipality)
    {
        return _paymentService.GetOwnerOptions()
            .Where(x => municipality == "Toutes les municipalités" ||
                        string.Equals(x.Municipality, municipality, StringComparison.OrdinalIgnoreCase))
            .Select(x => new FinancialOwner(
                x,
                _paymentService.GetAccountSnapshot(x.OwnerId),
                _paymentService.GetPaymentsForOwner(x.OwnerId)))
            .ToList();
    }

    private List<FinancialTransaction> GetTransactions(string municipality)
    {
        return GetOwners(municipality)
            .SelectMany(x => x.Payments.Select(p => new FinancialTransaction(x.Owner, p)))
            .ToList();
    }

    private static bool MatchesPeriod(DateTime date, string period)
    {
        DateTime today = DateTime.Today;
        DateTime day = date.Date;
        return period switch
        {
            "Aujourd'hui" => day == today,
            "7 derniers jours" => day >= today.AddDays(-6) && day <= today,
            "Ce mois" => day.Year == today.Year && day.Month == today.Month,
            "Cette année" => day.Year == today.Year,
            _ => true
        };
    }

    private static string Money(decimal amount) => amount.ToString("C2", FrenchCanadian);

    private sealed record FinancialOwner(
        PaymentOwnerOption Owner,
        PaymentAccountSnapshot Account,
        IReadOnlyList<PaymentListItemViewModel> Payments);

    private sealed record FinancialTransaction(
        PaymentOwnerOption Owner,
        PaymentListItemViewModel Payment);
}

public sealed record FinancialOverview(decimal Received, decimal Outstanding, decimal LateFees);

public sealed record FinancialReportResult(
    string Title,
    string Description,
    string Period,
    IReadOnlyList<string> Headers,
    IReadOnlyList<ReportPreviewRowViewModel> Rows,
    string Summary);
