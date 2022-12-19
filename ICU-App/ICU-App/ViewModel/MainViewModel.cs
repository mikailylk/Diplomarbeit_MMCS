using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Model;
using ICU_App.View;

namespace ICU_App.ViewModel;

[QueryProperty(nameof(Settingsmodel), nameof(Settingsmodel))]
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    SettingsModel settingsmodel;

    [RelayCommand]
    async Task Back()
    {
        await Shell.Current.GoToAsync($"{nameof(SettingsPage)}", true);
    }

    public MainViewModel()
    {}
}
