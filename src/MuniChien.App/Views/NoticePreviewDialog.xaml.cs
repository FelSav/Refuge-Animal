using MuniChien.App.Printing;
using MuniChien.App.Services;
using MuniChien.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MuniChien.App.Views;

public partial class NoticePreviewDialog : Window
{
    private readonly IReadOnlyList<NoticeCandidateViewModel> _notices;
    private readonly string _campaign;
    private readonly int _year;
    private FixedDocument _document = new();

    public NoticePreviewDialog(
        IReadOnlyList<NoticeCandidateViewModel> notices,
        string campaign, int year)
    {
        InitializeComponent();
        _notices = notices;
        _campaign = campaign;
        _year = year;
        BuildPreview();
    }

    private void BuildPreview()
    {
        NoticeTemplateSettings settings = NoticeTemplateSettingsStore.Load();
        _document = NoticePrintDocumentBuilder.Build(_notices, _campaign, _year, settings);
        PreviewViewer.Document = _document;
        DocumentInfo.Text = $"{_notices.Count} dossier(s), {_document.Pages.Count} page(s) · {_campaign} · {_year} · Lettre 8,5 × 11 po";
    }

    private void Configure_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsDialog = new NoticeTemplateSettingsDialog { Owner = this };
            if (settingsDialog.ShowDialog() == true)
                BuildPreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible de recharger le modèle : {ex.Message}",
                "Configuration des avis", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
            "Les soldes présentés sont fictifs et ne correspondent pas aux impayés vérifiés d'une année.\n\n" +
            "Chaque page portera « DÉMONSTRATION — NE PAS ENVOYER ».\n\n" +
            "Lancer une impression de test seulement ?",
            "Impression de démonstration", MessageBoxButton.YesNo,
            MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        try
        {
            // FixedDocument conserve les sauts de page : un dossier par page,
            // sauf s'il comporte plus de 11 chiens (pages supplémentaires).
            printDialog.PrintDocument(_document.DocumentPaginator, "MuniChien — Avis spécifique DÉMONSTRATION");
            MessageBox.Show(
                "Impression de test envoyée. Aucun avis n'a été marqué comme envoyé.",
                "Impression", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"L'impression a échoué : {ex.Message}",
                "Erreur d'impression", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
