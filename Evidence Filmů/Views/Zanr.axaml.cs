using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Evidence_Filmů.Views;

// Okno pro správu žánrů — uživatel může přidávat nové žánry do seznamu.
// Po zavření se změněný seznam předá zpět hlavnímu oknu přes vlastnost AktualniZanry.
public partial class Zanr : Window
{
    // Pozorovatelná kolekce = UI se automaticky aktualizuje při přidání položky
    private readonly ObservableCollection<string> _zanry;

    // Tato vlastnost vrací aktuální (případně upravenou) kopii seznamu žánrů
    public List<string> AktualniZanry => new List<string>(_zanry);

    // Konstruktor: přijme stávající seznam žánrů a zobrazí ho v ListBoxu
    public Zanr(List<string> zanry)
    {
        InitializeComponent();

        _zanry = new ObservableCollection<string>(zanry);

        // Propojíme ListBox v UI s naší kolekcí
        ZanrList.ItemsSource = _zanry;
    }

    // Klik na "Přidat" — přidá nový žánr z textového pole do seznamu
    private void OnPridatClick(object? sender, RoutedEventArgs e)
    {
        var text = NovyZanrBox.Text?.Trim() ?? "";

        // Validace: prázdný žánr nepřidáme
        if (string.IsNullOrWhiteSpace(text))
        {
            ChybaLabel.Text = "Žánr nesmí být prázdný.";
            return;
        }

        // Validace: duplikáty nepřidáme
        if (_zanry.Contains(text))
        {
            ChybaLabel.Text = "Tento žánr již existuje.";
            return;
        }

        // Přidáme žánr a vyčistíme pole + chybový popisek
        _zanry.Add(text);
        NovyZanrBox.Text = "";
        ChybaLabel.Text  = "";
    }

    // Tlačítko "Zavřít" — zavřeme okno (změny jsou přístupné přes AktualniZanry)
    private void OnZavritClick(object? sender, RoutedEventArgs e) => Close();
}
