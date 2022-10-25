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
        this.orientationSensor = orientationSensor;

        // Konstruktor für Android device

        speed = SensorSpeed.Fastest;
        OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
        calc = new AngleCalc();
        euler = new Vector3();
    }

    public DebugViewModel()
    {
        // Konstruktor für PC
    }

    [RelayCommand]
    void Start_Stop_Debug_Command()
    {
        if (orientationSensor != null && orientationSensor.IsMonitoring)
        {
            orientationSensor.Stop();
        }
        else if (orientationSensor != null && !orientationSensor.IsMonitoring)
        {
            orientationSensor.Start(speed);
        }
    }

    [RelayCommand]
    void Set_Position_Command()
    {
        standard_x = (euler.X * 180 / Math.PI);
        standard_y = (euler.Y * 180 / Math.PI);
        standard_z = (euler.Z * 180 / Math.PI);
    }

    private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        var data = e.Reading;
        // TODO: Binding
        OrientationDebug = "Orient: \n\r" +
            $"X: {Math.Round(data.Orientation.X, 4)}, Y: {Math.Round(data.Orientation.Y, 4)}, Z: {Math.Round(data.Orientation.Z, 4)}, W: {Math.Round(data.Orientation.W, 4)}";

        euler = calc.ToEulerAngles(data.Orientation);
        // OrientationEulerDebug = $"Euler\n\rX: {(int)(euler.X * 180 / Math.PI)} \n\rY: {(int)(euler.Y * 180 / Math.PI)} \n\rZ: {(int)(euler.Z * 180 / Math.PI)}";


        // Ausrichtung (Horizontal)
        double yaw = euler.Z * 180 / Math.PI;      // ("horizontale Neigung des Smartphones")
        if (yaw < 0)
        {
            yaw = yaw + 360;
        }
        double pitch = euler.X * 180 / Math.PI;     // ("Schrägheit des Smartphones")   
        double roll = euler.Y * 180 / Math.PI;       // ("vertikale Neigung des Smartphones" -> abhängig von Ausrichtung des Smartphones)
        OrientationEulerDebug = $"Dir\n\r{nameof(pitch)} {(int)(pitch)} \n\r{nameof(roll)} {(int)(roll)} \n\r{nameof(yaw)} {(int)(yaw)}";
        // OrientationDirDebug = $"Pos\n\rX: {((standard_x - euler.X) * 180 / Math.PI)} \n\rY: {(standard_y - euler.Y * 180 / Math.PI)} \n\rZ: {(standard_z - euler.Z * 180 / Math.PI)}";

        // Richtung (Vertikal, Horizontal Links, Horizontal Rechts)
        // Y ... -90 Links (Handy aufkippen)
        // Y ... +90 Rechts (Handy aufkippen)
        // Z ... Kopf links rechts drehen


    }
}

