namespace WebSamDeviceApis.Actions.Source
{
    /// <summary>
    /// Possible results of source operations.
    /// /// </summary>
    public enum SourceResult
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        SUCCESS,
        /// <summary>
        /// The loadpoint that was tried to be set has a different number of phases than what this source can provide.
        /// </summary>
        LOADPOINT_NOT_SUITABLE_DIFFERENT_NUMBER_OF_PHASES,
        /// <summary>
        /// The voltage which was tried to be set was higher that what this source can provide.
        /// </summary>
        LOADPOINT_NOT_SUITABLE_VOLTAGE_INVALID,
        /// <summary>
        /// The current that was tried to be set was higher than what this source can provide.
        /// </summary>
        LOADPOINT_NOT_SUITABLE_CURRENT_INVALID,
        /// <summary>
        /// The number of harmonics given in the loadpoint is higher than what the source can provide.
        /// </summary>
        LOADPOINT_NOT_SUITABLE_TOO_MANY_HARMONICS,
        /// <summary>
        /// The angle must be between 0 (inclusive) and 360 (exclusive).
        /// </summary>
        LOADPOINT_ANGLE_INVALID,
    }
}