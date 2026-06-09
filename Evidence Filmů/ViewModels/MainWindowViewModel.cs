using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Evidence_Filmů.Models;
using Evidence_Filmů.Servis;

namespace Evidence_Filmů.ViewModels;

// Hlavní ViewModel aplikace — řídí veškerá data a logiku hlavního okna.
// Dědí z ViewModelBase (ObservableObject), takže může notifikovat UI o změnách.
// "partial" je nutné pro CommunityToolkit.Mvvm generátor kódu ([ObservableProperty]).
public partial class MainWindowViewModel : ViewModelBase
{
    // ─── Datová vrstva ────────────────────────────────────────────────────────

    // Objekt zajišťující čtení a zápis dat do souborů v AppData
    private readonly Data _dataServis = new();

    // Interní seznam VŠECH položek (nepřefiltrovaný) — základ pro veškeré operace
    private readonly List<Item> _vsechnyPolozky = new();

    // ─── Veřejné kolekce pro binding ─────────────────────────────────────────

    // Nastavení aplikace (seznam žánrů, poslední použitý filtr)
    public AppSettings Nastaveni { get; private set; } = new();

    // Položky skutečně zobrazené v seznamu — aktualizuje se při každé změně filtru
    public ObservableCollection<Item> FilteredItems { get; } = new();

    // Dostupné žánry pro rozbalovací seznam filtru (vždy začíná možností "Vše")
    public ObservableCollection<string> AvailableGenres { get; } = new();

    // Pevný seznam typů pro filtr (Vše / Film / Kniha)
    public List<string> TypeFilters { get; } = new() { "Vše", "Film", "Kniha" };

    // ─── Vlastnosti s automatickým upozorněním (generuje CommunityToolkit.Mvvm) ─

    // Text pro fulltextové vyhledávání (prohledává název, žánr i popis)
    [ObservableProperty]
    private string _searchText = "";

    // Aktuálně vybraný žánr ve filtru ("Vše" = zobrazit vše)
    [ObservableProperty]
    private string _selectedGenreFilter = "Vše";

    // Aktuálně vybraný typ ve filtru ("Vše" / "Film" / "Kniha")
    [ObservableProperty]
    private string _selectedTypeFilter = "Vše";

    // Text zobrazený ve stavovém řádku (počty záznamů)
    [ObservableProperty]
    private string _statusText = "";

    // Aktuálně označená položka v seznamu (null = nic nevybráno)
    [ObservableProperty]
    private Item? _selectedItem;

    // ─── Konstruktor ──────────────────────────────────────────────────────────

    public MainWindowViewModel()
    {
        // Při spuštění aplikace automaticky načteme uložená data
        LoadData();
    }

    // ─── Reakce na změny filtrovacích vlastností ──────────────────────────────
    // Tyto "partial" metody jsou volány automaticky generátorem vždy, když se
    // příslušná vlastnost změní (tj. uživatel napíše text nebo vybere žánr/typ).

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedGenreFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedTypeFilterChanged(string value) => ApplyFilter();

    // ─── Načítání a ukládání dat ──────────────────────────────────────────────

    // Načte nastavení a položky ze souborů (AppData/Evidence_Filmů/)
    public void LoadData()
    {
        Nastaveni = _dataServis.LoadSettings();
        _vsechnyPolozky.Clear();
        _vsechnyPolozky.AddRange(_dataServis.LoadItems());

        RefreshGenreList();  // aktualizuj dropdown žánrů
        ApplyFilter();       // zobraz správné položky
        UpdateStatusText();  // aktualizuj statistiky dole
    }

    // Uloží všechny položky do souboru (volá se po každé změně dat)
    public void SaveData() => _dataServis.SaveItems(_vsechnyPolozky);

    // Uloží nastavení (žánry, poslední filtr) do souboru
    public void SaveSettings() => _dataServis.SaveSettings(Nastaveni);

    // ─── Operace CRUD (Create / Read / Update / Delete) ─────────────────────

    // Přidá novou položku do evidence a uloží
    public void AddItem(Item item)
    {
        _vsechnyPolozky.Add(item);
        SaveData();
        ApplyFilter();
        UpdateStatusText();
    }

