using System.Net.Http;
using System.Text.Json;

namespace MuniChien.Infrastructure.Api;

/// <summary>
/// Client HTTP indépendant des vues WPF et des services locaux.
/// Il ne fait aucune requête tant qu'une méthode GET n'est pas appelée.
/// Les réponses restent en JSON brut, car les modèles Swagger ne sont pas encore disponibles.
/// </summary>
public sealed class MuniChienApiClient : IDisposable
{
    public const string DefaultServerUrl = "http://192.168.0.98:5000/";

    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly TimeSpan _requestTimeout;
    private readonly Uri _serverAddress;

    /// <param name="serverUrl">Adresse racine du serveur, avec le port.</param>
    /// <param name="httpClient">Client injecté pour les tests ou la future composition.</param>
    /// <param name="requestTimeout">Temps maximal pour une requête, 15 secondes par défaut.</param>
    public MuniChienApiClient(
        string serverUrl = DefaultServerUrl,
        HttpClient? httpClient = null,
        TimeSpan? requestTimeout = null)
    {
        _serverAddress = NormalizeServerAddress(serverUrl);
        _requestTimeout = requestTimeout ?? DefaultRequestTimeout;

        if (_requestTimeout <= TimeSpan.Zero || _requestTimeout == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(requestTimeout),
                "Le délai d'attente doit être une durée positive et finie.");
        }

        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient is null;
    }

    public Uri ServerAddress => _serverAddress;

    /// <summary>GET /api/accueil — la structure JSON sera validée avec Maël.</summary>
    public Task<JsonElement> GetAccueilAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync("api/accueil", cancellationToken);

    /// <summary>GET /api/chiens — la structure JSON sera validée avec Maël.</summary>
    public Task<JsonElement> GetChiensAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync("api/chiens", cancellationToken);

    private async Task<JsonElement> GetJsonAsync(string relativePath, CancellationToken cancellationToken)
    {
        var endpoint = new Uri(_serverAddress, relativePath);

        // Le jeton externe est prioritaire : il permet d'annuler une navigation/requête.
        // Le jeton interne impose un délai même avec un HttpClient partagé.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_requestTimeout);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiHttpException(relativePath, response.StatusCode);
            }

            await using var stream = await response.Content
                .ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(
                stream, cancellationToken: timeoutCts.Token).ConfigureAwait(false);

            // Clone() garantit que le résultat reste valide après la fermeture du document.
            return document.RootElement.Clone();
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiConnectionException(
                $"Délai de connexion dépassé pour {relativePath}.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException(
                $"Impossible de joindre le serveur pour {relativePath}.", ex);
        }
        catch (IOException ex)
        {
            throw new ApiConnectionException(
                $"La lecture de la réponse a échoué pour {relativePath}.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiResponseException(
                $"La réponse de {relativePath} n'est pas un JSON valide.", ex);
        }
    }

    private static Uri NormalizeServerAddress(string serverUrl)
    {
        if (string.IsNullOrWhiteSpace(serverUrl)
            || !Uri.TryCreate(serverUrl.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "L'adresse doit être une URL HTTP(S) valide, sans identifiants, paramètres ni fragment.",
                nameof(serverUrl));
        }

        // Le slash final préserve un éventuel préfixe d'hébergement (ex. /munichien/).
        var builder = new UriBuilder(uri)
        {
            Path = uri.AbsolutePath.TrimEnd('/') + "/"
        };
        return builder.Uri;
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
