using MuniChien.Infrastructure.Api;

namespace MuniChien.Tests;

public class ApiConnectionSessionTests
{
    private static string TempPath() => Path.Combine(Path.GetTempPath(), "munichien-session-tests-" + Guid.NewGuid().ToString("N"), "connexion-api.json");

    [Fact]
    public void DemarrageToujoursEnModeApiEtNonEnDemo()
    {
        var store = new ApiConnectionSettingsStore(TempPath());
        var session = new ApiConnectionSession(store, (_, _) => Task.CompletedTask);
        Assert.True(session.IsApiMode);
        Assert.False(session.IsDemoMode);
        session.ActivateDemo();
        Assert.True(session.IsDemoMode);
        var freshSession = new ApiConnectionSession(store, (_, _) => Task.CompletedTask);
        Assert.True(freshSession.IsApiMode); // jamais persisté
        Assert.False(freshSession.IsDemoMode);
    }

    [Fact]
    public async Task RequeteReussieIndiqueEnLigne()
    {
        var session = new ApiConnectionSession(new ApiConnectionSettingsStore(TempPath()), (_, _) => Task.CompletedTask);
        await session.ProbeAsync();
        Assert.Equal(ApiProbeState.Online, session.ProbeState);
    }

    [Fact]
    public async Task ServeurInaccessibleNeDeclenchePasLeModeDemo()
    {
        var session = new ApiConnectionSession(new ApiConnectionSettingsStore(TempPath()),
            (_, _) => throw new ApiConnectionException("échec simulé", new HttpRequestException()));
        await session.ProbeAsync();
        Assert.Equal(ApiProbeState.Offline, session.ProbeState);
        Assert.True(session.IsApiMode);
        Assert.False(session.IsDemoMode);
    }

    [Fact]
    public void EnregistrerAdresseNeMemorisePasModeDemo()
    {
        var path = TempPath();
        try
        {
            var store = new ApiConnectionSettingsStore(path);
            var session = new ApiConnectionSession(store, (_, _) => Task.CompletedTask);
            session.SaveServerUrl("http://localhost:5010");
            session.ActivateDemo();
            var fresh = new ApiConnectionSession(store, (_, _) => Task.CompletedTask);
            Assert.Equal("http://localhost:5010/", fresh.ServerUrl);
            Assert.False(fresh.IsDemoMode);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }
}
