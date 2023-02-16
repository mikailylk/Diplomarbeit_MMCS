using System.Numerics;
using ICU_App.Calc;
using ICU_App.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Mapsui.Tiling;
using Mapsui;
using Mapsui.Widgets;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Extensions;
using Mapsui.Utilities;
using Mapsui.Projections;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using BruTile.Wms;
using CommunityToolkit.Mvvm.Input;

namespace ICU_App.View;

public partial class MainPage : ContentPage
{
    private MainViewModel _mainViewModel;
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _mainViewModel = vm;

        // Mapsui Maui unterstützt leider nicht einen schönen MVVM Pattern --> mapView Objekt vom xaml im ViewModel nutzen
        _mainViewModel.MapView = mapView;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _mainViewModel.IsActive = true;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _mainViewModel.IsActive = false;
    }

    // need to check if neccessary
    private void MainPage_Loaded(object sender, EventArgs e)
    {
        // MainView ist geladen --> relevante Daten bearbeiten
        _mainViewModel.IsActive = true;
    }

    private void MainPage_Unloaded(object sender, EventArgs e)
    {
        _mainViewModel.IsActive = false;
    }
}

