using MuniChien.Infrastructure.Api;
using System.IO;
using System.Windows;

namespace MuniChien.App.Views;

public partial class ApiAdministrationDialog : Window
{
    private readonly ApiConnectionSession _session;
    private CancellationTokenSource? _testCancellation;

    public ApiAdministrationDialog(ApiConnectionSession session)
    {
        InitializeComponent();
        _session = session;
        ServerUrlBox.Text = _session.ServerUrl;
        StatusText.Text = _session.StatusMessage;
        Closing += (_, _) => _testCancellation?.Cancel();
    }

    private async void Test_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Autoriser le test d'une adresse non encore enregistrée.
            using var client = new MuniChienApiClient(ServerUrlBox.Text.Trim(),
                requestTimeout: TimeSpan.FromSeconds(8));
            _testCancellation = new CancellationTokenSource();
            SetBusy(true);
            StatusText.Text = "Vérification en cours…";
            await client.GetAccueilAsync(_testCancellation.Token);
            StatusText.Text = "API accessible : JSON valide reçu de /api/accueil.";
        }
        catch (OperationCanceledException) { StatusText.Text = "Test annulé."; }
        catch (Exception ex) { StatusText.Text = "Échec : " + ex.Message; }
        finally
        {
            _testCancellation?.Dispose();
            _testCancellation = null;
            SetBusy(false);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _session.SaveServerUrl(ServerUrlBox.Text.Trim());
            DialogResult = true;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            StatusText.Text = "Enregistrement impossible : " + ex.Message;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void SetBusy(bool busy)
    {
        TestButton.IsEnabled = !busy;
        SaveButton.IsEnabled = !busy;
        ServerUrlBox.IsEnabled = !busy;
    }
}
