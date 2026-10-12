namespace MuniChien.App.Services;

/// <summary>
/// Point de composition du frontend. Permet de choisir la source de données
/// sans que la navigation connaisse les classes Local* concrètes.
///
/// Les contrats actuels sont synchrones : la future intégration HTTP devra
/// introduire des appels asynchrones, annulation et gestion des erreurs.
/// </summary>
public interface IApplicationServices
{
    IAnimalDirectoryService Animals { get; }
    ILicenseService Licenses { get; }
    IPaymentService Payments { get; }
    INoticeService Notices { get; }
    IDashboardStatisticsService Dashboard { get; }
}
