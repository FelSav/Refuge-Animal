using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public interface ILicenseService
{
    IReadOnlyList<LicenseListItemViewModel> GetLicenses();

    // Création locale : tarif explicite validé par l'employé, pas de prix inventé.
    LicenseListItemViewModel CreateLicense(int dogId, DateTime issueDate, decimal amountToBill);

    LicenseListItemViewModel? GetCurrentLicense(
        string licenseNumber);

    IReadOnlyList<DogLicenseHistoryViewModel> GetHistory(
        string licenseNumber);

    DateTime CalculateExpiration(
        DateTime renewalDate,
        bool extendToNextYear = false);

    LicenseListItemViewModel? RenewLicense(
        string licenseNumber,
        DateTime renewalDate,
        bool extendToNextYear = false);
}