using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Model;

/// <summary>
/// This class represents a set of settings (expandable).
/// </summary>
public class SettingsModel
{
    public SettingsModel() { }

    /// <summary>
    /// Initializes a new instance of the SettingsModel class with a specified Raspberry Pi IP address.
    /// </summary>
    /// <param name="raspi_ip">The IP address of the Raspberry Pi.</param>
    public SettingsModel(string raspi_ip)
    {
        this.raspi_ip = raspi_ip;
    }

    /// <summary>
    /// Gets or sets the IP address of the Raspberry Pi.
    /// </summary>
    public string raspi_ip { get; set; }
}