    // Aktualizuje data po úpravě existující položky a uloží
    // (Samotný objekt Item je předáván odkazem, takže změny jsou již v _vsechnyPolozky)
    public void UpdateItem()
    {
        SaveData();
        ApplyFilter();       // přefiltruj, protože se mohly změnit hodnoty
        UpdateStatusText();
    }

    // Smaže položku z evidence a uloží
    public void DeleteItem(Item item)
    {
        _vsechnyPolozky.Remove(item);
        SaveData();
        ApplyFilter();
        UpdateStatusText();
    }

    // ─── Export / Import ──────────────────────────────────────────────────────

    // Exportuje VŠECHNY položky (ne jen přefiltrované) do čitelného TXT souboru
    public void ExportToTxt(string path) => _dataServis.ExportToTxt(_vsechnyPolozky, path);

    // Importuje záznamy z dříve exportovaného TXT souboru, vrátí počet přidaných
    public int ImportFromTxt(string path)
    {
        var imported = _dataServis.ImportFromTxt(path);
        foreach (var item in imported)
            _vsechnyPolozky.Add(item);
        SaveData();
        ApplyFilter();
        UpdateStatusText();
        return imported.Count;
    }

    // ─── Správa žánrů ────────────────────────────────────────────────────────

    // Uloží nový seznam žánrů (po uzavření okna správy žánrů)
    public void UpdateGenres(List<string> newGenres)
    {
        Nastaveni.SavedGenres = newGenres;
        SaveSettings();
        RefreshGenreList();  // aktualizuj dropdown s žánry
    }

    // ─── Pomocné privátní metody ──────────────────────────────────────────────

    // Obnoví seznam žánrů v dropdownu filtru
    private void RefreshGenreList()
    {
        // Zapamatuj si aktuální výběr, abychom ho mohli obnovit
        var currentSelection = SelectedGenreFilter;

        AvailableGenres.Clear();
        AvailableGenres.Add("Vše"); // první položka vždy "zobrazit vše"
        foreach (var genre in Nastaveni.SavedGenres)
            AvailableGenres.Add(genre);

        // Obnov předchozí výběr, pokud žánr stále existuje; jinak resetuj na "Vše"
        SelectedGenreFilter = AvailableGenres.Contains(currentSelection) ? currentSelection : "Vše";
    }

    // Aplikuje všechny aktivní filtry a zobrazí odpovídající položky
    private void ApplyFilter()
    {
        FilteredItems.Clear();

        foreach (var item in _vsechnyPolozky)
        {
            // 1. Filtr podle typu (Film / Kniha / Vše)
            if (SelectedTypeFilter == "Film" && item.Type != ItemType.Film) continue;
            if (SelectedTypeFilter == "Kniha" && item.Type != ItemType.Book) continue;

            // 2. Filtr podle žánru
            if (SelectedGenreFilter != "Vše" && item.Genre != SelectedGenreFilter) continue;

            // 3. Fulltextové vyhledávání — prohledává název, žánr i popis
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var query = SearchText.ToLower();
                bool matches = item.Title.ToLower().Contains(query)
                            || item.Genre.ToLower().Contains(query)
                            || item.Description.ToLower().Contains(query);
                if (!matches) continue;
            }

            // Položka prošla všemi filtry → přidej ji do zobrazeného seznamu
            FilteredItems.Add(item);
        }

        // Ulož poslední použitý filtr do nastavení (při příštím spuštění se obnoví)
        Nastaveni.LastFilter = SelectedGenreFilter;
    }

    // Aktualizuje text stavového řádku se statistikami
    private void UpdateStatusText()
    {
        int celkem = _vsechnyPolozky.Count;
        int filmu  = _vsechnyPolozky.Count(p => p.Type == ItemType.Film);
        int knih   = _vsechnyPolozky.Count(p => p.Type == ItemType.Book);

        StatusText = $"Celkem záznamů: {celkem}   •   Filmů: {filmu}   •   Knih: {knih}";
    }
}
