using System.Net;
using System.Net.Http;
using System.Text;
using MuniChien.Infrastructure.Api;

namespace MuniChien.Tests;

public class MuniChienApiClientTests
{
    [Fact]
    public async Task Accueil_EnvoieGetEtRetourneUnJson()
    {
        using var http = new HttpClient(new FakeHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://192.168.0.98:5000/api/accueil", request.RequestUri!.ToString());
            Assert.Contains(request.Headers.Accept, h => h.MediaType == "application/json");
            return Task.FromResult(JsonResponse("{\"chiensActifs\":12}"));
        }));
        using var api = new MuniChienApiClient(httpClient: http);

        var result = await api.GetAccueilAsync();

        Assert.Equal(12, result.GetProperty("chiensActifs").GetInt32());
    }

    [Fact]
    public async Task Chiens_UtiliseLaBonneRouteEtRetourneLeTableau()
    {
        using var http = new HttpClient(new FakeHandler((request, _) =>
        {
            Assert.Equal("/api/chiens", request.RequestUri!.AbsolutePath);
            return Task.FromResult(JsonResponse("[{\"id\":1},{\"id\":2}]"));
        }));
        using var api = new MuniChienApiClient(httpClient: http);

        var result = await api.GetChiensAsync();

        Assert.Equal(2, result.GetArrayLength());
    }

    [Fact]
    public async Task Reponse404_DonneUneErreurHttpClaire()
    {
        using var http = new HttpClient(new FakeHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))));
        using var api = new MuniChienApiClient(httpClient: http);

        var error = await Assert.ThrowsAsync<ApiHttpException>(() => api.GetChiensAsync());

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("api/chiens", error.Endpoint);
    }

    [Fact]
    public async Task ServeurInjoignable_DonneUneErreurConnexion()
    {
        using var http = new HttpClient(new FakeHandler((_, _) =>
            throw new HttpRequestException("Simulation : serveur inaccessible")));
        using var api = new MuniChienApiClient(httpClient: http);

        await Assert.ThrowsAsync<ApiConnectionException>(() => api.GetAccueilAsync());
    }

    [Fact]
    public async Task ReponseNonJson_DonneUneErreurReponse()
    {
        using var http = new HttpClient(new FakeHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("pas du json", Encoding.UTF8, "text/plain")
            })));
        using var api = new MuniChienApiClient(httpClient: http);

        await Assert.ThrowsAsync<ApiResponseException>(() => api.GetChiensAsync());
    }

    [Fact]
    public async Task AnnulationDemandee_RespecteLeJetonExterne()
    {
        using var http = new HttpClient(new FakeHandler((_, token) =>
            Task.FromCanceled<HttpResponseMessage>(token)));
        using var api = new MuniChienApiClient(httpClient: http);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => api.GetAccueilAsync(cts.Token));
    }

    [Theory]
    [InlineData("ftp://192.168.0.98:5000")]
    [InlineData("192.168.0.98:5000")]
    [InlineData("http://user:password@192.168.0.98:5000")]
    public void AdresseInvalide_EstRejetee(string url)
    {
        Assert.Throws<ArgumentException>(() => new MuniChienApiClient(url));
    }

    [Fact]
    public async Task PrefixeDeServeur_EstConserve()
    {
        using var http = new HttpClient(new FakeHandler((request, _) =>
        {
            Assert.Equal("/munichien/api/accueil", request.RequestUri!.AbsolutePath);
            return Task.FromResult(JsonResponse("{}"));
        }));
        using var api = new MuniChienApiClient("https://exemple.test/munichien", http);

        await api.GetAccueilAsync();
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
