using System.Collections.Generic;

namespace Evidence_Filmů.Models;

// Nastavení aplikace — ukládají se do souboru v AppData a načítají se při každém spuštění.
public class AppSettings
{
    // Seznam žánrů uložených uživatelem (zobrazují se v dropdown při přidávání záznamu i ve filtru)
    // Výchozí hodnoty pokrývají nejběžnější filmové/knižní žánry
    public List<string> SavedGenres { get; set; } = new()
    {
        "Akce", "Drama", "Komedie", "Horor", "Sci-Fi",
        "Fantasy", "Thriller", "Dokumentární", "Romance", "Animovaný"
    };

    // Poslední použitý filtr žánru — obnoví se při příštím spuštění aplikace
    public string LastFilter { get; set; } = "";
}
