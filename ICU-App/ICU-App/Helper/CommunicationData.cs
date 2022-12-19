using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Helper;

public class CommunicationData
{
    // Handschuhdaten
    public double? Pitch { get; set; }
    public double? Roll { get; set; }
    public double? Yaw { get; set; }
    public double? Power { get; set; }

    // Kameradaten
    public double? PitchG { get; set; }
    public double? RollG { get; set; }
    public double? YawG { get;set; }
}
