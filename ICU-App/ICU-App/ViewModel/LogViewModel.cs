using CommunityToolkit.Mvvm.ComponentModel;
using ICU_App.Helper;
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


    private TelemetryDataCollection _telemetryDataCollection;

    public LogViewModel()
    {
        _telemetryDataCollection = new TelemetryDataCollection();
        telemetry_data_chart_collection = new TelemetryDataChartModelCollection();
    }

    partial void OnSelected_av_data_indexChanged(int value)
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
        }
    }

    protected override async void OnActivated()
    {
        base.OnActivated();
        _telemetryDataCollection.telemetryDataCollection.Clear();
        Available_data = await _telemetryDataCollection.GetListOfJsonDataStartStop();

        if (Available_data != null)
        {
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
