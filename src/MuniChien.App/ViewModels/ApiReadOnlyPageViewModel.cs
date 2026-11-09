using MuniChien.App.Navigation;
using MuniChien.Infrastructure.Api;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

/// <summary>
/// Affichage transitoire des données API en lecture seule en attendant les DTO de Maël.
/// Ne fournit aucune opération d'écriture. N'utilise jamais les services de démonstration.
/// </summary>
public sealed class ApiReadOnlyPageViewModel : ViewModelBase, IDisposable
{
    private readonly ApiConnectionSession _session;
    private readonly bool _isDogsPage;
    private CancellationTokenSource? _loading;
    private DataView? _tableView;
    private IReadOnlyList<ApiDisplayField> _summary = [];
    private string _note = "";
    private string _countLabel = "";
    private int _generation;
    private bool _disposed;

    public ApiReadOnlyPageViewModel(ApiConnectionSession session, bool isDogsPage)
    {
        _session = session;
        _isDogsPage = isDogsPage;
        RefreshCommand = new RelayCommand(() => _ = LoadAsync());
    }

    public string Heading => _isDogsPage ? "Chiens — données de l'API" : "Accueil — données de l'API";
    public string Endpoint => _isDogsPage ? "/api/chiens" : "/api/accueil";
    public DataLoadState LoadState { get; } = new();
    public ICommand RefreshCommand { get; }
    public IReadOnlyList<ApiDisplayField> Summary => _summary;
    public DataView? TableView => _tableView;
    public string Note => _note;
    public string CountLabel => _countLabel;
    public Visibility SummaryVisibility => _summary.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility TableVisibility => _tableView is not null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EmptyVisibility => LoadState.IsEmpty ? Visibility.Visible : Visibility.Collapsed;

    public async Task LoadAsync()
    {
        if (_disposed || !_session.IsApiMode) return;
        _loading?.Cancel();
        var cts = new CancellationTokenSource();
        _loading = cts;
        var generation = ++_generation;
        var serverUrl = _session.ServerUrl;

        _summary = [];
        _tableView = null;
        _countLabel = "";
        _note = "";
        NotifyPresentationChanged();
        LoadState.Begin();

        try
        {
            using var client = new MuniChienApiClient(serverUrl);
            JsonElement payload = _isDogsPage
                ? await client.GetChiensAsync(cts.Token)
                : await client.GetAccueilAsync(cts.Token);
            if (!StillCurrent(generation, cts, serverUrl)) return;

            var display = _isDogsPage
                ? ApiJsonProjection.Chiens(payload)
                : ApiJsonProjection.Accueil(payload);
            _summary = display.Summary;
            _note = display.Note;
            if (display.HasRecordCollection)
            {
                _tableView = CreateTable(display.Records);
                _countLabel = $"{display.RecordCount} entrée(s) reçue(s)";
            }
            NotifyPresentationChanged();
            LoadState.Complete(_summary.Count > 0 || display.RecordCount > 0,
                _isDogsPage ? "Aucun chien n'a été renvoyé par le serveur." : "Aucune statistique à afficher.");
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Navigation, changement de mode ou fermeture : aucune erreur à afficher.
        }
        catch (ApiHttpException ex)
        {
            if (StillCurrent(generation, cts, serverUrl))
                LoadState.Fail($"Le serveur a retourné HTTP {(int)ex.StatusCode} pour {Endpoint}.");
        }
        catch (Exception ex) when (ex is ApiConnectionException or ApiResponseException or
                                   HttpRequestException or TimeoutException or IOException or JsonException or
                                   ArgumentException or System.Data.DataException)
        {
            if (StillCurrent(generation, cts, serverUrl))
                LoadState.Fail($"Chargement impossible pour {Endpoint} : {ex.Message}");
        }
        finally
        {
            if (ReferenceEquals(_loading, cts)) _loading = null;
            cts.Dispose();
        }
    }

    private bool StillCurrent(int generation, CancellationTokenSource token, string url) =>
        !_disposed && generation == _generation && !token.IsCancellationRequested &&
        _session.IsApiMode && _session.ServerUrl == url;

    private static DataView CreateTable(IReadOnlyList<ApiDisplayRecord> records)
    {
        var table = new DataTable("Données API");
        var columnNames = records.SelectMany(record => record.Fields.Select(field => field.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(30).ToList();
        if (columnNames.Count == 0) columnNames.Add("Donnée");
        foreach (var name in columnNames) table.Columns.Add(name, typeof(string));
        foreach (var record in records)
        {
            var row = table.NewRow();
            foreach (var field in record.Fields)
                if (table.Columns.Contains(field.Name)) row[field.Name] = field.Value;
            table.Rows.Add(row);
        }
        return table.DefaultView;
    }

    private void NotifyPresentationChanged()
    {
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(TableView));
        OnPropertyChanged(nameof(Note));
        OnPropertyChanged(nameof(CountLabel));
        OnPropertyChanged(nameof(SummaryVisibility));
        OnPropertyChanged(nameof(TableVisibility));
    }

    public void MarkConnectionLost()
    {
        if (_disposed) return;
        _generation++;
        _loading?.Cancel();
        _summary = [];
        _tableView = null;
        _countLabel = "";
        _note = "";
        NotifyPresentationChanged();
        LoadState.Fail("Connexion à l'API interrompue. Les données précédemment affichées ont été masquées. Réessayez lorsque le serveur sera disponible.");
    }

    public void Dispose()
    {
        _disposed = true;
        _generation++;
        _loading?.Cancel();
        // Le chargement courant libère sa source de cancellation dans finally.
    }
}
