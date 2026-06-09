using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Evidence_Filmů.Views;

// Jednoduchý informační dialog — zobrazí nadpis, text zprávy a tlačítko OK.
// Používá se pro oznámení úspěchu (export, import) nebo upozornění.
public partial class Zprava : Window
{
    // Konstruktor: nadpis = titulek okna i nadpisový label, text = tělo zprávy
    public Zprava(string nadpis, string text)
    {
        InitializeComponent();

        // Nastavíme titulek okna i zobrazovaný nadpis
        Title = nadpis;
        NadpisLabel.Text = nadpis;

        // Nastavíme tělo zprávy
        TextLabel.Text = text;
    }

    // Tlačítko "OK" — zavřeme dialog
    private void OnOkClick(object? sender, RoutedEventArgs e) => Close();
}
