using CommunityToolkit.Mvvm.ComponentModel;

namespace Evidence_Filmů.ViewModels;

// Základní třída pro všechny ViewModely.
// Dědí z ObservableObject (CommunityToolkit.Mvvm), která zajišťuje:
//   • INotifyPropertyChanged — UI se automaticky překreslí při změně vlastností
//   • Podporu pro [ObservableProperty] atribut (generování vlastností ze polí)
public abstract class ViewModelBase : ObservableObject
{
}
