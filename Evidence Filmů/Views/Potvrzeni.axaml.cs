using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Evidence_Filmů.Views;

// Potvrzovací dialog — zobrazí otázku a dvě tlačítka: "Smazat" a "Zrušit".
// Používá se před nevratnými akcemi (smazání záznamu).
public partial class Potvrzeni : Window
{
    // Po zavření okna zjistíme, jestli uživatel potvrdil akci (true) nebo ne (false)
    public bool Potvrzeno { get; private set; }

    // Konstruktor přijme text otázky zobrazené v dialogu
    public Potvrzeni(string zprava)
    {
        InitializeComponent();

        // Zobrazíme předaný text (např. "Opravdu chcete smazat Inception?")
        ZpravaLabel.Text = zprava;
    }

    // Klik na "Smazat" — nastavíme příznak a zavřeme
    private void OnPotvrzeniClick(object? sender, RoutedEventArgs e)
    {
        Potvrzeno = true;
        Close();
    }

    // Klik na "Zrušit" — zavřeme bez potvrzení (Potvrzeno zůstane false)
    private void OnZrusitClick(object? sender, RoutedEventArgs e) => Close();
}
