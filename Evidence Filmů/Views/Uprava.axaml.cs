using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Evidence_Filmů.Models;

namespace Evidence_Filmů.Views;

// Okno pro přidání nového záznamu nebo úpravu existujícího.
// Otevírá se buď s prázdným formulářem (nový záznam) nebo předvyplněný (úprava).
public partial class Uprava : Window
{
    // Příznak: uživatel klikl "Uložit" (true) nebo "Zrušit" (false/výchozí)
    public bool Potvrzeno { get; private set; }

    // Odkaz na upravovanou položku (null = přidáváme novou)
    private Item? _editItem;

    // Konstruktor: zanry = seznam dostupných žánrů, editItem = upravovaná položka (nebo null)
    public Uprava(List<string> zanry, Item? editItem = null)
    {
        InitializeComponent();
        _editItem = editItem;

        // Naplníme dropdown pro výběr typu: Film je výchozí (index 0 = ItemType.Film)
        TypBox.ItemsSource = new[] { "Film", "Kniha" };
        TypBox.SelectedIndex = 0; // výchozí = Film

        // Naplníme autocomplete seznam žánrů
        ZanrBox.ItemsSource = zanry;

        // Nadpis formuláře se liší podle režimu (nový / úprava)
        NadpisLabel.Text = editItem == null ? "Přidat záznam" : "Upravit záznam";

        // Pokud upravujeme existující záznam, předvyplníme formulář jeho daty
        if (editItem != null)
        {
            // Film = index 0, Kniha = index 1 (shoduje se s pořadím v ItemsSource)
            TypBox.SelectedIndex = editItem.Type == ItemType.Film ? 0 : 1;
            NazevBox.Text        = editItem.Title;
            ZanrBox.Text         = editItem.Genre;
            DelkaBox.Text        = editItem.Length.ToString();
            HodnoceniBox.Text    = editItem.Rating.ToString("F1", CultureInfo.InvariantCulture);
            PopisBox.Text        = editItem.Description;
            DokončenoBox.IsChecked = editItem.IsCompleted;
        }

        // Při změně typu aktualizujeme popisky (minuty vs. strany, zhlédnuto vs. přečteno)
        TypBox.SelectionChanged += (s, e) => AktualizujPopisky();
        AktualizujPopisky(); // voláme hned, aby se popisky zobrazily správně od začátku
    }

    // Aktualizuje popisky polí podle aktuálně vybraného typu (Film / Kniha)
    private void AktualizujPopisky()
    {
        bool jeKniha = TypBox.SelectedIndex == 1;

        // Délka: pro knihu = počet stran, pro film = počet minut
        DelkaLabel.Text = jeKniha ? "Počet stran" : "Délka (minuty)";

        // Checkbox dokončení: přečteno (kniha) nebo zhlédnuto (film)
        DokončenoBox.Content = jeKniha ? "Přečteno" : "Zhlédnuto";
    }

    // Klik na "Uložit" — provede validaci a uloží data do objektu Item
    private void OnUlozitClick(object? sender, RoutedEventArgs e)
    {
        // ─ Validace vstupů ─────────────────────────────────────────────────

        // Název je povinný
        if (string.IsNullOrWhiteSpace(NazevBox.Text))
        {
            ChybaLabel.Text = "Název nesmí být prázdný.";
            return;
        }

        // Žánr je povinný
        if (string.IsNullOrWhiteSpace(ZanrBox.Text))
        {
            ChybaLabel.Text = "Žánr nesmí být prázdný.";
            return;
        }

        // Délka: nepovinná, výchozí hodnota je 0; pokud uživatel něco napsal, musí to být kladné celé číslo
        int delka = 0;
        if (!string.IsNullOrWhiteSpace(DelkaBox.Text))
        {
            if (!int.TryParse(DelkaBox.Text, out delka) || delka < 0)
            {
                ChybaLabel.Text = "Délka musí být kladné celé číslo.";
                return;
            }
        }

        // Hodnocení: nepovinné, výchozí hodnota je 0; pokud uživatel něco napsal, musí být v rozsahu 0–10
        // Přijímáme jak desetinnou tečku, tak desetinnou čárku
        double hodnoceni = 0;
        if (!string.IsNullOrWhiteSpace(HodnoceniBox.Text))
        {
            if (!double.TryParse(HodnoceniBox.Text.Replace(',', '.'),
                                  NumberStyles.Any,
                                  CultureInfo.InvariantCulture,
                                  out hodnoceni)
                || hodnoceni < 0 || hodnoceni > 10)
            {
                ChybaLabel.Text = "Hodnocení musí být číslo od 0 do 10 (např. 7.5).";
                return;
            }
        }

        // ─ Uložení do objektu ──────────────────────────────────────────────

        // Vytvoříme nový objekt nebo upravíme existující
        var item = _editItem ?? new Item();

        // Mapování indexu dropdownu na enum: index 0 = Film, index 1 = Kniha
        item.Type        = TypBox.SelectedIndex == 0 ? ItemType.Film : ItemType.Book;
        item.Title       = NazevBox.Text!.Trim();
        item.Genre       = ZanrBox.Text!.Trim();
        item.Length      = delka;
        item.Rating      = hodnoceni;
        item.Description = PopisBox.Text?.Trim() ?? "";
        item.IsCompleted = DokončenoBox.IsChecked ?? false;

        _editItem = item; // uložíme odkaz, aby ho GetItem() mohl vrátit
        Potvrzeno = true;
        Close();
    }

    // Klik na "Zrušit" — zavřeme okno bez uložení
    private void OnZrusitClick(object? sender, RoutedEventArgs e) => Close();

    // Vrátí výsledný objekt Item (voláme po ShowDialog, pokud Potvrzeno == true)
    public Item GetItem() => _editItem ?? new Item();
}
