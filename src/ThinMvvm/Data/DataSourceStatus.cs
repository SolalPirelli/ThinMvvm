namespace ThinMvvm.Data
{
    /// <summary>
    /// The states a data source can be in.
    /// </summary>
    public enum DataSourceStatus
    {
        /// <summary>
        /// No loading operation was performed.
        /// </summary>
        None,

        /// <summary>
        /// Data is being loaded.
        /// </summary>
        Loading,

        /// <summary>
        /// Data was previously loaded, and more related data is being loaded.
        /// </summary>
        LoadingMore,

        /// <summary>
        /// Data has been loaded.
        /// </summary>
        Loaded
    }
}