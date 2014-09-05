// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Possible statuses of data load operations.
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// No load operation was requested.
        /// </summary>
        NoData,

        /// <summary>
        /// Data is being loaded.
        /// </summary>
        Loading,

        /// <summary>
        /// Data was loaded successfully.
        /// </summary>
        Loaded,

        /// <summary>
        /// An error occurred while loading data.
        /// </summary>
        Error,

        /// <summary>
        /// A network error occurred while loading data.
        /// </summary>
        NetworkError
    }
}