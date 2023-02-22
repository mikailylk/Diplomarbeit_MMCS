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
using Microsoft.Maui.Controls;

namespace ICU_App.ViewModel;

/// <summary>
/// This class is the view model for the MainPage. 
/// It includes the following functionalities:
/// - Handles the orientation sensor data to control gimbal and get the orientation angles.
/// - Receives and sends data from/to the Raspberry Pi Pico and Raspberry Pi Zero using UDP protocol.
/// - Gets the location of the smartphone and sets the location on the map.
/// - Saves the telemetry data to JSON and CSV files.
/// </summary>

[QueryProperty(nameof(Settingsmodel), nameof(Settingsmodel))]
public partial class MainViewModel : ObservableRecipient
{
    /// <summary>
    /// A property that represents the SettingsModel of the application.
    /// </summary>
    [ObservableProperty]
    SettingsModel settingsmodel;

    /// <summary>
    /// A property that represents the URL of the camera view in the MainPage.
    /// </summary>
    [ObservableProperty]
    string camurl;

    /// <summary>
    /// A property that represents the MapView of the application.
    /// </summary>
    [ObservableProperty]
    MapView mapView;

    /// <summary>
    /// The longitude of the smartphone's current location.
    /// </summary>
    private double _longitude_phone;

    /// <summary>
    /// The latitude of the smartphone's current location.
    /// </summary>
    private double _latitude_phone;

    private CancellationTokenSource _cancelTokenSource;

    /// <summary>
    /// A boolean that indicates whether the application is currently checking the location of the smartphone.
    /// </summary>
    private bool _isCheckingLocation;

    /// <summary>
    /// A UDPListener that is used to receive data from Raspberry Pi Pico.
    /// </summary>
    private UDPListener _udplistener;
    /// <summary>
    /// A CancellationTokenSource that is used to cancel the UDPListener.
    /// </summary>
    private CancellationTokenSource _cancelListenerTokenSource;

    /// <summary>
    /// A UDPClient that is used to receive/send data from/to Raspberry Pi Zero.
    /// </summary>
    private UDPClient _udpclient;
    /// <summary>
    /// A CancellationTokenSource that is used to cancel the UDPClient.
    /// </summary>
    private CancellationTokenSource _cancelClientTokenSource;

    /// <summary>
    /// A property that represents the telemetry data received from Raspberry Pi Zero.
    /// </summary>
    [ObservableProperty]
    private string telemetry;

    /// <summary>
    /// A TelemetryDataCollection that is used to collect telemetry data.
    /// </summary>
    private TelemetryDataCollection _telemetryDataCollection;

    /// <summary>
    /// An IOrientationSensor that is used to read the orientation sensor data.
    /// </summary>
    private IOrientationSensor _orientationSensor;
    /// <summary>
    /// A SensorSpeed that represents the speed at which the orientation 
    /// sensor data should be read.
    /// </summary>
    private SensorSpeed speed = SensorSpeed.UI;

    /// <summary>
    /// An AngleCalc that is used to calculate the Euler angles representation.
    /// </summary>
    private AngleCalc _calc;

    /// <summary>
    /// A Vector3 that represents the Euler angles of the orientation sensor data.
    /// </summary>
    private Vector3 _angles;

    private Quaternion _originQ = new Quaternion(0, 0, 0, 1);

    public MainViewModel()
    {}

    /// <summary>
    /// The constructor for the MainViewModel class. 
    /// It sets up the orientation sensor if it is supported.
    /// </summary>
    public MainViewModel(IOrientationSensor orientationSensor)
    {
        // constructor for android devices
        if (orientationSensor != null)
        {
            _orientationSensor = orientationSensor;
        }

        // check if orientation sensor is supported
        if (_orientationSensor.IsSupported)
        {
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
            _calc = new AngleCalc();
            _angles = new Vector3();
        }
    }

    /// <summary>
    /// Overrides the method called when the ViewModel is activated, 
    /// sets up the map material, starts the UDP server and client and 
    /// gets the location of the smartphone.
    /// </summary>
    protected override void OnActivated()
    {
        base.OnActivated();
        Camurl = $"http://{settingsmodel.raspi_ip}:8082/index.html";

        // UDP server: receive data from raspberry pi pico, merge them with gimbal data
        // and send it to raspberry pi zero
        IPEndPoint socket = new IPEndPoint(IPAddress.Any, 8086); // Empfängersocket für PICO
        _udplistener = new UDPListener(socket); // Listener für PICO
        _cancelListenerTokenSource = new CancellationTokenSource();

        _cancelClientTokenSource = new CancellationTokenSource();
        _udplistener.cancellationTokenSource = _cancelListenerTokenSource;


        // UDP client: eceive data from raspberry pi zero (telemetry data)
        _udpclient = UDPClient.ConnectTo(settingsmodel.raspi_ip, 8088);
        _udpclient.cancellationTokenSource = _cancelClientTokenSource;

        // setup map material
        SetupMap();

        // run server and client
        Task.Run(() => RunServer(), _udplistener.cancellationTokenSource.Token);

        Task.Run(() => RunClient(), _udpclient.cancellationTokenSource.Token);
    }

