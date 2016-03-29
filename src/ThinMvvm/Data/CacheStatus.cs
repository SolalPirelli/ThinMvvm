namespace ThinMvvm.Data
{
    /// <summary>
    /// Represents the possible states for cached data.
    /// </summary>
    public enum CacheStatus
    {
        /// <summary>
        /// Live data is being used.
        /// </summary>
        Unused,

        /// <summary>
        /// Cached data is being used.
        /// </summary>
        Used
    }
}