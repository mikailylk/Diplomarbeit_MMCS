using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Model;

/// <summary>
/// This class helps to deserializes/serializes the communication data as JSON
/// [glovedata: pitch, roll, yaw, power],
/// [gimbaldata: pitch, roll, yaw].
/// </summary>
public class CommunicationData
{
    // Glovedata: data from glove sensor

    /// <summary>
    /// Gets or sets pitch angle from glove sensor.
    /// </summary>
    public double? Pitch { get; set; }
    /// <summary>
    /// Gets or sets roll angle from glove sensor.
    /// </summary>
    public double? Roll { get; set; }
    /// <summary>
    /// Gets or sets yaw angle from glove sensor.
    /// </summary>
    public double? Yaw { get; set; }
    /// <summary>
    /// Gets or sets power angle from glove sensor.
    /// </summary>
    public double? Power { get; set; }

    // Gimbaldata: data from orientation sensor of smartphone
    /// <summary>
    /// Gets or sets pitch angle of gimbal.
    /// </summary>
    public double? PitchG { get; set; }
    /// <summary>
    /// Gets or sets roll angle of gimbal.
    /// </summary>
    public double? RollG { get; set; }
    /// <summary>
    /// Gets or sets yaw angle of gimbal.
    /// </summary>
    public double? YawG { get; set; }
}
