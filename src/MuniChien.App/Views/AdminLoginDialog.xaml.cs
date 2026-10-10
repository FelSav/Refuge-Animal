using System;
using System.Windows;

namespace MuniChien.App.Views;

public partial class AdminLoginDialog : Window
{
    public AdminLoginDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameInput.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        // DÉMONSTRATION UNIQUEMENT : aucune authentification réelle.
        // À supprimer dès que l'API d'authentification sera disponible.
        bool validUser = string.Equals(
            UsernameInput.Text.Trim(), "Admin", StringComparison.OrdinalIgnoreCase);
        bool validPassword = PasswordInput.Password == "1234";

        if (!validUser || !validPassword)
        {
            ErrorMessage.Text = "Nom d'utilisateur ou mot de passe incorrect.";
            ErrorMessage.Visibility = Visibility.Visible;
            PasswordInput.Clear();
            PasswordInput.Focus();
            return;
        }

        PasswordInput.Clear();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
