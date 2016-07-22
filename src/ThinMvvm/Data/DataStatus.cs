namespace ThinMvvm.Data
{
    /// <summary>
    /// The possible states a data chunk can be in.
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// Data was successfully loaded.
        /// </summary>
        Normal,

        /// <summary>
        /// Data failed to load, but cached data is used instead.
        /// </summary>
        Cached,

        /// <summary>
        /// Data failed to load, and no cached data was available.
        /// </summary>
        Error
    }
}