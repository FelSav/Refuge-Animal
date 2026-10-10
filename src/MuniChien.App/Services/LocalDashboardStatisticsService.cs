using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

/// <summary>
/// Indicateurs calculés depuis les services de démonstration partagés.
/// Les comptes de paiement locaux ne sont pas ventilés par année de licence.
/// Ils ne doivent jamais être présentés comme des données officielles.
/// </summary>
public sealed class LocalDashboardStatisticsService : IDashboardStatisticsService
{
    private static readonly CultureInfo FrenchCanada = CultureInfo.GetCultureInfo("fr-CA");

    private readonly IAnimalDirectoryService _directory;
    private readonly ILicenseService _licenses;
    private readonly IPaymentService _payments;

    public LocalDashboardStatisticsService(
        IAnimalDirectoryService? directory = null,
        ILicenseService? licenses = null,
        IPaymentService? payments = null)
    {
        _directory = directory ?? new LocalAnimalDirectoryService();
        _licenses = licenses ?? new LocalLicenseService();
        _payments = payments ?? new LocalPaymentService();
    }

    public IReadOnlyDictionary<string, string> GetMetricValues()
    {
        DateTime today = DateTime.Today;
        DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
        DateTime nextMonth = startOfMonth.AddMonths(1);

        IReadOnlyList<OwnerDogViewModel> dogs = _directory.GetDogs();
        IReadOnlyList<OwnerDirectoryItem> owners = _directory.GetOwners();
        IReadOnlyList<LicenseListItemViewModel> licenses = _licenses.GetLicenses();

        // Un propriétaire peut posséder plusieurs licences : on calcule
        // une seule fois son compte et son historique de paiements.
        int[] paymentOwnerIds = _payments.GetOwnerOptions()
            .Select(owner => owner.OwnerId)
            .Distinct()
            .ToArray();

        int unpaidOwners = paymentOwnerIds.Count(ownerId =>
            _payments.GetAccountSnapshot(ownerId).Balance > 0m);

        List<PaymentListItemViewModel> allPayments = paymentOwnerIds
            .SelectMany(ownerId => _payments.GetPaymentsForOwner(ownerId))
            .ToList();

        List<PaymentListItemViewModel> monthlyPayments = allPayments
            .Where(payment => payment.PaymentDate.Date >= startOfMonth &&
                              payment.PaymentDate.Date < nextMonth)
            .ToList();

        int paymentsToday = allPayments.Count(payment =>
            payment.PaymentDate.Date == today);

        decimal revenueThisMonth = monthlyPayments.Sum(payment => payment.Amount);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["activeDogs"] = Count(dogs.Count(dog => IsStatus(dog.Status, "Actif"))),
            ["inactiveDogs"] = Count(dogs.Count(dog => IsStatus(dog.Status, "Inactif"))),
            ["dogsWithoutLicense"] = Count(dogs.Count(dog => !HasKnownLicense(dog, licenses))),
            ["licensesToRenew"] = Count(licenses.Count(license => IsStatus(license.Status, "À renouveler"))),
            ["expiredLicenses"] = Count(licenses.Count(license => IsStatus(license.Status, "Expirée"))),
            ["unpaidBalances"] = Count(unpaidOwners),
            ["paymentsToday"] = Count(paymentsToday),
            ["paymentsThisMonth"] = Count(monthlyPayments.Count),
            ["monthlyRevenue"] = revenueThisMonth.ToString("N2", FrenchCanada) + " $",
            ["activeOwners"] = Count(owners.Count(owner => IsStatus(owner.Status, "Actif"))),
            ["inactiveOwners"] = Count(owners.Count(owner => IsStatus(owner.Status, "Inactif"))),

            // L'admissibilité aux avis dépend d'impayés PAR ANNÉE de licence
            // et d'une campagne précise; la version locale ne possède pas
            // ces informations. Ne surtout pas inventer une valeur.
            ["noticesToSend"] = "—"
        };

        return values;
    }

    private static bool HasKnownLicense(
        OwnerDogViewModel dog,
        IReadOnlyList<LicenseListItemViewModel> licenses)
    {
        if (string.IsNullOrWhiteSpace(dog.LicenseNumber) ||
            dog.LicenseNumber.Trim() == "—" || dog.LicenseNumber.Trim() == "0")
        {
            return false;
        }

        return licenses.Any(license =>
            license.DogId == dog.DogId &&
            license.LicenseNumber.Equals(dog.LicenseNumber,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsStatus(string? value, string status) =>
        string.Equals(value?.Trim(), status, StringComparison.OrdinalIgnoreCase);

    private static string Count(int value) => value.ToString("N0", FrenchCanada);
}
