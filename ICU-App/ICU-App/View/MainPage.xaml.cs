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

/// <summary>
/// The MainPage class is a view that displays the live video, a map for tracking
/// and other UI elements.
/// It contains a reference to an instance of MainViewModel, 
/// which is the view model for this page.
/// </summary>
public partial class MainPage : ContentPage
{
    private MainViewModel _mainViewModel;
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _mainViewModel = vm;

        // Mapsui Maui sadly doesn't support MVVM pattern
        // therefore the mapView object of xaml is used in the ViewModel
        _mainViewModel.MapView = mapView;
    }

    /// <summary>
    /// Overrides the OnNavigatedTo method of the ContentPage class
    /// and sets the IsActive property of the view model to true.
    /// </summary>
    /// <param name="args">
    /// The arguments passed to the OnNavigatedTo method.
    /// </param>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _mainViewModel.IsActive = true;
    }

    /// <summary>
    /// Overrides the OnNavigatedFrom method of the ContentPage class 
    /// and sets the IsActive property of the view model to false.
    /// </summary>
    /// <param name="args">
    /// The arguments passed to the OnNavigatedFrom method.
    /// </param>
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _mainViewModel.IsActive = false;
    }
}

