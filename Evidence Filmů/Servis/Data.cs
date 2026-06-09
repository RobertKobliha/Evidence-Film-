using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Evidence_Filmů.Models;

namespace Evidence_Filmů.Servis;

// Třída Data zajišťuje veškerou práci se soubory — čtení, zápis, export a import.
// Všechna data ukládáme do složky AppData, aby se neztratila při přesunu exe souboru.
public class Data
{
    // Cesta ke složce aplikace v AppData (např. C:\Users\Jméno\AppData\Roaming\Evidence_Filmů)
    private static readonly string AppDataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "Evidence_Filmů");

    // Soubor s uloženými záznamy (filmy / knihy)
    private static readonly string DataFile = Path.Combine(AppDataDir, "media.txt");

    // Soubor s nastavením aplikace (uložené žánry, poslední filtr)
    private static readonly string SettingsFile = Path.Combine(AppDataDir, "settings.txt");

    public Data()
    {
        // Zajistíme, že složka AppData existuje (při prvním spuštění ji vytvoříme)
        Directory.CreateDirectory(AppDataDir);
    }

    // ─── Načítání dat ─────────────────────────────────────────────────────────

    // Načte seznam všech záznamů ze souboru. Vrátí prázdný seznam, pokud soubor neexistuje.
    public List<Item> LoadItems()
    {
        var items = new List<Item>();
        if (!File.Exists(DataFile)) return items;

        // Každý řádek souboru = jeden záznam
        foreach (var line in File.ReadAllLines(DataFile, Encoding.UTF8))
        {
            var item = ParseItem(line);
            if (item != null) items.Add(item);
        }
        return items;
    }

    // Načte nastavení aplikace (žánry, poslední filtr). Vrátí výchozí nastavení, pokud soubor neexistuje.
    public AppSettings LoadSettings()
    {
        var settings = new AppSettings();
        if (!File.Exists(SettingsFile)) return settings;

        foreach (var line in File.ReadAllLines(SettingsFile, Encoding.UTF8))
        {
            if (line.StartsWith("GENRE:"))
                // Přidáme uložený žánr do seznamu
                settings.SavedGenres.Add(line[6..].Trim());
            else if (line.StartsWith("LAST_FILTER:"))
                // Obnovíme poslední použitý filtr
                settings.LastFilter = line[12..].Trim();
        }
        return settings;
    }

    // ─── Ukládání dat ─────────────────────────────────────────────────────────

    // Uloží celý seznam záznamů do souboru (přepíše předchozí obsah)
    public void SaveItems(IEnumerable<Item> items)
    {
        var lines = items.Select(i => SerializeItem(i));
        File.WriteAllLines(DataFile, lines, Encoding.UTF8);
    }

    // Uloží nastavení aplikace do souboru
    public void SaveSettings(AppSettings settings)
    {
        var lines = new List<string>();

        // Každý žánr na samostatný řádek s předponou "GENRE:"
        foreach (var g in settings.SavedGenres)
            lines.Add("GENRE:" + g);

        // Poslední filtr
        lines.Add("LAST_FILTER:" + settings.LastFilter);

        File.WriteAllLines(SettingsFile, lines, Encoding.UTF8);
    }

    // ─── Export / Import ──────────────────────────────────────────────────────

    // Exportuje záznamy do čitelného TXT souboru (pro sdílení nebo tisk)
    public void ExportToTxt(IEnumerable<Item> items, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== EVIDENCE EXPORT ===");
        sb.AppendLine("Datum exportu: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        sb.AppendLine();

        // Každý záznam vypisujeme v lidsky čitelném formátu
        foreach (var item in items)
        {
            sb.AppendLine("--- " + item.Type.ToString().ToUpper() + " ---");
            sb.AppendLine("Název:     " + item.Title);
            sb.AppendLine("Žánr:      " + item.Genre);
            sb.AppendLine("Délka:     " + item.Length + " " + item.LengthLabel);
            sb.AppendLine("Hodnocení: " + item.Rating.ToString("F1") + "/10");
            sb.AppendLine("Stav:      " + (item.IsCompleted ? item.CompletedLabel : "Nedokončeno"));
            sb.AppendLine("Popis:     " + item.Description);
            sb.AppendLine("Přidáno:   " + item.AddedDate.ToString("dd.MM.yyyy"));
            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    // Importuje záznamy z TXT souboru (podporuje formát exportu i formát datového souboru)
    public List<Item> ImportFromTxt(string path)
    {
        var items = new List<Item>();
        if (!File.Exists(path)) return items;

        foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
        {
            var item = ParseItem(line);
            if (item != null) items.Add(item);
        }
        return items;
    }

    // ─── Serializace a deserializace ──────────────────────────────────────────

    // Převede záznam na jeden řádek textu (pole oddělená | )
    // Formát: Id|Typ|Název|Žánr|Popis|Délka|Hodnocení|Dokončeno|DatumPřidání
    private string SerializeItem(Item i)
    {
        return i.Id + "|" + (int)i.Type + "|" + Escape(i.Title) + "|" + Escape(i.Genre) + "|" +
               Escape(i.Description) + "|" + i.Length + "|" +
               i.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
               i.IsCompleted + "|" + i.AddedDate.ToString("O");
    }

    // Převede řádek textu zpět na objekt Item. Vrátí null, pokud řádek nelze zpracovat.
    private Item? ParseItem(string line)
    {
        // Přeskočíme prázdné řádky a řádky z exportovaného čitelného formátu
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("=") ||
            line.StartsWith("-") || line.StartsWith("Datum"))
            return null;

        var parts = line.Split('|');
        if (parts.Length < 9) return null; // špatný formát, přeskočíme

        try
        {
            return new Item
            {
                Id          = Guid.TryParse(parts[0], out var g) ? g : Guid.NewGuid(),
                Type        = (ItemType)int.Parse(parts[1]),
                Title       = Unescape(parts[2]),
                Genre       = Unescape(parts[3]),
                Description = Unescape(parts[4]),
                Length      = int.Parse(parts[5]),
                Rating      = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture),
                IsCompleted = bool.Parse(parts[7]),
                AddedDate   = DateTime.Parse(parts[8])
            };
        }
        catch
        {
            // Pokud řádek nelze zpracovat (poškozená data), přeskočíme ho
            return null;
        }
    }

    // Escape: nahradí speciální znaky, aby nenarušily formát souboru
    private string Escape(string s) => s.Replace("|", "\\|").Replace("\n", "\\n");

    // Unescape: obnoví původní znaky po načtení ze souboru
    private string Unescape(string s) => s.Replace("\\|", "|").Replace("\\n", "\n");
}
