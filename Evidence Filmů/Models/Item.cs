using System;

namespace Evidence_Filmů.Models;

// Typ záznamu - Film nebo Kniha
// Film = 0 (výchozí), Book = 1
public enum ItemType { Film, Book }

// Třída reprezentující jeden záznam (film nebo kniha)
public class Item
{
    // Jedinečný identifikátor záznamu (generuje se automaticky)
    public Guid Id { get; set; } = Guid.NewGuid();

    // Typ záznamu: Film nebo Kniha
    public ItemType Type { get; set; }

    // Název díla
    public string Title { get; set; } = "";

    // Žánr (např. Akce, Drama, Sci-Fi)
    public string Genre { get; set; } = "";

    // Krátký textový popis
    public string Description { get; set; } = "";

    // Délka: počet minut (film) nebo počet stran (kniha)
    public int Length { get; set; }

    // Hodnocení od 0.0 do 10.0
    public double Rating { get; set; }

    // Příznak, zda byl záznam dokončen (zhlédnut / přečten)
    public bool IsCompleted { get; set; }

    // Datum přidání záznamu do evidence
    public DateTime AddedDate { get; set; } = DateTime.Now;

    // ─── Pomocné zobrazovací vlastnosti ───────────────────────────────────────

    // Popisek jednotky délky podle typu (minuty / strany)
    public string LengthLabel => Type == ItemType.Book ? "stran" : "minut";

    // Popisek dokončení podle typu (Přečteno / Zhlédnuto)
    public string CompletedLabel => Type == ItemType.Book ? "Přečteno" : "Zhlédnuto";

    // Štítek s emoji pro zobrazení v seznamu (🎬 Film / 📚 Kniha)
    public string TypeLabel => Type == ItemType.Book ? "📚 Kniha" : "🎬 Film";

    // Formátované hodnocení pro zobrazení v seznamu (např. "⭐ 8.5")
    public string RatingLabel => $"⭐ {Rating:F1}";

    // Stav dokončení s emoji pro zobrazení v seznamu
    public string StatusDisplay => IsCompleted ? $"✅ {CompletedLabel}" : "⏳ Nedokončeno";
}