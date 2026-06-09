using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Evidence_Filmů.ViewModels;
using Evidence_Filmů.Views;

namespace Evidence_Filmů;

// Třída App — vstupní bod Avalonia aplikace.
// Inicializuje XAML a vytváří hlavní okno při spuštění.
public partial class App : Application
{
    // Načte a zpracuje App.axaml (styly, šablony, témata)
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Volá se po inicializaci frameworku — zde spustíme hlavní okno
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Vytvoříme hlavní okno a přiřadíme mu ViewModel jako DataContext
            // DataContext je "mozek" okna — obsahuje data a logiku
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
