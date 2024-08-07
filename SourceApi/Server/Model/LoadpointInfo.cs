namespace SourceApi.Model;

/// <summary>
/// Information on the last loadpoint set.
/// </summary>
[Serializable]
public class LoadpointInfo
{
    /// <summary>
    /// The time the loadpoint was last set.
    /// </summary>
    public DateTime? SavedAt { get; set; }

    /// <summary>
    /// The time the loadpoint has been successfully activated.
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// the source is turned on or off
    /// </summary>
    public bool? IsActive { get; set; }
}
