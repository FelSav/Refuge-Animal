using MuniChien.App.Services;
using System.Globalization;
using System.Windows;

namespace MuniChien.App.Views;

public partial class LoyaltyRewardDialog : Window
{
    public LoyaltyRewardDialog(string programName, LoyaltyRewardCandidate reward)
    {
        InitializeComponent();
        ProgramText.Text = $"{programName} · 12 achats vérifiés";
        ProductText.Text = reward.Purchase.ProductName;
        ReferenceText.Text = $"Référence exacte : {reward.Purchase.ProductReference}";
        FormatText.Text = $"Format : {reward.Purchase.Format}";
        PositionText.Text = $"Produit de l'achat n° {reward.Index + 1} sur 12";
        PriceText.Text = reward.Purchase.PricePaid.ToString("C", CultureInfo.GetCultureInfo("fr-CA"));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
