using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public interface ILicenseService
{
    IReadOnlyList<LicenseListItemViewModel> GetLicenses();

    LicenseListItemViewModel? GetCurrentLicense(
        string licenseNumber);

    IReadOnlyList<DogLicenseHistoryViewModel> GetHistory(
        string licenseNumber);

    DateTime CalculateExpiration(
        DateTime renewalDate);

    LicenseListItemViewModel? RenewLicense(
        string licenseNumber,
        DateTime renewalDate);
}