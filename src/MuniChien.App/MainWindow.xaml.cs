using MuniChien.App.ViewModels;
using System.Windows;

namespace MuniChien.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}