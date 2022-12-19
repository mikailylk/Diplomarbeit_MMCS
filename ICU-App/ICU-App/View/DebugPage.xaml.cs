using ICU_App.Calc;
using ICU_App.ViewModel;
using System.Numerics;
using ICU_App.Helper;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using System.Net.NetworkInformation;
// using Android.Media;
using System.Text.Json;
using System.Globalization;

namespace ICU_App.View;

public partial class DebugPage : ContentPage
{
    UDPClient client;
    UDPListener server;
    public DebugPage(DebugViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

        //IPEndPoint socket = new IPEndPoint(IPAddress.Any, 8086);    // Empfängersocket für PICO

        ////UDP-Server erstellen und starten
        //server = new UDPListener(socket);       // Listener für PICO

        //// Client starten, um Daten an Raspberry Pi Zero zu verschicken
        //client = UDPClient.ConnectTo("192.168.137.241", 8088);   // HOTSPOT LAPTOP
        //// client = UDPClient.ConnectTo("192.168.188.71", 8088);   // ZUHAUSE

        //Die Nachrichten lesen und auf Label ausgeben
        //Task.Run(async () =>
        //{
        //    try
        //    {
        //        int i = 0;
        //        while (!server.cancellationTokenSource.IsCancellationRequested)
        //        {
        //            // Daten von Pico abwarten
        //            var received = await server.Receive();

        //            // Daten an Raspberry Pi Zero verschicken
        //            CommunicationData communicationData = JsonSerializer.Deserialize<CommunicationData>(received.Message.ToString());
        //            //CommunicationData communicationData = new CommunicationData();
        //            //communicationData.Yaw = i;
        //            //communicationData.Pitch = 0;
        //            //communicationData.Roll = 0;
        //            //communicationData.Power = 0;

        //            communicationData.PitchG = 255;
        //            communicationData.YawG = 255;
        //            communicationData.RollG = 152;
        //            //if (i<255)
        //            //{
        //            //    i++;
        //            //}
        //            //else
        //            //{
        //            //    i = 0;
        //            //}
        //            string message = JsonSerializer.Serialize(communicationData);

        //            //string message = received.Message.ToString();

        //            client.Send(message);

        //            // string sender = received.Sender.ToString();
        //            // string anzahlbytes = System.Text.Encoding.ASCII.GetBytes(message).Length + "";
        //            // Device.BeginInvokeOnMainThread(() => lblreceived.Text = message + ";" + sender + "Bytes: "+ anzahlbytes);

        //            Device.BeginInvokeOnMainThread(() => lblreceived.Text = message);

        //            // Thread.Sleep(1);

        //            //if (received.Message == "quit")
        //            //    break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        server?.Close();
        //    }
        //}, server.cancellationTokenSource.Token);


        //Task.Run(async () =>
        //{
        //    while (!server.cancellationTokenSource.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            // auf Reply von Raspberry Pi Zero warten
        //            var received = await client.Receive();
        //            string message = received.Message;

        //            Device.BeginInvokeOnMainThread(() => lblreceived_from_zero.Text = "loopback: " + message);

        //            // Thread.Sleep(100);
        //            //if (received.Message.Contains("quit"))
        //            //    break;
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.Write(ex);
        //        }
        //    }
        //}, server.cancellationTokenSource.Token);
    }

    //private void Button_Clicked(object sender, EventArgs e)
    //{
    //    server.cancellationTokenSource.Cancel();
    //    server?.Close();
    //}

    //protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    //{
    //    base.OnNavigatedFrom(args);
    //    server.cancellationTokenSource.Cancel();
    //    server?.Close();

    //}
}