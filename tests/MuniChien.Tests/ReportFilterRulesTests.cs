using MuniChien.App.Services;

namespace MuniChien.Tests;

public class ReportFilterRulesTests
{
    [Fact]
    public void Recherche_Race_Golden_Retrouve_GoldenRetriever()
    {
        Assert.True(ReportFilterRules.Search("Golden", "Golden Retriever"));
        Assert.False(ReportFilterRules.Search("Golden", "Husky"));
    }

    [Fact]
    public void Recherche_Nom_Ignore_Accents_Et_Casse()
    {
        Assert.True(ReportFilterRules.Search("elodie", "Élodie Côté"));
    }

    [Fact]
    public void Recherche_Telephone_Ignore_La_Ponctuation()
    {
        Assert.True(ReportFilterRules.Search("4185551234", "(418) 555-1234"));
        Assert.True(ReportFilterRules.Search("1234", "(418) 555-1234"));
    }

    [Fact]
    public void Race_Selectionnee_Exclut_Les_Autres()
    {
        Assert.True(ReportFilterRules.Choice("Golden Retriever", "Golden Retriever",
            ReportFilterCriteria.AllBreeds));
        Assert.False(ReportFilterRules.Choice("Husky", "Golden Retriever",
            ReportFilterCriteria.AllBreeds));
        Assert.True(ReportFilterRules.Choice("Husky", ReportFilterCriteria.AllBreeds,
            ReportFilterCriteria.AllBreeds));
    }

    [Fact]
    public void Periode_Sur_Date_Respecte_Les_Bornes()
    {
        var today = new DateTime(2026, 10, 24);
        Assert.True(ReportFilterRules.Period(new DateTime(2026, 10, 18),
            "7 derniers jours", today));
        Assert.False(ReportFilterRules.Period(new DateTime(2026, 10, 17),
            "7 derniers jours", today));
        Assert.False(ReportFilterRules.Period(new DateTime(2025, 10, 24),
            "Cette année", today));
        Assert.True(ReportFilterRules.Period(new DateTime(2026, 9, 1),
            "Toutes", today));
    }

    [Fact]
    public void Modes_De_Paiement_Ne_Se_Melangent_Pas()
    {
        Assert.True(ReportFilterRules.Method(true, "Chèque"));
        Assert.False(ReportFilterRules.Method(false, "Chèque"));
        Assert.True(ReportFilterRules.Method(false, "Autre mode"));
    }

    [Fact]
    public void Montant_Minimum_S_Applique_Aux_Totaux_Filtres()
    {
        Assert.True(ReportFilterRules.Minimum(50m, 25m));
        Assert.False(ReportFilterRules.Minimum(24.99m, 25m));
        Assert.True(ReportFilterRules.Minimum(0m, null));
    }
}
