using MuniChien.App.Printing;
using MuniChien.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MuniChien.App.Views;

public partial class NoticeMailingPreviewDialog : Window
{
    private readonly FixedDocument _document;
    private readonly bool _isEnvelope;

    public NoticeMailingPreviewDialog(
        IReadOnlyList<NoticeCandidateViewModel> recipients,
        bool isEnvelope)
    {
        InitializeComponent();
        foreach (var recipient in recipients)
        {
            if (!NoticeMailingDocumentBuilder.TryValidateRecipient(recipient, out string reason))
                throw new InvalidOperationException(
                    $"Adresse incomplète pour {recipient.OwnerName} (dossier #{recipient.FileNumber}) : {reason}.");
        }
        _isEnvelope = isEnvelope;
        _document = isEnvelope
            ? NoticeMailingDocumentBuilder.BuildEnvelopes(recipients)
            : NoticeMailingDocumentBuilder.BuildLabels(recipients);
        PreviewViewer.Document = _document;

        PreviewTitle.Text = isEnvelope ? "Enveloppes — Aperçu" : "Étiquettes postales — Aperçu";
        string format = isEnvelope ? "enveloppe no 10 (9,5 × 4,125 po)" : "lettre, 30 étiquettes (3 × 10)";
        PreviewInfo.Text = $"{recipients.Count} destinataire(s) · {_document.Pages.Count} page(s) · Format : {format}. " +
            "Adresses complètes selon les champs locaux (non vérifiées auprès de Postes Canada). " +
            "Vérifiez l'alignement du support avant toute impression définitive.";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        string support = _isEnvelope ? "l'enveloppe no 10" : "la feuille de 30 étiquettes";
        var answer = MessageBox.Show(
            "Cette impression utilise uniquement des données locales de démonstration. " +
            "Les adresses incomplètes sont exclues, mais les autres adresses ne sont pas vérifiées. " +
            "Chaque support porte une mention de test.\n\n" +
            $"Configurez le format {support} dans les propriétés de l'imprimante. " +
            "Nous recommandons Microsoft Print to PDF pour vérifier les marges.\n\n" +
            "Lancer une impression de TEST ?",
            "Impression d'essai — MuniChien", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            dialog.PrintDocument(_document.DocumentPaginator,
                _isEnvelope ? "MuniChien — Enveloppes DÉMO" : "MuniChien — Étiquettes DÉMO");
            MessageBox.Show(
                "Document envoyé à l'imprimante. Aucun avis n'a été marqué comme envoyé.",
                "Test d'impression", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible d'imprimer : {ex.Message}",
                "Erreur d'impression", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
