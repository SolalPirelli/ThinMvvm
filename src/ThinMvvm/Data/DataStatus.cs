namespace ThinMvvm.Data
{
    /// <summary>
    /// Represents the possible states a data source can be in.
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// No data has been requested.
        /// </summary>
        None,

        /// <summary>
        /// Data is being loaded.
        /// </summary>
        Loading,

        /// <summary>
        /// Data was successfully loaded.
        /// </summary>
        Loaded,

        /// <summary>
        /// The loading operation yielded no data.
        /// </summary>
        NoData
    }
}