using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MuniChien.Infrastructure.Api;

public enum ApiProbeState { NotTested, Checking, Online, Offline }

/// <summary>
/// État de connexion partagé par l'application. Le mode API est TOUJOURS celui du démarrage.
/// Le mode démo est une exception temporaire, jamais sauvegardée sur disque.
/// Cette étape ne mappe pas encore les données JSON aux ViewModels.
/// </summary>
public sealed class ApiConnectionSession : INotifyPropertyChanged
{
    private readonly ApiConnectionSettingsStore _settingsStore;
    private readonly Func<string, CancellationToken, Task> _probe;
    private string _serverUrl;
    private ApiProbeState _probeState = ApiProbeState.NotTested;
    private string _status = "Connexion non encore vérifiée.";
    private bool _demoMode;
    private bool _isChecking;

    public ApiConnectionSession(
        ApiConnectionSettingsStore? settingsStore = null,
        Func<string, CancellationToken, Task>? probe = null)
    {
        _settingsStore = settingsStore ?? new ApiConnectionSettingsStore();
        try
        {
            _serverUrl = _settingsStore.LoadServerUrl();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                   or System.Text.Json.JsonException)
        {
            _serverUrl = MuniChienApiClient.DefaultServerUrl;
            _status = "Configuration API illisible : vérifier l'adresse dans Administration.";
        }
        _probe = probe ?? ProbeEndpointAsync;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public string ServerUrl => _serverUrl;
    public ApiProbeState ProbeState => _probeState;
    public string StatusMessage => _status;
    public bool IsDemoMode => _demoMode;
    public bool IsApiMode => !_demoMode;
    public bool IsChecking => _isChecking;
    public string ModeDisplay => _demoMode ? "DÉMONSTRATION — données fictives" : "API — mode normal";

    public void ActivateDemo()
    {
        if (_demoMode) return;
        _demoMode = true;
        OnPropertyChanged(nameof(IsDemoMode));
        OnPropertyChanged(nameof(IsApiMode));
        OnPropertyChanged(nameof(ModeDisplay));
    }

    public void ReturnToApi()
    {
        if (!_demoMode) return;
        _demoMode = false;
        OnPropertyChanged(nameof(IsDemoMode));
        OnPropertyChanged(nameof(IsApiMode));
        OnPropertyChanged(nameof(ModeDisplay));
    }

    public void SaveServerUrl(string serverUrl)
    {
        _settingsStore.SaveServerUrl(serverUrl); // valide avant d'écrire
        _serverUrl = _settingsStore.LoadServerUrl();
        SetProbeState(ApiProbeState.NotTested, "Adresse enregistrée. Nouvelle vérification en attente.");
        OnPropertyChanged(nameof(ServerUrl));
    }

    public async Task ProbeAsync(CancellationToken cancellationToken = default)
    {
        if (_isChecking) return;
        _isChecking = true;
        OnPropertyChanged(nameof(IsChecking));
        string address = _serverUrl;
        SetProbeState(ApiProbeState.Checking, "Vérification automatique de /api/accueil…");
        try
        {
            await _probe(address, cancellationToken);
            if (cancellationToken.IsCancellationRequested || address != _serverUrl) return;
            SetProbeState(ApiProbeState.Online,
                "API joignable : /api/accueil retourne un JSON valide. Les pages métier restent à intégrer.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (address == _serverUrl)
                SetProbeState(ApiProbeState.NotTested, "Vérification annulée.");
        }
        catch (ApiHttpException ex)
        {
            if (address == _serverUrl)
                SetProbeState(ApiProbeState.Offline,
                    $"Le serveur répond HTTP {(int)ex.StatusCode} pour /api/accueil. Vérifier la route ou l'autorisation.");
        }
        catch (Exception ex) when (ex is ApiConnectionException or ApiResponseException or
                                   HttpRequestException or TimeoutException or IOException)
        {
            if (address == _serverUrl)
                SetProbeState(ApiProbeState.Offline, "API inaccessible ou réponse invalide : " + ex.Message);
        }
        catch (Exception ex)
        {
            if (address == _serverUrl)
                SetProbeState(ApiProbeState.Offline, "Vérification échouée : " + ex.Message);
        }
        finally
        {
            _isChecking = false;
            OnPropertyChanged(nameof(IsChecking));
        }
    }

    private static async Task ProbeEndpointAsync(string url, CancellationToken cancellationToken)
    {
        using var api = new MuniChienApiClient(url, requestTimeout: TimeSpan.FromSeconds(8));
        await api.GetAccueilAsync(cancellationToken);
    }

    private void SetProbeState(ApiProbeState state, string status)
    {
        _probeState = state;
        _status = status;
        OnPropertyChanged(nameof(ProbeState));
        OnPropertyChanged(nameof(StatusMessage));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
