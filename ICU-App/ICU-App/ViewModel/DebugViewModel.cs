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
    Vector3 retAngles;

    Quaternion old_q = new Quaternion(0, 0, 0, 0);
    Quaternion new_q;

    //double deltax = 0;
    //double deltaz = 0;

    //double oldx = 0;
    //double oldz = 0;

    double x, y, z;

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
            speed = SensorSpeed.UI;
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
            calc = new AngleCalc();
            angles = new Vector3();
            retAngles = new Vector3();
            x = 0;
            y = 0;
            z = 0;
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
        // https://stackoverflow.com/questions/54540095/xamarin-orientation-sensor-quaternion
        
        if (originQ == Quaternion.Identity)
        {
            originQ = Quaternion.Inverse(e.Reading.Orientation);
        }

        var q = Quaternion.Multiply(originQ, e.Reading.Orientation);

        //angles = calc.ToEulerAngles(q);
        //angles.Z = (float)(angles.Z * 180 / Math.PI);    // ("horizontale Neigung des Smartphones")
        //angles.Y = (float)(angles.Y * 180 / Math.PI);    // ("Schrägheit des Smartphones")   
        //angles.X = (float)(angles.X * 180 / Math.PI);    // ("vertikale Neigung des Smartphones" -> abhängig von Ausrichtung des Smartphones)

        angles = calc.quaternion2Euler(q,AngleCalc.RotSeq.YZX);

        var data = e.Reading;
        // TODO: Binding
        OrientationDebug = "Orient: \n\r" +
            $"X: {Math.Round(data.Orientation.X, 3)}, Y: {Math.Round(data.Orientation.Y, 3)}, Z: {Math.Round(data.Orientation.Z, 3)}, W: {Math.Round(data.Orientation.W, 3)}";


        ////Quaternion q = new Quaternion((float)data.Orientation.X, 3), (float)Math.Round(data.Orientation.Y, 3), (float)Math.Round(data.Orientation.Z, 3), (float)Math.Round(data.Orientation.W, 3));

        //Quaternion q = e.Reading.Orientation;

        //Quaternion delta = old_q * Quaternion.Inverse(q);

        //old_q = q;

        //angles = calc.ToEulerAngles(delta);
        //// OrientationEulerDebug = $"Euler\n\rX: {(int)(euler.X * 180 / Math.PI)} \n\rY: {(int)(euler.Y * 180 / Math.PI)} \n\rZ: {(int)(euler.Z * 180 / Math.PI)}";

        //angles.Z = (float)(angles.Z * 180 / Math.PI);   // ("horizontale Neigung des Smartphones")
        //angles.Y = (float)(angles.Y * 180 / Math.PI);   // ("Schrägheit des Smartphones")   
        //angles.X = (float)(angles.X * 180 / Math.PI);   // ("vertikale Neigung des Smartphones" -> abhängig von Ausrichtung des Smartphones)

        x = angles.X;
        y = angles.Y;
        z = angles.Z;
        OrientationEulerDebug = $"Dir\n\rPitch: {(double)(z)} \n\rRoll {(double)(y)} \n\rYaw {(double) x}\n\r";
    }
    //private void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    //{
    //    var data = e.Reading;
    //    // TODO: Binding
    //    OrientationDebug = "Orient: \n\r" +
    //        $"X: {Math.Round(data.Orientation.X, 3)}, Y: {Math.Round(data.Orientation.Y, 3)}, Z: {Math.Round(data.Orientation.Z, 3)}, W: {Math.Round(data.Orientation.W, 3)}";

    //    //Quaternion q = new Quaternion((float)data.Orientation.X, 3), (float)Math.Round(data.Orientation.Y, 3), (float)Math.Round(data.Orientation.Z, 3), (float)Math.Round(data.Orientation.W, 3));

    //    Quaternion q = e.Reading.Orientation;

    //    Quaternion delta = old_q * Quaternion.Inverse(q);

    //    old_q = q;

    //    angles = calc.ToEulerAngles(delta);
    //    // OrientationEulerDebug = $"Euler\n\rX: {(int)(euler.X * 180 / Math.PI)} \n\rY: {(int)(euler.Y * 180 / Math.PI)} \n\rZ: {(int)(euler.Z * 180 / Math.PI)}";

    //    angles.Z = (float)(angles.Z * 180 / Math.PI);       // ("horizontale Neigung des Smartphones")
    //    angles.Y = (float)(angles.Y * 180 / Math.PI);     // ("Schrägheit des Smartphones")   
    //    angles.X = (float)(angles.X * 180 / Math.PI);      // ("vertikale Neigung des Smartphones" -> abhängig von Ausrichtung des Smartphones)

    //    //if (Math.Abs(angles.Y) < 85)
    //    //{
    //    //    deltax = angles.X - oldx;
    //    //    deltaz = angles.Z - oldz;

    //    //    if (deltax < -100)
    //    //    {
    //    //        angles.X = angles.X + 180;
    //    //        retAngles.Y = -90 + (-90 - angles.Y);
    //    //    }
    //    //    else
    //    //    {
    //    //        retAngles.Y = angles.Y;
    //    //    }
    //    //    if (deltaz < -100)
    //    //    {
    //    //        angles.Z = angles.Z + 180;
    //    //    }

    //    //    if (deltax > 100)
    //    //    {
    //    //        angles.X = angles.X - 180;
    //    //        retAngles.Y = 90 + (90 - angles.Y);
    //    //    }

    //    //    if (deltaz > 100)
    //    //    {
    //    //        angles.Z = angles.Z - 180;
    //    //    }

    //    //    oldx = angles.X;
    //    //    oldz = angles.Z;

    //    //    angles.X = angles.X + (float)0.1;
    //    //    angles.Y = angles.Y + (float)0.1;
    //    //    angles.Z = angles.Z + (float)0.1;


    //    //    if (angles.X < 0)
    //    //    {
    //    //        retAngles.X = angles.X + 360;
    //    //    }
    //    //    else
    //    //    {
    //    //        retAngles.X = angles.X;
    //    //    }
    //    //    if (angles.Z < 0)
    //    //    {
    //    //        retAngles.Z = angles.Z + 360;
    //    //    }
    //    //    else
    //    //    {
    //    //        retAngles.Z = angles.Z;
    //    //    }

    //    //    if (retAngles.Y < 0)
    //    //    {
    //    //        retAngles.Y = retAngles.Y + 360;
    //    //    }
    //    //OrientationEulerDebug = $"Dir\n\r{nameof(pitch)} {(int)(pitch)} \n\r{nameof(roll)} {(int)(roll)} \n\r{nameof(yaw)} {(int)(yaw)}\n\r";

    //    x += angles.X;
    //    y += angles.Y;
    //    z += angles.Z;
    //    OrientationEulerDebug = $"Dir\n\rPitch: {(double)(y)} \n\rRoll {(double)(x)} \n\rYaw {(double)z}\n\r";
    //}

    // Richtung (Vertikal, Horizontal Links, Horizontal Rechts)
    // Y ... -90 Links (Handy aufkippen)
    // Y ... +90 Rechts (Handy aufkippen)
    // Z ... Kopf links rechts drehen
}

