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

public partial class LogViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<string> available_data;

    [ObservableProperty]
    private int selected_av_data_index = -1;

    [ObservableProperty]
    private TelemetryDataChartModelCollection telemetry_data_chart_collection;

    [ObservableProperty]
    private bool ischecked_diagram = true;

    [ObservableProperty]
    private bool ischecked_trace = false;

    [ObservableProperty]
    private bool isChartVisible = true;

    [ObservableProperty]
    private bool isMapVisible = false;

    [ObservableProperty]
    MapView mapView;

    private TelemetryDataCollection _telemetryDataCollection;

    public LogViewModel()
    {
        _telemetryDataCollection = new TelemetryDataCollection();
        telemetry_data_chart_collection = new TelemetryDataChartModelCollection();
    }

    [RelayCommand]
    private void ExecuteRBChangedCommand(string selection_type)
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

    partial void OnSelected_av_data_indexChanged(int value)
    {

        if(Selected_av_data_index >= 0)
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
                // Mapsui_Map.ZoomToPoint(longitudemin, latitudemin, MapView);
                Mapsui_Map.ZoomToRectangle(longitudemax, latitudemax, longitudemin, latitudemin, MapView);
            }
        }
        
    }

    protected override async void OnActivated()
    {
        base.OnActivated();
        _telemetryDataCollection.telemetryDataCollection.Clear();
        Available_data = await _telemetryDataCollection.GetListOfJsonDataStartStop();

        if (Available_data != null)
        {
            Mapsui_Map.SetupMapMaterial(MapView);
            Selected_av_data_index = 0;
        }
        else
        {
            Available_data.Clear();
            Available_data.Add("No log entry!");
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        _telemetryDataCollection.telemetryDataCollection.Clear();
    }
}