    /// <summary>
    /// Overrides the method called when the ViewModel is deactivated,  
    /// saves the telemetry data to JSON and Excel, cancels the requests 
    /// to get the location of the smartphone (if stuck) and 
    /// cancels the UDP server and client.
    /// </summary>
    protected override async void OnDeactivated()
    {
        base.OnDeactivated();
        // TODO: save smartphone coordinates into a list (not static as now)
        CancelRequest();
        Cancel_Server();
        Cancel_Client();

        _orientationSensor.Stop();
        OrientationSensor.ReadingChanged -= OrientationSensor_ReadingChanged;

        // save the telemetry data to Excel and JSON
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

    /// <summary>
    /// This method sets up the map material, draws a red pin on the map, 
    /// gets the current location of smartphone and updates it on the map.
    /// </summary>
    private async void SetupMap()
    {
        // setup map material and draw red pin (for testing)
        Mapsui_Map.SetupMapMaterial(MapView);
        Mapsui_Map.DrawRedPin(MapView, 9.624545, 47.271408);

        // get location of smartphone
        await GetCurrentLocation();

        // setup telemetry data collection
        _telemetryDataCollection = new TelemetryDataCollection(_longitude_phone, _latitude_phone);
    }

    /// <summary>
    /// This method starts the UDP server and listens for data from the 
    /// Raspberry Pi Pico.
    /// </summary>
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

                // get orientation sensor data 
                communicationData.PitchG = (int)_angles.Z;
                communicationData.YawG = (int)_angles.X;
                communicationData.RollG = (int)_angles.Y;

                string message = JsonSerializer.Serialize(communicationData);

                _udpclient.Send(message);

                // 1ms timeout
                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            // cancellation was submitted
        }
        finally
        {
            // close listener
            _udplistener?.Close();
        }
    }

    /// <summary>
    /// This method starts the UDP client and receives data from the Raspberry Pi Zero.
    /// </summary>
    private async void RunClient()
    {
        try
        {
            while (!_udpclient.cancellationTokenSource.IsCancellationRequested)
            {
                // wait for reply from raspberry pi (telemetry data)
                var received = await _udpclient.Receive();
                string message = received.Message;

                // deserialize the telemetry data
                TelemetryData telemetryData = JsonSerializer.Deserialize<TelemetryData>(message);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Telemetry = telemetryData.ToString();
                });
             
                // for now, just get long & lat of smartphone once and write it as location
                telemetryData.LONGITUDE_SMARTPHONE = _longitude_phone;
                telemetryData.LATITUDE_SMARTPHONE = _latitude_phone;

                // add telemetry data to collection
                _telemetryDataCollection.telemetryDataCollection.Add(telemetryData);
            }
        }
        catch (Exception ex)
        {
            // cancellation was submitted
        }
    }

    /// <summary>
    /// This method is called when the orientation sensor data changes. 
    /// It sets the angles of the orientation sensor data and converts to Euler angles.
    /// </summary>
    private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        // https://stackoverflow.com/questions/54540095/xamarin-orientation-sensor-quaternion

        if (_originQ == Quaternion.Identity)
        {
            _originQ = Quaternion.Inverse(e.Reading.Orientation);
        }

        var q = Quaternion.Multiply(_originQ, e.Reading.Orientation);

        // calculate Euler angle representation
        _angles = _calc.quaternion2Euler(q, AngleCalc.RotSeq.YZX);

        // TODO: Winkel immer zwischen 0 und 360°
        string s = $"Dir\n\rPitch: {(double)(_angles.Z)} \n\rRoll {(double)(_angles.Y)} \n\rYaw {(double)_angles.X}\n\r";
    }

    /// <summary>
    /// A command that starts or stops the reading process
    /// of orientation sensor.
    /// </summary>
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

    /// <summary>
    /// This method does offset correction to orientation sensor data.
    /// </summary>
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

    /// <summary>
    /// This method cancels the UDP server.
    /// </summary>
    private void Cancel_Server()
    {
        if(_cancelListenerTokenSource.IsCancellationRequested == false)
        {
            _cancelListenerTokenSource.Cancel();
        }
    }

    /// <summary>
    /// This method cancels the UDP client.
    /// </summary>
    private void Cancel_Client()
    {
        if (_cancelClientTokenSource.IsCancellationRequested == false)
        {
            _cancelClientTokenSource.Cancel();
        }
    }

    /// <summary>
    /// This method gets the current location of smartphone.
    /// </summary>
    #region Location vom Gerät abrufen
    private async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            // get necessary permissions
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
        {
            // location could not be retrieved
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    /// <summary>
    /// This method cancels getting current location.
    /// </summary>
    private void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
    #endregion

    /// <summary>
    /// A command that navigates back to SettingsPage
    /// </summary>
    [RelayCommand]
    private async Task Back()
    {
        // navigate back to SettingsPage
        await Shell.Current.GoToAsync($"..", true);
    }
}
