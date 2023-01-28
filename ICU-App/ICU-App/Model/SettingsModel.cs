using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Model;

public class SettingsModel
{
    public SettingsModel(string raspi_ip)
    {
        this.raspi_ip = raspi_ip;
    }

    // TODO: Settings 
    public string raspi_ip { get; set; }
}