using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using BurdenApi.Actions.Algorithms;

namespace BurdenApi.Models;

/// <summary>
/// Parameters to run the calibration for a single step.
/// </summary>
public class CalibrationRequest
{
    /// <summary>
    /// Burden to use.
    /// </summary>
    [NotNull, Required]
    public string Burden { get; set; } = null!;

    /// <summary>
    /// Range to use.
    /// </summary>
    [NotNull, Required]
    public string Range { get; set; } = null!;

    /// <summary>
    /// Step to use.
    /// </summary>
    [NotNull, Required]
    public string Step { get; set; } = null!;

    /// <summary>
    /// Set to choose the best voltage or current range.
    /// </summary>
    [NotNull, Required]
    public bool ChooseBestRange { get; set; }

    /// <summary>
    /// Algorithm to use to calibrate burden steps.
    /// </summary>
    [NotNull, Required]
    public CalibrationAlgorithms Algorithm { get; set; } = CalibrationAlgorithms.Default;
}