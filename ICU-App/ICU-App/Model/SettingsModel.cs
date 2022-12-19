using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Model;

public class SettingsModel
{
    public SettingsModel(int testid)
    {
        this.testid = testid;
    }

    // TODO: Settings 
    public int testid { get; set; }
}