using System.Collections.Generic;

namespace MuniChien.App.Services;

/// <summary>
/// Valeurs affichées par le tableau de bord. Le futur service API
/// pourra remplacer l'implémentation locale sans changer la vue.
/// </summary>
public interface IDashboardStatisticsService
{
    IReadOnlyDictionary<string, string> GetMetricValues();
}
