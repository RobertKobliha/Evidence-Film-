using Avalonia.Controls;
using Avalonia.Interactivity;
using Evidence_Filmů.Models;

namespace Evidence_Filmů.Views;

// Okno pro zobrazení detailu jednoho záznamu (jen pro čtení).
// Otevírá se dvojitým kliknutím nebo tlačítkem "Detail" v hlavním okně.
public partial class Uvod : Window
{
    // Konstruktor přijímá položku, jejíž data chceme zobrazit
    public Uvod(Item item)
    {
        InitializeComponent();

        // Naplníme všechna textová pole daty z předané položky

        // Štítek typu (emoji + název) ve vrchním levém odznaku
        TypLabel.Text = item.TypeLabel;

        // Hlavní nadpis — název díla
        NazevLabel.Text = item.Title;

        // Žánr
        ZanrLabel.Text = item.Genre;

        // Délka s příslušnou jednotkou (minuty pro film, strany pro knihu)
        DelkaLabel.Text = item.Length + " " + item.LengthLabel;

        // Hodnocení s hvězdičkou
        HodnoceniLabel.Text = "⭐ " + item.Rating.ToString("F1") + " / 10";

        // Stav dokončení (zhlédnuto / přečteno / nedokončeno)
        StavLabel.Text = item.IsCompleted ? "✅ " + item.CompletedLabel : "⏳ Nedokončeno";

        // Datum přidání do evidence
        DatumLabel.Text = item.AddedDate.ToString("dd.MM.yyyy");

        // Popis — pokud není zadán, zobrazíme oznamovací text
        PopisLabel.Text = string.IsNullOrWhiteSpace(item.Description)
                          ? "(bez popisu)"
                          : item.Description;
    }

    // Tlačítko "Zavřít" — jednoduše zavřeme okno
    private void OnZavritClick(object? sender, RoutedEventArgs e) => Close();
}
