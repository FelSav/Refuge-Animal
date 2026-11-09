using System.Text.Json;
using MuniChien.Infrastructure.Api;

namespace MuniChien.Tests;

public sealed class ApiJsonProjectionTests
{
    [Fact]
    public void Accueil_PreserveLesNomsDeChampsSansSupposerDeContrat()
    {
        using var doc = JsonDocument.Parse("""{"chiensActifs":12,"licences":{"valides":8},"serveur":true}""");
        var result = ApiJsonProjection.Accueil(doc.RootElement);
        Assert.Contains(result.Summary, x => x.Name == "chiensActifs" && x.Value == "12");
        Assert.Contains(result.Summary, x => x.Name == "licences.valides" && x.Value == "8");
        Assert.Contains(result.Summary, x => x.Name == "serveur" && x.Value == "true");
    }

    [Fact]
    public void Chiens_AccepteUnTableauRacineSansDTO()
    {
        using var doc = JsonDocument.Parse("""[{"noDossier":7,"nom":"Rex"},{"noDossier":8,"nom":"Luna"}]""");
        var result = ApiJsonProjection.Chiens(doc.RootElement);
        Assert.True(result.HasRecordCollection);
        Assert.Equal(2, result.RecordCount);
        Assert.Contains(result.Records[0].Fields, x => x.Name == "nom" && x.Value == "Rex");
    }

    [Fact]
    public void Chiens_AccepteUnContenantDataEtNeRenommeAucunChamp()
    {
        using var doc = JsonDocument.Parse("""{"total":1,"data":{"chiens":[{"nom_du_chien":"Moka","proprietaire":{"ville":"Roberval"}}]}}""");
        var result = ApiJsonProjection.Chiens(doc.RootElement);
        Assert.Single(result.Records);
        Assert.Contains(result.Records[0].Fields, x => x.Name == "nom_du_chien" && x.Value == "Moka");
        Assert.Contains(result.Records[0].Fields, x => x.Name == "proprietaire.ville" && x.Value == "Roberval");
    }

    [Fact]
    public void Chiens_TableauVideAfficheZeroSansInventerDeChien()
    {
        using var doc = JsonDocument.Parse("""{"chiens":[]}""");
        var result = ApiJsonProjection.Chiens(doc.RootElement);
        Assert.True(result.HasRecordCollection);
        Assert.Equal(0, result.RecordCount);
        Assert.Empty(result.Records);
    }

    [Fact]
    public void Chiens_FormeInconnueSignaleUnContratAValider()
    {
        using var doc = JsonDocument.Parse("""{"erreur":"Format inattendu"}""");
        var result = ApiJsonProjection.Chiens(doc.RootElement);
        Assert.False(result.HasRecordCollection);
        Assert.Contains(result.Summary, x => x.Name == "erreur");
    }
}
