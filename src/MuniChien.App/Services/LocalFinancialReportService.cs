using MuniChien.App.ViewModels;
using System.Globalization;

namespace MuniChien.App.Services;

/// <summary>Rapports financiers locaux : filtres appliqués avant calcul des totaux.</summary>
public sealed class LocalFinancialReportService
{
    private static readonly CultureInfo FrenchCanadian = CultureInfo.GetCultureInfo("fr-CA");
    private readonly IPaymentService _paymentService;

    public LocalFinancialReportService(IPaymentService paymentService) => _paymentService = paymentService;

    public FinancialOverview GetOverview(string municipality)
    {
        var owners = GetOwners(municipality);
        return new FinancialOverview(
            owners.Sum(x => x.Payments.Sum(p => p.Amount)),
            owners.Sum(x => Math.Max(0m, x.Account.Balance)),
            owners.Sum(x => x.Account.LateFees));
    }

    public FinancialReportResult GenerateUnpaidBalances(ReportFilterCriteria c)
    {
        var entries = GetOwners(c.Municipality)
            .Where(x => ReportFilterRules.Search(c.SearchText, x.Owner.OwnerName,
                x.Owner.Phone, x.Owner.CellPhone, x.Owner.FileNumber))
            .Where(x => x.Account.Balance > 0m && ReportFilterRules.Minimum(x.Account.Balance, c.MinimumAmount))
            .ToList();
        var sorted = c.SortBy switch
        {
            "Client" => Sort(entries, x => x.Owner.OwnerName, c.Descending),
            "Municipalité" => Sort(entries, x => x.Owner.Municipality, c.Descending),
            _ => Sort(entries, x => x.Account.Balance, c.Descending)
        };
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Owner.FileNumber,
            Value2 = x.Owner.OwnerName,
            Value3 = x.Owner.Municipality,
            Value4 = Money(x.Account.AmountDue + x.Account.LateFees),
            Value5 = Money(x.Account.TotalPayments),
            Value6 = Money(x.Account.Balance)
        }).ToList();
        return new FinancialReportResult("Soldes impayés",
            "Situation actuelle des comptes locaux; paiements et frais inclus.",
            c.Describe(ReportKind.UnpaidBalances),
            ["Dossier", "Client", "Municipalité", "Facturé + frais", "Payé", "Solde"], rows,
            $"{rows.Count} dossier(s) avec solde · Total restant : {Money(entries.Sum(x => x.Account.Balance))}");
    }

    public FinancialReportResult GenerateLateFees(ReportFilterCriteria c)
    {
        var entries = GetOwners(c.Municipality)
            .Where(x => ReportFilterRules.Search(c.SearchText, x.Owner.OwnerName,
                x.Owner.Phone, x.Owner.CellPhone, x.Owner.FileNumber))
            .Where(x => x.Account.LateFees > 0m && ReportFilterRules.Minimum(x.Account.LateFees, c.MinimumAmount))
            .ToList();
        var sorted = c.SortBy switch
        {
            "Client" => Sort(entries, x => x.Owner.OwnerName, c.Descending),
            "Municipalité" => Sort(entries, x => x.Owner.Municipality, c.Descending),
            _ => Sort(entries, x => x.Account.LateFees, c.Descending)
        };
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Owner.FileNumber,
            Value2 = x.Owner.OwnerName,
            Value3 = x.Owner.Municipality,
            Value4 = Money(x.Account.LateFees),
            Value5 = Money(x.Account.TotalPayments),
            Value6 = Money(x.Account.Balance)
        }).ToList();
        return new FinancialReportResult("Frais de retard",
            "Frais enregistrés dans les comptes; le solde porte sur le compte entier.",
            c.Describe(ReportKind.LateFees),
            ["Dossier", "Client", "Municipalité", "Frais", "Payé (compte)", "Solde (compte)"], rows,
            $"{rows.Count} dossier(s) avec frais · Total des frais : {Money(entries.Sum(x => x.Account.LateFees))}");
    }

    public FinancialReportResult GeneratePayments(ReportFilterCriteria c)
    {
        var transactions = OrderTransactions(GetTransactions(c)
            .Where(x => ReportFilterRules.Period(x.Payment.PaymentDate, c.Period, DateTime.Today)), c).ToList();
        return new FinancialReportResult("Historique des paiements",
            "Paiements enregistrés dans les données locales de démonstration.",
            c.Describe(ReportKind.Payments),
            ["Date", "Reçu", "Client", "Municipalité", "Montant", "Chèque"],
            PaymentRows(transactions),
            $"{transactions.Count} paiement(s) · Total : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    public FinancialReportResult GenerateRevenue(ReportFilterCriteria c)
    {
        // Le filtrage s'applique aux transactions AVANT le regroupement mensuel.
        var transactions = GetTransactions(c)
            .Where(x => ReportFilterRules.Period(x.Payment.PaymentDate, c.Period, DateTime.Today))
            .ToList();
        var groups = transactions.GroupBy(x => new
        {
            x.Payment.PaymentDate.Year,
            x.Payment.PaymentDate.Month,
            x.Owner.Municipality
        }).ToList();
        var sorted = c.SortBy switch
        {
            "Montant" => Sort(groups, x => x.Sum(t => t.Payment.Amount), c.Descending),
            "Municipalité" => Sort(groups, x => x.Key.Municipality, c.Descending),
            _ => Sort(groups, x => x.Key.Year * 100 + x.Key.Month, c.Descending)
        };
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = $"{x.Key.Year:D4}-{x.Key.Month:D2}",
            Value2 = x.Key.Municipality,
            Value3 = x.Count().ToString(CultureInfo.InvariantCulture),
            Value4 = x.Select(p => p.Owner.OwnerId).Distinct().Count().ToString(CultureInfo.InvariantCulture),
            Value5 = Money(x.Sum(p => p.Payment.Amount))
        }).ToList();
        return new FinancialReportResult("Revenus enregistrés",
            "Sommaire mensuel des paiements filtrés; ceci n'est pas un bilan comptable officiel.",
            c.Describe(ReportKind.Revenue),
            ["Mois", "Municipalité", "Paiements", "Clients", "Montant", ""], rows,
            $"{transactions.Count} paiement(s) · Revenus enregistrés : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    public FinancialReportResult GenerateDailyPayments(ReportFilterCriteria c)
    {
        var transactions = OrderTransactions(GetTransactions(c)
            .Where(x => x.Payment.PaymentDate.Date == c.Day.Date), c).ToList();
        return new FinancialReportResult("Paiements journaliers",
            "Transactions de la date sélectionnée dans les données locales.",
            c.Describe(ReportKind.DailyPayments),
            ["Date", "Reçu", "Client", "Municipalité", "Montant", "Chèque"],
            PaymentRows(transactions),
            $"{transactions.Count} paiement(s) · Total du jour : {Money(transactions.Sum(x => x.Payment.Amount))}");
    }

    private static IEnumerable<FinancialTransaction> OrderTransactions(
        IEnumerable<FinancialTransaction> transactions, ReportFilterCriteria c) => c.SortBy switch
    {
        "Montant" => Sort(transactions, x => x.Payment.Amount, c.Descending),
        "Client" => Sort(transactions, x => x.Owner.OwnerName, c.Descending),
        "Reçu" => Sort(transactions, x => x.Payment.ReceiptNumber, c.Descending),
        _ => Sort(transactions, x => x.Payment.PaymentDate, c.Descending)
    };

    private static IEnumerable<T> Sort<T, TKey>(IEnumerable<T> source, Func<T, TKey> key, bool descending) =>
        descending ? source.OrderByDescending(key) : source.OrderBy(key);

    private static List<ReportPreviewRowViewModel> PaymentRows(IEnumerable<FinancialTransaction> transactions) =>
        transactions.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.Payment.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Value2 = x.Payment.ReceiptNumber,
            Value3 = x.Owner.OwnerName,
            Value4 = x.Owner.Municipality,
            Value5 = Money(x.Payment.Amount),
            Value6 = x.Payment.IsCheque ? "Oui" : "Non"
        }).ToList();

    private List<FinancialOwner> GetOwners(string municipality) =>
        _paymentService.GetOwnerOptions()
            .Where(x => ReportFilterRules.Choice(x.Municipality, municipality,
                ReportFilterCriteria.AllMunicipalities))
            .Select(x => new FinancialOwner(x,
                _paymentService.GetAccountSnapshot(x.OwnerId),
                _paymentService.GetPaymentsForOwner(x.OwnerId)))
            .ToList();

    private List<FinancialTransaction> GetTransactions(ReportFilterCriteria c) =>
        GetOwners(c.Municipality)
            .SelectMany(x => x.Payments.Select(p => new FinancialTransaction(x.Owner, p)))
            .Where(x => ReportFilterRules.Search(c.SearchText,
                x.Owner.OwnerName, x.Owner.Phone, x.Owner.CellPhone, x.Owner.FileNumber,
                x.Payment.ReceiptNumber, x.Payment.LicenseNumber))
            .Where(x => ReportFilterRules.Method(x.Payment.IsCheque, c.PaymentMethod))
            .Where(x => ReportFilterRules.Minimum(x.Payment.Amount, c.MinimumAmount))
            .ToList();

    private static string Money(decimal amount) => amount.ToString("C2", FrenchCanadian);

    private sealed record FinancialOwner(PaymentOwnerOption Owner,
        PaymentAccountSnapshot Account, IReadOnlyList<PaymentListItemViewModel> Payments);
    private sealed record FinancialTransaction(PaymentOwnerOption Owner, PaymentListItemViewModel Payment);
}

public sealed record FinancialOverview(decimal Received, decimal Outstanding, decimal LateFees);
public sealed record FinancialReportResult(string Title, string Description, string Period,
    IReadOnlyList<string> Headers, IReadOnlyList<ReportPreviewRowViewModel> Rows, string Summary);
