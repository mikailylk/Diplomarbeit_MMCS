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
using ICU_App.Helper;

namespace ICU_App.ViewModel;

public partial class SettingsViewModel : ObservableRecipient
{

    [ObservableProperty]
    private ObservableCollection<string> hostnames;

    [ObservableProperty]
    private int selected_hostname_index = -1;

    private SettingsModel settingsmodel;

    private List<NetworkInterface> _networkInterfaces;

    private NetworkInterface _selectedInterface;

    private IPAddress _ipAddress;

    private IPAddress _subnetmask;


    public SettingsViewModel()
    {
        settingsmodel = new SettingsModel("192.168.177.10");
        // find hostnames in network -> set raspberry pi as server https://stackoverflow.com/questions/4042789/how-to-get-ip-of-all-hosts-in-lan
        hostnames = new ObservableCollection<string>();
    }
    protected override void OnActivated()
    {
        base.OnActivated();

        if (_ipAddress != null)
        {
            PingAllUsers();
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        hostnames.Clear();
    }

    public async void SelectNetworkInterface()
    {
        // Gets available networkinterfaces (exclude Loopback & inactive interfaces)
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        _networkInterfaces = new List<NetworkInterface>();
        foreach (NetworkInterface ni in interfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                _networkInterfaces.Add(ni);
            }
        }

        // Let user to select the interface
        string[] buttons = new string[_networkInterfaces.Count];
        for (int i = 0; i < _networkInterfaces.Count; i++)
        {
            buttons[i] = _networkInterfaces[i].Name;
        }

        string interface_name = await Application.Current.MainPage.DisplayActionSheet("Networkinterface", "Close Application", null, buttons);

        // if no interface available or user doesn't select any interface 
        if (String.IsNullOrEmpty(interface_name) || interface_name == "Close Application")
        {
            Application.Current.Quit();
            return;
        }

        foreach (NetworkInterface ni in _networkInterfaces)
        {
            if (ni.Name == interface_name)
            {
                _selectedInterface = ni;
                break;
            }
        }

        // Gets Ip Address and Subnetmask
        foreach (UnicastIPAddressInformation ip in _selectedInterface.GetIPProperties().UnicastAddresses)
        {
            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                _ipAddress = IPAddress.Parse(ip.Address.ToString());
                _subnetmask = IPAddress.Parse(ip.IPv4Mask.ToString());
                break;
            }
        }

        PingAllUsers();

        //Task.Run(() =>
        //{
        //    //string ipBase = "192.168.188.";   // zuhause



        //    string ipBase = "192.168.137.";     // Handy Hotspot
        //    for (int i = 1; i < 255; i++)
        //    {
        //        string ip = ipBase + i.ToString();

        //        Ping p = new Ping();
        //        p.SendAsync(ip, 1000, ip);   // 1000 in ms --> Raspberry Pi ist träge und antwortet nicht sehr schnell
        //        p.PingCompleted += P_PingCompleted;
        //    }
        //    return;
        //});
    }

    private void PingAllUsers()
    {
        // Get network address and the last ip address (broadcast address) in network
        IPAddress network_address = IPv4Helper.GetNetworkAddress(_ipAddress, _subnetmask);
        IPAddress broadcast_address = IPv4Helper.GetBroadcastAddress(_ipAddress, _subnetmask);
        string s = broadcast_address.ToString();

        // Ping to all users in network
        Task.Run(() =>
        {
            for (uint i = 1; i < UInt32.MaxValue; i++)
            {
                IPAddress curr_ip = IPv4Helper.GetNextIPAddress(network_address, i);
                if (!curr_ip.Equals(_ipAddress))
                {
                    Ping p = new Ping();
                    p.SendAsync(curr_ip, 1000, curr_ip);   // 2000 in ms --> Raspberry Pi ist träge und antwortet nicht sehr schnell
                    p.PingCompleted += P_PingCompleted;
                }

                if (curr_ip.Equals(broadcast_address))
                {
                    break;
                }
            }
        });
    }

    private void P_PingCompleted(object sender, PingCompletedEventArgs e)
    {

        string ip = String.Join('.', e.UserState);
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
