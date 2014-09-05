// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Possible statuses of cache usage.
    /// </summary>
    public enum CacheStatus
    {
        /// <summary>
        /// No cached data is available.
        /// </summary>
        NoData,

        /// <summary>
        /// Cached data is used temporarily while loading data.
        /// </summary>
        UsedTemporarily,

        /// <summary>
        /// Cached data is used because live data is not available.
        /// </summary>
        Used,

        /// <summary>
        /// The code opted out of using caching, and live data is used.
        /// </summary>
        OptedOut,

        /// <summary>
        /// Live data is used.
        /// </summary>
        Unused
    }
}