using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RefMeterApi.Models;
using WebSamDeviceApis.Model;

namespace ScriptApi.Models;

/// <summary>
/// 
/// </summary>
public class StartDosageScript : StartScriptRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Required, NotNull]
    public Loadpoint Loadpoint { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Required, NotNull]
    public MeasurementModes MeasurementMode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required, NotNull]
    public double Energy { get; set; }
}
