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
using Microsoft.Maui.Controls;

namespace ICU_App.ViewModel;

public partial class SettingsViewModel : ObservableRecipient
{

    [ObservableProperty]
    private ObservableCollection<string> hostnames;

    [ObservableProperty]
    private int selected_hostname_index = -1;

    private SettingsModel settingsmodel;

    public SettingsViewModel()
    {
        settingsmodel = new SettingsModel("192.168.177.10");
        // find hostnames in network -> set raspberry pi as server https://stackoverflow.com/questions/4042789/how-to-get-ip-of-all-hosts-in-lan
        hostnames = new ObservableCollection<string>();
        
    }
    protected override void OnActivated()
    {
        base.OnActivated();

        Task.Run(() =>
        {
            // string ipBase = "192.168.188.";
            string ipBase = "192.168.137.";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.SendAsync(ip, 1000, ip);   // 1000 in ms --> Raspberry Pi ist träge und antwortet nicht sehr schnell
                p.PingCompleted += P_PingCompleted;
            }
            return;
        });
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        hostnames.Clear();
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
                MainThread.BeginInvokeOnMainThread(() => Hostnames.Add(name + " ; " + ip));
            }
            catch (SocketException ex)
            {
                name = "?";
                MainThread.BeginInvokeOnMainThread(() => Hostnames.Add(name + " ; " + ip));
            }
        }
        else if (e.Reply == null)
        {
            // pingen ist fehlgeschlagen
        }
    }

    [RelayCommand]
    private async Task NavigateDebugPage()
    {
        await Shell.Current.GoToAsync($"{nameof(DebugPage)}");
    }


    [RelayCommand]
    private async Task NavigateMainPage()
    {
        if (selected_hostname_index == -1) { return; }
        
        // die IP-Adresse des Raspberry Pi auf die nächste Seite geben --> Webview und Kommunikation
        settingsmodel.raspi_ip = hostnames[selected_hostname_index].Split(';')[1].Trim();

        await Shell.Current.GoToAsync($"{nameof(MainPage)}",
            new Dictionary<string, object>
            {
                ["Settingsmodel"] = settingsmodel
            }
            );
    }

    [RelayCommand]
    private async Task NavigateLogPage()
    {
        // TODO: alte Flugdaten (Batteriespannung, Koordinaten, ...) in einem Chart bzw. in einer Tabelle darstellen
        // await Shell.Current.GoToAsync($"{nameof(LogPage)}");
        await Shell.Current.DisplayAlert("Attention", "This feature is currently not awailable!", "OK");
    }
}
