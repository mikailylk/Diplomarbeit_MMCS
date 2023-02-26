using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Model;
using Mapsui.UI.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.ViewModel;

/// <summary>
/// The ViewModel for the LogPage, which handles loading the telemetry data 
/// and controlling the display of the chart and map.
/// </summary>
public partial class LogViewModel : ObservableRecipient
{
    /// <summary>
    /// An observable collection of the available telemetry data (binding).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> available_data;

    /// <summary>
    /// The index of the selected telemetry data log (binding).
    /// </summary>
    [ObservableProperty]
    private int selected_av_data_index = -1;

    /// <summary>
    /// The collection of telemetry data chart models (binding).
    /// </summary>
    [ObservableProperty]
    private TelemetryDataChartModelCollection telemetry_data_chart_collection;

    /// <summary>
    /// A boolean indicating, if the chart is checked.
    /// </summary>
    [ObservableProperty]
    private bool ischecked_diagram = true;

    /// <summary>
    /// A boolean indicating if the trace-map is checked.
    /// </summary>
    [ObservableProperty]
    private bool ischecked_trace = false;

    /// <summary>
    /// A boolean indicating if the chart is visible.
    /// </summary>
    [ObservableProperty]
    private bool isChartVisible = true;

    /// <summary>
    /// A boolean indicating if the map is visible.
    /// </summary>
    [ObservableProperty]
    private bool isMapVisible = true;

    /// <summary>
    /// The MapView used in the LogPage.
    /// </summary>
    [ObservableProperty]
    MapView mapView;

    /// <summary>
    /// The collection of telemetry data.
    /// </summary>
    private TelemetryDataCollection _telemetryDataCollection;

    /// <summary>
    /// Initializes a new instance of the LogViewModel class.
    /// </summary>
    public LogViewModel()
    {
        _telemetryDataCollection = new TelemetryDataCollection();
        telemetry_data_chart_collection = new TelemetryDataChartModelCollection();
    }

    /// <summary>
    /// A command that executes when the RadioButton selection is changed.
    /// </summary>
    /// <param name="selection_type">The type of selection.</param>
    [RelayCommand]
    private void ExecuteRBChanged(string selection_type)
    {
        if(selection_type == "Show diagram")
        {
            Ischecked_trace = false;
            Ischecked_diagram = true;
            IsChartVisible = true;
            IsMapVisible = false;
        }
        else
        {
            Ischecked_trace = true;
            Ischecked_diagram = false;
            IsChartVisible = false;
            IsMapVisible = true;
        }
    }

    /// <summary>
    /// A method that is called when the selected_av_data_index is changed.
    /// </summary>
    /// <param name="value">The new value of the selected_av_data_index.</param>
    partial void OnSelected_av_data_indexChanged(int value)
    {

        if(Selected_av_data_index >= 0 && !Available_data[0].ToString().StartsWith("No log"))
        {
            Task<bool> task = Task.Run(async () =>
            {
                return await _telemetryDataCollection.ReadFromJSON(
                                DateTime.ParseExact(Available_data[Selected_av_data_index].Split(" ")[1],
                                "dd-MM-yyyy--HH-mm-ss", CultureInfo.InvariantCulture),
                                DateTime.ParseExact(Available_data[Selected_av_data_index].Split(" ")[3],
                                "dd-MM-yyyy--HH-mm-ss", CultureInfo.InvariantCulture));
            });
            bool loaded = task.Result;

            // check if any data available
            if (loaded)
            {
                Telemetry_data_chart_collection = TelemetryDataChartModelCollection.ParseTelemetryDataCollection(_telemetryDataCollection);

                List<double> longitude = new List<double>();
                List<double> latitude = new List<double>();

                for (int i = 0; i < _telemetryDataCollection.telemetryDataCollection.Count; i++)
                {
                   
                    longitude.Add(_telemetryDataCollection.telemetryDataCollection[i].LONGITUDE);
                    latitude.Add(_telemetryDataCollection.telemetryDataCollection[i].LATITUDE);
                }

                double longitudemin = longitude.Min();
                double latitudemin = latitude.Min();

                double longitudemax = longitude.Max();
                double latitudemax = latitude.Max();

                Mapsui_Map.DrawTrace(MapView, longitude, latitude);
                Mapsui_Map.ZoomToRectangle(longitudemax, latitudemax, longitudemin, latitudemin, MapView);
            }
        }
    }

    /// <summary>
    /// Overrides the method called when the ViewModel is activated, 
    /// loads the available telemetry data and setups the map material.
    /// </summary>
    protected override async void OnActivated()
    {
        base.OnActivated();

        _telemetryDataCollection.telemetryDataCollection.Clear();
        Available_data = await _telemetryDataCollection.GetListOfAvailableJsonData();

        if (!Available_data[0].ToString().StartsWith("No log"))
        {
            Mapsui_Map.SetupMapMaterial(MapView);
        }
        Selected_av_data_index = 0;
    }

    /// <summary>
    /// Overrides the method called when the ViewModel is deactivated and 
    /// clearing the collections.
    /// </summary>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        _telemetryDataCollection.telemetryDataCollection.Clear();
        Telemetry_data_chart_collection.Clear();
    }
}
