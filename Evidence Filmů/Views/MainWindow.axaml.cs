using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Evidence_Filmů.ViewModels;

namespace Evidence_Filmů.Views;

// Kód na pozadí hlavního okna — zpracovává události z UI a deleguje logiku na ViewModel.
public partial class MainWindow : Window
{
    // Zkrácený přístup k ViewModelu (DataContext je nastaven v App.axaml.cs)
    private MainWindowViewModel VM => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ZÁHLAVNÍ TLAČÍTKA
    // ═══════════════════════════════════════════════════════════════════════════

    // Otevře dialog pro přidání nového záznamu (film nebo kniha)
    private async void OnPridatClick(object? sender, RoutedEventArgs e)
    {
        // Předáme seznam žánrů, aby ho dialog mohl nabídnout v autocomplete
        var dialog = new Uprava(VM.Nastaveni.SavedGenres);
        await dialog.ShowDialog(this);

        // Pokud uživatel klikl "Uložit" (nikoliv "Zrušit"), přidáme položku
        if (dialog.Potvrzeno)
            VM.AddItem(dialog.GetItem());
    }

    // Otevře okno pro správu žánrů (přidání / odebrání)
    private async void OnZanryClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Zanr(VM.Nastaveni.SavedGenres);
        await dialog.ShowDialog(this);

        // Po zavření okna uložíme případně změněné žánry
        VM.UpdateGenres(dialog.AktualniZanry);
    }

    // Exportuje celý seznam do TXT souboru (uživatel si vybere umístění)
    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        // TopLevel je nejvyšší prvek stromu — přes něj se přistupuje k dialogům OS
        var topLevel = TopLevel.GetTopLevel(this)!;

        var soubor = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exportovat seznam",
            SuggestedFileName = "evidence_filmů.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Textový soubor") { Patterns = new[] { "*.txt" } }
            }
        });

        if (soubor != null)
        {
            VM.ExportToTxt(soubor.Path.LocalPath);

            // Informujeme uživatele o úspěchu
            var zprava = new Zprava("Export dokončen",
                                    "Seznam byl úspěšně exportován do souboru.");
            await zprava.ShowDialog(this);
        }
    }

    // Importuje záznamy z dříve exportovaného TXT souboru
    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;

        var soubory = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Importovat ze souboru",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Textový soubor") { Patterns = new[] { "*.txt" } }
            }
        });

        if (soubory.Count > 0)
        {
            int pocet = VM.ImportFromTxt(soubory[0].Path.LocalPath);

            var zprava = new Zprava("Import dokončen",
                                    $"Bylo importováno {pocet} záznamů.");
            await zprava.ShowDialog(this);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TLAČÍTKA VE STAVOVÉM ŘÁDKU (pracují s vybranou položkou)
    // ═══════════════════════════════════════════════════════════════════════════

    // Zobrazí detail vybraného záznamu (jen pro čtení)
    private async void OnDetailClick(object? sender, RoutedEventArgs e)
    {
        if (VM.SelectedItem == null)
        {
            await UkazatUpozorneni("Vyberte záznam ze seznamu.");
            return;
        }

        var detail = new Uvod(VM.SelectedItem);
        await detail.ShowDialog(this);
    }

    // Otevře dialog pro úpravu vybraného záznamu
    private async void OnUpravitClick(object? sender, RoutedEventArgs e)
    {
        if (VM.SelectedItem == null)
        {
            await UkazatUpozorneni("Vyberte záznam, který chcete upravit.");
            return;
        }

        // Předáme aktuální položku; Uprava ji přímo modifikuje
        var dialog = new Uprava(VM.Nastaveni.SavedGenres, VM.SelectedItem);
        await dialog.ShowDialog(this);

        if (dialog.Potvrzeno)
            VM.UpdateItem(); // data jsou již uložena v objektu, stačí uložit soubor
    }

    // Smaže vybraný záznam po potvrzení
    private async void OnSmazatClick(object? sender, RoutedEventArgs e)
    {
        if (VM.SelectedItem == null)
        {
            await UkazatUpozorneni("Vyberte záznam, který chcete smazat.");
            return;
        }

        // Zobrazíme potvrzovací dialog se jménem záznamu
        var potvrzeni = new Potvrzeni(
            $"Opravdu chcete smazat záznam \"{VM.SelectedItem.Title}\"?\nTuto akci nelze vrátit.");
        await potvrzeni.ShowDialog(this);

        if (potvrzeni.Potvrzeno)
            VM.DeleteItem(VM.SelectedItem);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DALŠÍ UDÁLOSTI
    // ═══════════════════════════════════════════════════════════════════════════

    // Dvojitý klik na záznam v seznamu = otevřít detail
    private async void OnSeznamDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (VM.SelectedItem == null) return;

        var detail = new Uvod(VM.SelectedItem);
        await detail.ShowDialog(this);
    }

    // ─── Pomocná metoda ───────────────────────────────────────────────────────

    // Zobrazí jednoduché informační okno s upozorněním
    private Task UkazatUpozorneni(string text)
    {
        var zprava = new Zprava("Upozornění", text);
        return zprava.ShowDialog(this);
    }
}
