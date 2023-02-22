using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Model;
using Mapsui;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using ICU_App.Helper;
using System.Net;
using System.Text.Json;
using ICU_App.Calc;
using System.Numerics;
using System.Diagnostics;

namespace ICU_App.ViewModel;

[QueryProperty(nameof(Settingsmodel), nameof(Settingsmodel))]
public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    SettingsModel settingsmodel;

    [ObservableProperty]
    string camurl;

    [ObservableProperty]
    Mapsui.UI.Maui.MapView mapView;

    private double _longitude_phone;
    private double _latitude_phone;

    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    private UDPListener _udplistener;
    private CancellationTokenSource _cancelListenerTokenSource;

    private UDPClient _udpclient;
    private CancellationTokenSource _cancelClientTokenSource;

    [ObservableProperty]
    private string telemetry;

    private TelemetryDataCollection _telemetryDataCollection;

    private IOrientationSensor _orientationSensor;
    private SensorSpeed speed = SensorSpeed.UI;

    private AngleCalc _calc;
    private Vector3 _angles;

    private Quaternion _originQ = new Quaternion(0, 0, 0, 1);

    public MainViewModel()
    {}

    public MainViewModel(IOrientationSensor orientationSensor)
    {
        // Konstruktor für Android device
        if (orientationSensor != null)
        {
            _orientationSensor = orientationSensor;
        }

        // Überprüfen ob Orientation Sensor unterstützt wird
        if (_orientationSensor.IsSupported)
        {
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
            _calc = new AngleCalc();
            _angles = new Vector3();
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        Camurl = $"http://{settingsmodel.raspi_ip}:8082/index.html";

        //UDP-Server erstellen und starten

        IPEndPoint socket = new IPEndPoint(IPAddress.Any, 8086); // Empfängersocket für PICO
        _udplistener = new UDPListener(socket); // Listener für PICO
        _cancelListenerTokenSource = new CancellationTokenSource();

        _cancelClientTokenSource = new CancellationTokenSource();
        _udplistener.cancellationTokenSource = _cancelListenerTokenSource;


        // Client starten, um Daten an Raspberry Pi Zero zu verschicken
        _udpclient = UDPClient.ConnectTo(settingsmodel.raspi_ip, 8088);
        _udpclient.cancellationTokenSource = _cancelClientTokenSource;

        // Kartenmaterial aufbereiten
        SetupMap();

        // Server und Client laufenlassen
        Task.Run(() => RunServer(), _udplistener.cancellationTokenSource.Token);

        Task.Run(() => RunClient(), _udpclient.cancellationTokenSource.Token);


    }

    protected override async void OnDeactivated()
    {
        base.OnDeactivated();
        // TODO: Koordinaten in einer Liste abspeichern --> am Ende die Telemetrydaten und Koordinaten der Drohne & Handy in einem File abspeichern
        CancelRequest();
        Cancel_Server();
        Cancel_Client();

        _orientationSensor.Stop();
        OrientationSensor.ReadingChanged -= OrientationSensor_ReadingChanged;

        // Abspeicherung der Telemetrie-Daten als CSV
        _telemetryDataCollection.FillWithDummyData(_longitude_phone, _latitude_phone);
        bool saving_json_stat = await _telemetryDataCollection.SaveToJSON();
        bool savingstat = await _telemetryDataCollection.SaveToCSVFile();

        string message = "Saving the telemetry data succeeded!\n\r" +
            $"JSON: {saving_json_stat}\n\rCSV: {savingstat}";

        if (savingstat && saving_json_stat)
        {
            await Shell.Current.DisplayAlert("Information", message, "OK");
        }
        else
        {
            message = "Saving the telemetry data failed\r\n" +
                $"JSON: {saving_json_stat}\n\rCSV: {savingstat}";

            await Shell.Current.DisplayAlert("Information", message, "OK");
        }

    }

    private async void SetupMap()
    {

        Mapsui_Map.SetupMapMaterial(MapView);
        Mapsui_Map.DrawRedPin(MapView, 9.624545, 47.271408);

        //Location vom Smartphone GPS holen
        await GetCurrentLocation();

        // setup telemetry data collection
        _telemetryDataCollection = new TelemetryDataCollection(_longitude_phone, _latitude_phone);


        #region Tracezeichnen
        //// Trace einzeichnen https://github.com/Mapsui/Mapsui/blob/master/Samples/Mapsui.Samples.Forms/Mapsui.Samples.Forms.Shared/PolylineSample.cs
        //Polyline polylin = new Polyline()
        //{
        //    StrokeWidth = 4,
        //    StrokeColor = Colors.Red,
        //    IsClickable = true
        //};

        //polylin.Positions.Add(new Position(0, 0));
        //polylin.Positions.Add(new Position(2, 10));
        //polylin.Positions.Add(new Position(5, 40));
        //polylin.Positions.Add(new Position(47.271408, 9.624545));

        //mapView.Drawables.Add(polylin);
        #endregion
    }


    private async void RunServer()
    {
        try
        {
            while (!_udplistener.cancellationTokenSource.IsCancellationRequested) // udplistener wirft exception (cancelled), when server geschlossen werden soll
            {
                // Daten von Pico abwarten
                var received = await _udplistener.Receive();
                // Daten an Raspberry Pi Zero verschicken
                CommunicationData communicationData = JsonSerializer.Deserialize<CommunicationData>(received.Message.ToString());

                // {"Pitch":999,"Roll":555,"Yaw":888,"Power":666,"PitchG":777,"RollG":766,"YawG":944}

                //CommunicationData communicationData = new CommunicationData();
                //communicationData.Yaw = 5;
                //communicationData.Pitch = 10;
                //communicationData.Roll = 5;
                //communicationData.Power = 10;
                // TODO: Daten von Orientationsensor holen
                //communicationData.PitchG = (int)_angles.Z;
                //communicationData.YawG = (int)_angles.X;
                //communicationData.RollG = (int)_angles.Y;

                communicationData.PitchG = (int)15;
                communicationData.YawG = (int)14;
                communicationData.RollG = (int)12;


                string message = JsonSerializer.Serialize(communicationData);

                _udpclient.Send(message);

                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            // Cancellation wurde eingereicht
        }
        finally
        {
            _udplistener?.Close();
        }
    }

    private async void RunClient()
    {
        try
        {
            while (!_udpclient.cancellationTokenSource.IsCancellationRequested)
            {
                // auf Reply von Raspberry Pi Zero warten
                var received = await _udpclient.Receive();
                string message = received.Message;
                TelemetryData telemetryData = JsonSerializer.Deserialize<TelemetryData>(message);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Telemetry = telemetryData.ToString();
                });
                // TODO: Telemetrydaten in eine Liste geben und am Ende in einem File abspeichern
                // for now, just get long & lat of smartphone once and write it as location
                telemetryData.LONGITUDE_SMARTPHONE = _longitude_phone;
                telemetryData.LATITUDE_SMARTPHONE = _latitude_phone;

                _telemetryDataCollection.telemetryDataCollection.Add(telemetryData);
            }
        }
        catch (Exception ex)
        {
            // Cancellation wurde eingereicht
        }
    }

    private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        // https://stackoverflow.com/questions/54540095/xamarin-orientation-sensor-quaternion

        if (_originQ == Quaternion.Identity)
        {
            _originQ = Quaternion.Inverse(e.Reading.Orientation);
        }

        var q = Quaternion.Multiply(_originQ, e.Reading.Orientation);

        _angles = _calc.quaternion2Euler(q, AngleCalc.RotSeq.YZX);

        // TODO: Winkel immer zwischen 0 und 360°
        string s = $"Dir\n\rPitch: {(double)(_angles.Z)} \n\rRoll {(double)(_angles.Y)} \n\rYaw {(double)_angles.X}\n\r";
    }

    [RelayCommand]
    private void StartStopRotatingCam()
    {
        if (_orientationSensor.IsSupported)
        {
            if (_orientationSensor.IsMonitoring)
            {
                _orientationSensor.Stop();
            }
            else if (!_orientationSensor.IsMonitoring)
            {
                _orientationSensor.Start(speed);
                Set_Position();
            }
        }
    }

    private void Set_Position()
    {
        if (_orientationSensor == null)
        {
            return;
        }
        _originQ.X = 0;
        _originQ.Y = 0;
        _originQ.Z = 0;
        _originQ.W = 1;
    }

    private void Cancel_Server()
    {
        if(_cancelListenerTokenSource.IsCancellationRequested == false)
        {
            _cancelListenerTokenSource.Cancel();
        }
    }

    private void Cancel_Client()
    {
        if (_cancelClientTokenSource.IsCancellationRequested == false)
        {
            _cancelClientTokenSource.Cancel();
        }
    }

    #region Location vom Gerät abrufen
    private async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            // Berechtigungen im 
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
            {
                _longitude_phone = location.Longitude;
                _latitude_phone = location.Latitude;

                Mapsui_Map.ZoomToPoint(location.Longitude, location.Latitude, MapView);
            }
        }

        catch (Exception ex)
        // Catch one of the following exceptions:
        //   FeatureNotSupportedException
        //   FeatureNotEnabledException
        //   PermissionException
        {
            // Lokation konnte nicht abgerufen werden
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    private void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
    #endregion

    [RelayCommand]
    async Task Back()
    {
        // Zurücknavigieren (Settingspage)
        await Shell.Current.GoToAsync($"..", true);
    }

}
