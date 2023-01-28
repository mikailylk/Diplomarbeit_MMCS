using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Model;
using ICU_App.View;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using ICU_App.Helper;
using System.Net;
using System.Text.Json;
using Mapsui.Providers.Wms;
using ExCSS;
using ICU_App.Calc;
using System.Numerics;
using Microsoft.Maui.Devices.Sensors;

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

    private double _longtitude_phone;
    private double _latitude_phone;

    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    private UDPListener _udplistener;
    private CancellationTokenSource _cancelListenerTokenSource;

    private UDPClient _udpclient;
    private CancellationTokenSource _cancelClientTokenSource;


    private IOrientationSensor _orientationSensor;
    private SensorSpeed speed = SensorSpeed.UI;

    AngleCalc calc;
    Vector3 angles;

    Quaternion originQ = new Quaternion(0, 0, 0, 1);

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
            calc = new AngleCalc();
            angles = new Vector3();
        }
    }

    protected override async void OnActivated()
    {
        base.OnActivated();
        Camurl = $"http://{settingsmodel.raspi_ip}:8082/index.html";

        //camurl = new UrlWebViewSource();
        //camurl.Url = new Uri("https://www.google.de").ToString();
        //UDP-Server erstellen und starten

        IPEndPoint socket = new IPEndPoint(IPAddress.Any, 8086); // Empfängersocket für PICO
        _udplistener = new UDPListener(socket); // Listener für PICO
        _cancelListenerTokenSource = new CancellationTokenSource();

        _cancelClientTokenSource = new CancellationTokenSource();
        _udplistener.cancellationTokenSource = _cancelListenerTokenSource;


        // Client starten, um Daten an Raspberry Pi Zero zu verschicken
        _udpclient = UDPClient.ConnectTo(settingsmodel.raspi_ip, 8088);
        _udpclient.cancellationTokenSource = _cancelClientTokenSource;

        // Camurl = @"https://www.google.com";
        // Camurl = settingsmodel.raspi_ip;

        // Kartenmaterial aufbereiten
        SetupMap();

        // Server und Client laufenlassen
        Task.Run(async() => RunServer(), _udplistener.cancellationTokenSource.Token);

        Task.Run(async () => RunClient(), _udpclient.cancellationTokenSource.Token);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        // TODO: Koordinaten in einer Liste abspeichern --> am Ende die Telemetrydaten und Koordinaten der Drohen & Handy in einem File abspeichern
        CancelRequest();
        Cancel_Server();
        Cancel_Client();

        OrientationSensor.ReadingChanged -= OrientationSensor_ReadingChanged;
    }

    #region Karte
    private async void SetupMap()
    {
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        mapView.Map.CRS = "EPSG:4326";  // Longtitude Latitude verwenden

        mapView.Map.Widgets.Clear();

        mapView.Pins.Add(new Mapsui.UI.Maui.Pin()
        {
            Position = new Position(47.271408, 9.624545),
            Type = PinType.Pin,
            Label = "Zero point",
            Address = "Zero point",
            Scale = 0.7F
        });

        // mapView.Pins.Remove()

        //Location vom Smartphone GPS holen
        await GetCurrentLocation();

        #region Punkt zoomen
        //Position pos = new Position(47.271408, 9.624545);
        //MPoint mp = pos.ToMapsui();

        //mapView.Navigator.NavigateTo(mp, mapView.Map.Resolutions[16]);
        #endregion Punkt zoomen


        #region rect
        //Position dronepos = new Position(47.271408 + 0.001, 9.624545 + 0.001);
        //MPoint mpdrone = dronepos.ToMapsui();

        //Position pos = new Position(47.271408 - 0.0001, 9.624545 - 0.0001);
        //MPoint mp = pos.ToMapsui();


        //double x = mp.X;
        //double y = mp.Y;


        //double x2 = mpdrone.X;
        //double y2 = mpdrone.Y;

        //MRect rect = new MRect(x, y, x2, y2);

        //mapView.Navigator.NavigateTo(rect, ScaleMethod.Fit, 0);
        #endregion

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

    private void ZoomToPoint(double longtitude, double latitude, Mapsui.UI.Maui.MapView mpview)
    {
        // Die Koordinaten in einen Punkt umwandeln und auf Karte hineinzoomen
        Position currentpos = new Position(latitude, longtitude);
        mapView.MyLocationLayer.UpdateMyLocation(currentpos);
        MPoint currentpos_point = currentpos.ToMapsui();

        // Hineinzoomen
        mapView.Navigator.NavigateTo(currentpos_point, mapView.Map.Resolutions[19]);
    }

    private void ZoomToRectangle(double longtitude_rc_device, double latitude_rc_device)
    {
        Position rcdevice_pos = new Position(47.271408 + 0.001, 9.624545 + 0.001);
        MPoint mp_rcdevice = rcdevice_pos.ToMapsui();

        Position phone_pos = new Position(47.271408 - 0.0001, 9.624545 - 0.0001);
        MPoint mp_phone = phone_pos.ToMapsui();

        // x1 --> Bottom Left
        // x2 --> Top Right
        // y1 --> Bottom Left
        // y2 --> Top Right


        double x1 = mp_phone.X;
        double y1 = mp_phone.Y;

        double x2 = mp_rcdevice.X;
        double y2 = mp_rcdevice.Y;

        double x_temp;
        double y_temp;

        if (x1 > x2)
        {
            x_temp = x2;
            x2 = x1;
            x1 = x_temp;
        }
        if (y1 > y2)
        {
            y_temp = y2;
            y2 = y1;
            y1 = y_temp;
        }
        MRect rect = new MRect(x1, y1, x2, y2);

        mapView.Navigator.NavigateTo(rect, ScaleMethod.Fit, 0);
    }
    #endregion


    private async void RunServer()
    {
        try
        {
            while (!_udplistener.cancellationTokenSource.IsCancellationRequested) // udplistener wirft exception (cancelled), when server geschlossen werden soll
            {
                // Daten von Pico abwarten
                // var received = await _udplistener.Receive();
                // Daten an Raspberry Pi Zero verschicken
                //CommunicationData communicationData = JsonSerializer.Deserialize<CommunicationData>(received.Message.ToString());
                CommunicationData communicationData = new CommunicationData();
                communicationData.Yaw = 5;
                communicationData.Pitch = 10;
                communicationData.Roll = 5;
                communicationData.Power = 10;
                // TODO: Daten von Orientationsensor holen
                communicationData.PitchG = (int)angles.Z;
                communicationData.YawG = (int)angles.X;
                communicationData.RollG = (int)angles.Y;

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
                // TODO: Telemetrydaten in eine Liste geben und am Ende in einem File abspeichern
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

        if (originQ == Quaternion.Identity)
        {
            originQ = Quaternion.Inverse(e.Reading.Orientation);
        }

        var q = Quaternion.Multiply(originQ, e.Reading.Orientation);

        angles = calc.quaternion2Euler(q, AngleCalc.RotSeq.YZX);

        // TODO: Winkel immer zwischen 0 und 360°
        string s = $"Dir\n\rPitch: {(double)(angles.Z)} \n\rRoll {(double)(angles.Y)} \n\rYaw {(double)angles.X}\n\r";
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
        originQ.X = 0;
        originQ.Y = 0;
        originQ.Z = 0;
        originQ.W = 1;
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
                ZoomToPoint(location.Longitude, location.Latitude, MapView);
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
