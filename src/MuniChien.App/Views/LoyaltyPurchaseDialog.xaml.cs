using Microsoft.Win32;
using MuniChien.App.Services;
using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace MuniChien.App.Views;

public partial class LoyaltyPurchaseDialog : Window
{
    private readonly LoyaltyPurchaseRecord? _original;
    private readonly bool _readOnly;
    private string _attachmentPath = "";

    public LoyaltyPurchaseRecord? ResultPurchase { get; private set; }
    public bool DeleteRequested { get; private set; }

    public LoyaltyPurchaseDialog(string programName, int purchaseIndex,
        LoyaltyPurchaseRecord? existing, DateTime? legacyDate = null, bool readOnly = false)
    {
        InitializeComponent();
        _original = existing;
        _readOnly = readOnly;
        TitleText.Text = readOnly ? "Détail de l'achat archivé"
            : existing is not null ? "Modifier un achat" : "Enregistrer un sac";
        SubtitleText.Text = $"{programName} · Sac n° {purchaseIndex + 1} sur 12";
        ProductNameBox.Text = existing?.ProductName ?? "";
        ReferenceBox.Text = existing?.ProductReference ?? "";
        FormatBox.Text = existing?.Format ?? "";
        PriceBox.Text = existing is null ? "" : existing.PricePaid.ToString("0.00", CultureInfo.GetCultureInfo("fr-CA"));
        PurchaseDatePicker.SelectedDate = existing?.PurchasedAt ?? legacyDate ?? DateTime.Today;
        PurchaseDatePicker.DisplayDateEnd = DateTime.Today;
        InvoiceBox.Text = existing?.InvoiceNumber ?? "";
        _attachmentPath = existing?.AttachmentPath ?? "";
        RefreshAttachment();
        DeleteButton.Visibility = !readOnly && (existing is not null || legacyDate.HasValue)
            ? Visibility.Visible : Visibility.Collapsed;
        SaveButton.Visibility = readOnly ? Visibility.Collapsed : Visibility.Visible;
        BrowseButton.IsEnabled = !readOnly;
        ClearAttachmentButton.IsEnabled = !readOnly;
        ProductNameBox.IsReadOnly = readOnly;
        ReferenceBox.IsReadOnly = readOnly;
        FormatBox.IsReadOnly = readOnly;
        PriceBox.IsReadOnly = readOnly;
        InvoiceBox.IsReadOnly = readOnly;
        PurchaseDatePicker.IsEnabled = !readOnly;
        if (!readOnly && legacyDate.HasValue && existing is null)
            TitleText.Text = "Compléter une ancienne coche";
    }

    private void RefreshAttachment()
    {
        AttachmentText.Text = string.IsNullOrWhiteSpace(_attachmentPath)
            ? "Aucun justificatif sélectionné" : Path.GetFileName(_attachmentPath);
        AttachmentText.ToolTip = string.IsNullOrWhiteSpace(_attachmentPath) ? "Aucun fichier" : _attachmentPath;
        ClearAttachmentButton.Visibility = string.IsNullOrWhiteSpace(_attachmentPath) || _readOnly
            ? Visibility.Collapsed : Visibility.Visible;
        OpenAttachmentButton.Visibility = string.IsNullOrWhiteSpace(_attachmentPath)
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        var picker = new OpenFileDialog
        {
            Title = "Choisir l'image d'une facture",
            Filter = "Images (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp",
            Multiselect = false
        };
        if (picker.ShowDialog(this) != true) return;
        if (new FileInfo(picker.FileName).Length > 10 * 1024 * 1024)
        {
            MessageBox.Show(this, "Choisissez une image de moins de 10 Mo.", "Image trop volumineuse",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _attachmentPath = picker.FileName;
        RefreshAttachment();
    }

    private void OpenAttachment_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_attachmentPath)) return;
        if (!File.Exists(_attachmentPath))
        {
            MessageBox.Show(this, "L'image est introuvable à cet emplacement.",
                "Justificatif indisponible", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo(_attachmentPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Impossible d'ouvrir le justificatif",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ClearAttachment_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        _attachmentPath = "";
        RefreshAttachment();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        try
        {
            var rawPrice = PriceBox.Text.Trim().Replace('$', ' ').Trim();
            if (!decimal.TryParse(rawPrice, NumberStyles.Number,
                    CultureInfo.GetCultureInfo("fr-CA"), out var amount) &&
                !decimal.TryParse(rawPrice, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out amount))
                throw new ArgumentException("Entrez un prix valide, par exemple 79,99.");
            if (PurchaseDatePicker.SelectedDate is not DateTime date)
                throw new ArgumentException("Choisissez la date de l'achat.");
            var purchase = new LoyaltyPurchaseRecord
            {
                Id = _original?.Id ?? Guid.Empty,
                ProductName = ProductNameBox.Text.Trim(),
                ProductReference = ReferenceBox.Text.Trim(),
                Format = FormatBox.Text.Trim(),
                PricePaid = amount,
                PurchasedAt = date.Date,
                InvoiceNumber = InvoiceBox.Text.Trim(),
                AttachmentPath = _attachmentPath,
                Origin = _original?.Origin ?? "Manuel",
                ExternalTransactionKey = _original?.ExternalTransactionKey
            };
            LocalLoyaltyService.ValidatePurchase(purchase);
            ResultPurchase = purchase;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Achat à corriger", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (MessageBox.Show(this,
                "Supprimer cet achat et ses informations ? Cette action ne peut pas être annulée.",
                "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        DeleteRequested = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
