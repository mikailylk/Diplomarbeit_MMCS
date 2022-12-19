using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Calc;
using System.Numerics;

namespace ICU_App.ViewModel;

public partial class DebugViewModel : ObservableObject
{
    IOrientationSensor orientationSensor;

    SensorSpeed speed;
    AngleCalc calc;
    Vector3 euler;

    double standard_x;
    double standard_y;
    double standard_z;

    [ObservableProperty]
    string orientationDebug;
    [ObservableProperty]
    string orientationEulerDebug;
    [ObservableProperty]
    string orientationDirDebug;

    public DebugViewModel(IOrientationSensor orientationSensor)
    {
        // Konstruktor für Android device
        if (orientationSensor != null)
        {
            this.orientationSensor = orientationSensor;
        }

        // Überprüfen ob Orientation Sensor unterstützt wird
        if (this.orientationSensor.IsSupported)
        {
            speed = SensorSpeed.Fastest;
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
            calc = new AngleCalc();
            euler = new Vector3();
        }
    }

    public DebugViewModel()
    {
        // Konstruktor für PC (Windows besitzt kein Orientation Sensor)
    }

    [RelayCommand(CanExecute = nameof(CanStart_Stop_Debug))]
    private void Start_Stop_Debug()
    {
        if (this.orientationSensor.IsSupported)
        {
            if (orientationSensor.IsMonitoring)
            {
                orientationSensor.Stop();
            }
            else if (!orientationSensor.IsMonitoring)
            {
                orientationSensor.Start(speed);
            }
        }
        
    }

    private bool CanStart_Stop_Debug()
    {
        if (this.orientationSensor != null)
        {
            return true;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanSet_Position))]
    private void Set_Position()
    {
        standard_x = (euler.X * 180 / Math.PI);
        standard_y = (euler.Y * 180 / Math.PI);
        standard_z = (euler.Z * 180 / Math.PI);
    }

    private bool CanSet_Position()
    {
        if (this.orientationSensor != null)
        {
            return true;
        }
        return false;
    }

    private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        var data = e.Reading;


        // TODO: Binding
        OrientationDebug = "Orient: \n\r" +
            $"X: {Math.Round(data.Orientation.X, 3)}, Y: {Math.Round(data.Orientation.Y, 3)}, Z: {Math.Round(data.Orientation.Z, 3)}, W: {Math.Round(data.Orientation.W, 3)}";

        Quaternion q = new Quaternion((float)Math.Round(data.Orientation.X, 3), (float)Math.Round(data.Orientation.Y, 3), (float)Math.Round(data.Orientation.Z, 3), (float)Math.Round(data.Orientation.W, 3));
        euler = calc.ToEulerAngles(q);
        // OrientationEulerDebug = $"Euler\n\rX: {(int)(euler.X * 180 / Math.PI)} \n\rY: {(int)(euler.Y * 180 / Math.PI)} \n\rZ: {(int)(euler.Z * 180 / Math.PI)}";

        //double yaw = euler.Z * 180 / Math.PI;       // ("horizontale Neigung des Smartphones")
        //double pitch = euler.Y * 180 / Math.PI;     // ("Schrägheit des Smartphones")   
        //double roll = euler.X * 180 / Math.PI;      // ("vertikale Neigung des Smartphones" -> abhängig von Ausrichtung des Smartphones)

        double yaw = euler.Z;
        double pitch = euler.Y;
        double roll = euler.X;


        OrientationEulerDebug = $"Dir\n\r{nameof(pitch)} {(int)(pitch)} \n\r{nameof(roll)} {(int)(roll)} \n\r{nameof(yaw)} {(int)(yaw)}\n\r";
        // Richtung (Vertikal, Horizontal Links, Horizontal Rechts)
        // Y ... -90 Links (Handy aufkippen)
        // Y ... +90 Rechts (Handy aufkippen)
        // Z ... Kopf links rechts drehen

        
    }
}

