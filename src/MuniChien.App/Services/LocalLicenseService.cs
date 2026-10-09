using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public class LocalLicenseService : ILicenseService
{
    public IReadOnlyList<LicenseListItemViewModel> GetLicenses()
    {
        return LocalLicenseStore.Licenses;
    }


    public LicenseListItemViewModel? GetCurrentLicense(
        string licenseNumber)
    {
        return LocalLicenseStore.GetCurrentLicense(
            licenseNumber);
    }


    public IReadOnlyList<DogLicenseHistoryViewModel> GetHistory(
        string licenseNumber)
    {
        return LocalLicenseStore.GetHistory(
            licenseNumber);
    }


    public DateTime CalculateExpiration(
        DateTime renewalDate,
        bool extendToNextYear = false)
    {
        return LocalLicenseStore.CalculateExpiration(
            renewalDate,
            extendToNextYear);
    }

    public LicenseListItemViewModel? RenewLicense(
        string licenseNumber,
        DateTime renewalDate,
        bool extendToNextYear = false)
    {
        return LocalLicenseStore.RenewLicense(
            licenseNumber,
            renewalDate,
            extendToNextYear);
    }
}