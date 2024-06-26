using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SharedLibrary.Models.Logging;

/// <summary>
/// The logical context of a communication event.s
/// </summary>
public class InterfaceLogEntryScope
{
    /// <summary>
    /// Allows to identify all response for a single request.
    /// </summary>
    public string RequestId { get; set; } = null!;

    /// <summary>
    /// Set for sent data, unset for incoming.
    /// </summary>
    [NotNull, Required]
    public required bool Outgoing { get; set; }
}
