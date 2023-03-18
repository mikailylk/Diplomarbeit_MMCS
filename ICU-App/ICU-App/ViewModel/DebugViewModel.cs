// using Android.Views.Animations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Calc;
using System.Numerics;
//using static Android.Resource;

namespace ICU_App.ViewModel;

public partial class DebugViewModel : ObservableObject
{
    IOrientationSensor orientationSensor;

    SensorSpeed speed;
    AngleCalc calc;

    Vector3 angles;

    double x, y, z;

    [ObservableProperty]
    string orientationDebug;
    [ObservableProperty]
    string orientationEulerDebug;
    [ObservableProperty]
    string orientationDirDebug;

    public DebugViewModel(IOrientationSensor orientationSensor)
    {
        // constructor for android devices
        if (orientationSensor != null)
        {
            this.orientationSensor = orientationSensor;
        }

        // check if orientationsensor is supported
        if (this.orientationSensor.IsSupported)
        {
            speed = SensorSpeed.UI;
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
            calc = new AngleCalc();
            angles = new Vector3();
            x = 0;
            y = 0;
            z = 0;
        }
    }

    public DebugViewModel()
    {}

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
        originQ.X = 0;
        originQ.Y = 0;
        originQ.Z = 0;
        originQ.W = 1;
    }

    private bool CanSet_Position()
    {
        if (this.orientationSensor != null)
        {
            return true;
        }
        return false;
    }

    Quaternion originQ = new Quaternion(0,0,0,1);
    private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        var q = e.Reading.Orientation;

        angles = calc.quaternion2Euler(q, AngleCalc.RotSeq.ZXY);

        var data = e.Reading;
        // TODO: Binding
        OrientationDebug = "Orient: \n\r" +
            $"X: {Math.Round(data.Orientation.X, 3)}, Y: {Math.Round(data.Orientation.Y, 3)}, Z: {Math.Round(data.Orientation.Z, 3)}, W: {Math.Round(data.Orientation.W, 3)}";

        x = angles.X;
        y = angles.Y;
        z = angles.Z;
        OrientationEulerDebug = $"Dir\n\rPitch: {(double)(x)} \n\rRoll {(double)(y)} \n\rYaw {(double)z}\n\r";
        //OrientationEulerDebug = $"Dir\n\rPitch: {(double)(x)} \n\rRoll {(double)(z)} \n\rYaw {(double)y}\n\r";
    }
}

