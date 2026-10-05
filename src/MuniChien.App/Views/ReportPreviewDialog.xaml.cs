using Microsoft.Win32;
using MuniChien.App.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MuniChien.App.Views;

public partial class ReportPreviewDialog : Window
{
    private readonly IReadOnlyList<string>
        _headers;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public ReportPreviewDialog(
        string reportTitle,
        string description,
        string municipality,
        string period,
        IReadOnlyList<string> headers,
        IReadOnlyList<ReportPreviewRowViewModel> rows)
    {
        InitializeComponent();


        ReportTitle =
            reportTitle;

        Description =
            description;

        Municipality =
            municipality;

        Period =
            period;

        GeneratedAt =
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm");


        _headers =
            headers;


        Rows =
            new ObservableCollection<ReportPreviewRowViewModel>(
                rows);


        ConfigureColumns();


        DataContext =
            this;
    }


    // ==================================================
    // PROPRIÉTÉS
    // ==================================================

    public string ReportTitle { get; }

    public string Description { get; }

    public string Municipality { get; }

    public string Period { get; }

    public string GeneratedAt { get; }


    public ObservableCollection<ReportPreviewRowViewModel>
        Rows
    { get; }


    public bool HasResults =>
        Rows.Count > 0;


    public string ResultCountText =>
        Rows.Count == 1
            ? "1 résultat"
            : $"{Rows.Count} résultats";


    // ==================================================
    // COLONNES
    // ==================================================

    private void ConfigureColumns()
    {
        DataGridColumn[] columns =
        [
            Column1,
            Column2,
            Column3,
            Column4,
            Column5,
            Column6
        ];


        for (int i = 0; i < columns.Length; i++)
        {
            string header =
                i < _headers.Count
                    ? _headers[i]
                    : string.Empty;


            columns[i].Header =
                header;


            columns[i].Visibility =
                string.IsNullOrWhiteSpace(
                    header)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }


    // ==================================================
    // EXPORT CSV
    // ==================================================

    private void ExportCsv_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (Rows.Count == 0)
        {
            MessageBox.Show(
                "Il n'y a aucune donnée à exporter.",
                "Export",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }


        SaveFileDialog dialog =
            new()
            {
                Title =
                    "Exporter le rapport",

                Filter =
                    "Fichier CSV (*.csv)|*.csv",

                FileName =
                    BuildDefaultFileName()
            };


        if (dialog.ShowDialog(this) != true)
        {
            return;
        }


        StringBuilder csv =
            new();


        IEnumerable<string> visibleHeaders =
            _headers
                .Where(
                    header =>
                        !string.IsNullOrWhiteSpace(
                            header));


        csv.AppendLine(
            string.Join(
                ";",
                visibleHeaders.Select(
                    EscapeCsv)));


        foreach (
            ReportPreviewRowViewModel row
            in Rows)
        {
            string[] values =
            [
                row.Value1,
                row.Value2,
                row.Value3,
                row.Value4,
                row.Value5,
                row.Value6
            ];


            int visibleColumnCount =
                _headers.Count(
                    header =>
                        !string.IsNullOrWhiteSpace(
                            header));


            csv.AppendLine(
                string.Join(
                    ";",
                    values
                        .Take(
                            visibleColumnCount)
                        .Select(
                            EscapeCsv)));
        }


        File.WriteAllText(
            dialog.FileName,
            csv.ToString(),
            new UTF8Encoding(
                true));


        MessageBox.Show(
            "Le rapport a été exporté avec succès.",
            "Export terminé",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }


    // ==================================================
    // FERMER
    // ==================================================

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }


    // ==================================================
    // CSV
    // ==================================================

    private static string EscapeCsv(
        string value)
    {
        string safeValue =
            value ?? string.Empty;


        safeValue =
            safeValue.Replace(
                "\"",
                "\"\"");


        return
            $"\"{safeValue}\"";
    }


    private string BuildDefaultFileName()
    {
        string safeTitle =
            string.Join(
                "_",
                ReportTitle
                    .Split(
                        Path.GetInvalidFileNameChars(),
                        StringSplitOptions.RemoveEmptyEntries));


        safeTitle =
            safeTitle.Replace(
                ' ',
                '_');


        return
            $"{safeTitle}_{DateTime.Today:yyyy-MM-dd}.csv";
    }
}