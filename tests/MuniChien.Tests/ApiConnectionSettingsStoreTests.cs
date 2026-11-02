using MuniChien.Infrastructure.Api;

namespace MuniChien.Tests;

public class ApiConnectionSettingsStoreTests
{
    [Fact]
    public void SansFichier_RetourneAdresseParDefaut()
    {
        string file = TemporaryPath();
        var store = new ApiConnectionSettingsStore(file);
        Assert.Equal(MuniChienApiClient.DefaultServerUrl, store.LoadServerUrl());
    }

    [Fact]
    public void EnregistrementPuisLecture_RetrouveAdresseNormalisee()
    {
        string file = TemporaryPath();
        try
        {
            var store = new ApiConnectionSettingsStore(file);
            store.SaveServerUrl("http://127.0.0.1:5001");
            Assert.Equal("http://127.0.0.1:5001/", store.LoadServerUrl());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }

    [Fact]
    public void UneAdresseInvalide_NeCreePasDeFichier()
    {
        string file = TemporaryPath();
        var store = new ApiConnectionSettingsStore(file);
        Assert.Throws<ArgumentException>(() => store.SaveServerUrl("ftp://127.0.0.1"));
        Assert.False(File.Exists(file));
    }

    [Fact]
    public void FichierEndommage_ProduitErreurClaire()
    {
        string file = TemporaryPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, "{pas du json}");
            var store = new ApiConnectionSettingsStore(file);
            Assert.Throws<System.Text.Json.JsonException>(() => store.LoadServerUrl());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }

    private static string TemporaryPath() => Path.Combine(
        Path.GetTempPath(), "munichien-api-tests-" + Guid.NewGuid().ToString("N"),
        "connexion-api.json");
}
