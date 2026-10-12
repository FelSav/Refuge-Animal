namespace MuniChien.App.Services;

/// <summary>
/// Implémentation de démonstration. Tous les écrans utilisent les mêmes
/// services pour une session, sans accéder à une API ou une base officielle.
/// </summary>
public sealed class LocalApplicationServices : IApplicationServices
{
    public LocalApplicationServices()
    {
        Animals = new LocalAnimalDirectoryService();
        Licenses = new LocalLicenseService();
        Payments = new LocalPaymentService();
        Notices = new LocalNoticeService();
        Dashboard = new LocalDashboardStatisticsService(Animals, Licenses, Payments);
    }

    public IAnimalDirectoryService Animals { get; }
    public ILicenseService Licenses { get; }
    public IPaymentService Payments { get; }
    public INoticeService Notices { get; }
    public IDashboardStatisticsService Dashboard { get; }
}
