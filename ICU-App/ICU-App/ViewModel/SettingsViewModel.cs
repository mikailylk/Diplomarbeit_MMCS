using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICU_App.Model;
using ICU_App.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace ICU_App.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private SettingsModel settingsmodel;
    public SettingsViewModel()
    {
        settingsmodel = new SettingsModel(testid: 2);
        this.testid2 = settingsmodel.testid;

        // find hostnames in network -> set raspberry pi as server https://stackoverflow.com/questions/4042789/how-to-get-ip-of-all-hosts-in-lan
        hostnames = new ObservableCollection<string>();
        Task.Run(() =>
        {
            string ipBase = "192.168.188.";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.SendAsync(ip, 100, ip);   // daurt max. etwa 25 Sekunden, bis alle im Netz gefunden sind
                p.PingCompleted += P_PingCompleted;
            }
            return;
        });
    }

    private void P_PingCompleted(object sender, PingCompletedEventArgs e)
    {
        string ip = (string)e.UserState;
        if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        {
            string name;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                name = hostEntry.HostName;
                Device.BeginInvokeOnMainThread(() => Hostnames.Add(name + " ; " + ip));
            }
            catch (SocketException ex)
            {
                name = "?";
            }
        }
        else if (e.Reply == null)
        {
            // pingen ist fehlgeschlagen
        }
    }

    private int testid2;
    public int Testid2
    {
        get { return settingsmodel.testid; }
        set 
        { 
            settingsmodel.testid = value;
            SetProperty(ref testid2, settingsmodel.testid);
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> hostnames;

    [RelayCommand]
    private async Task Navigate()
    {
        //await Shell.Current.GoToAsync($"{nameof(MainPage)}",
        //    new Dictionary<string, object>
        //    {
        //        ["Settingsmodel"] = settingsmodel
        //    }
        //    );
        await Shell.Current.GoToAsync($"{nameof(DebugPage)}");
    }

    [RelayCommand]
    private void Increment_Test_id()
    {
        Testid2++;
    }
}
