using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ICU_App.Helper;

class TelemetryData
{
    // Telemetriedaten
    public double? TIMESTAMP { get; set; }
    public double? BATT_AMP { get; set; }
    public double? BATT_VOLT { get; set; }
    public double? BOARD_AMP { get; set; }
    public double? HYDRO { get; set; }
    public double? TEMP { get; set; }
    public double? PRESSURE { get; set; }
    public double? LONGTITUDE { get; set; }
    public double? LATITUDE { get; set; }

    public override string ToString()
    {
        return $"Timestamp: {new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)TIMESTAMP).ToLocalTime()}" +
            $"\nBatterystatus: {BATT_AMP}A, {BATT_VOLT}V\n" +
            $"Board current: {BOARD_AMP}A\n" +
            $"Hydro: {HYDRO}, Temperature: {TEMP}°C, Pressure: {PRESSURE}\n" +
            $"Location: Longtitude: {LONGTITUDE}; Latitude: {LATITUDE}";
    }
}
