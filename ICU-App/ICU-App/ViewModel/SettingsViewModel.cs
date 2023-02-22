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

/// <summary>
/// The ViewModel for the SettingsPage, which handles scanning the network, 
/// selecting the raspberry pi and navigating to other pages.
/// </summary>
public partial class SettingsViewModel : ObservableRecipient
{
    /// <summary>
    /// The collection of available hostnames on the network.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> hostnames;

    /// <summary>
    /// The index of the currently selected hostname in the collection.
    /// </summary>
    [ObservableProperty]
    private int selected_hostname_index = -1;

    private SettingsModel settingsmodel;

    private List<NetworkInterface> _networkInterfaces;

    private NetworkInterface _selectedInterface;

    private IPAddress _ipAddress;

    private IPAddress _subnetmask;

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel()
    {
        settingsmodel = new SettingsModel();

        // find hostnames in network and set raspberry pi as server https://stackoverflow.com/questions/4042789/how-to-get-ip-of-all-hosts-in-lan
        hostnames = new ObservableCollection<string>();
    }
    /// <summary>
    /// Overrides the method called when the ViewModel is activated, 
    /// and ping all users again (if new users joined the network).
    /// </summary>
    protected override void OnActivated()
    {
        base.OnActivated();

        if (_ipAddress != null)
        {
            PingAllUsers();
        }
    }

    /// <summary>
    /// Overrides the method called when the ViewModel is deactivated and 
    /// clearing the collections.
    /// </summary>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Hostnames.Clear();
    }

    /// <summary>
    /// Opens a dialog box to let the user select a network interface, 
    /// then retrieves the IP address and subnet mask and 
    /// sends a ping to all users on the network.
    /// </summary>
    public async void SelectNetworkInterface()
    {
        // Gets available networkinterfaces (exclude Loopback & inactive interfaces)
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        _networkInterfaces = new List<NetworkInterface>();
        foreach (NetworkInterface ni in interfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {

                foreach (UnicastIPAddressInformation IP in ni.GetIPProperties().UnicastAddresses)
                {
                    // check if interface is a IPv4 Interface
                    if (IP.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _networkInterfaces.Add(ni);
                    }
                }
            }
        }

        // Let user to select the interface
        string[] buttons = new string[_networkInterfaces.Count];
        for (int i = 0; i < _networkInterfaces.Count; i++)
        {
            buttons[i] = _networkInterfaces[i].Name;
        }

        string interface_name;

        do
        {
            interface_name = await Application.Current.MainPage.DisplayActionSheet("Networkinterface", "Close Application", null, buttons);

        } while (String.IsNullOrEmpty(interface_name));
        

        // if no interface selected 
        if (interface_name == "Close Application")
        {
            // Close application
            Application.Current.Quit();
            return;
        }

        // Gets selected interface
        _selectedInterface = _networkInterfaces.Where(ni => ni.Name == interface_name).First();

        // Gets Ip Address and Subnetmask
        foreach (UnicastIPAddressInformation ip in _selectedInterface.GetIPProperties().UnicastAddresses)
        {
            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                _ipAddress = IPAddress.Parse(ip.Address.ToString());
                _subnetmask = IPAddress.Parse(ip.IPv4Mask.ToString());
                break;
            }
        }
        // Send Ping to all users in network
        PingAllUsers();
    }

    /// <summary>
    /// Sends a ping to all users on the network and 
    /// adds the hostname and IP address of each user that responds.
    /// </summary>
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
                    p.SendAsync(curr_ip, 1000, curr_ip);
                    p.PingCompleted += P_PingCompleted;
                }

                if (curr_ip.Equals(broadcast_address))
                {
                    break;
                }
            }
        });
    }

    /// <summary>
    /// This method handles the PingCompleted event and performs the following actions:
    /// - Retrieves the IP address of the host that was pinged from the user state.
    /// - If the ping was successful, it attempts to retrieve the host name 
    /// using the IP address and adds the host name and IP address to a list.
    /// - If the ping failed, it does nothing.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The PingCompletedEventArgs that contains information about the ping operation.</param>
    private void P_PingCompleted(object sender, PingCompletedEventArgs e)
    {

        string ip = String.Join('.', e.UserState);

        // check if ping was successfull
        if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        {
            string name;
            try
            {
                // retrieve the hostname (ip --> dns --> hostname)
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
            // ping failed
        }
    }

    /// <summary>
    /// A command that navigates to DebugPage.
    /// </summary>
    [RelayCommand]
    private async Task NavigateDebugPage()
    {
        await Shell.Current.GoToAsync($"{nameof(DebugPage)}");
    }

    /// <summary>
    /// A command that navigates to MainPage and queries the IP address
    /// of raspberry pi/board.
    /// </summary>
    [RelayCommand]
    private async Task NavigateMainPage()
    {
        if (selected_hostname_index == -1) { return; }
        
        // die IP-Adresse des Raspberry Pi auf die nächste Seite geben --> WebView und Kommunikation
        settingsmodel.raspi_ip = hostnames[selected_hostname_index].Split(';')[1].Trim();

        await Shell.Current.GoToAsync($"{nameof(MainPage)}",
            new Dictionary<string, object>
            {
                ["Settingsmodel"] = settingsmodel
            }
            );
    }

    /// <summary>
    /// A command that navigates to LogPage.
    /// </summary>
    [RelayCommand]
    private async Task NavigateLogPage()
    {
        //await Shell.Current.DisplayAlert("Attention",
        //"This feature is currently not available!", "OK"); (it's done)
        await Shell.Current.GoToAsync($"{nameof(LogPage)}");

    }
}
