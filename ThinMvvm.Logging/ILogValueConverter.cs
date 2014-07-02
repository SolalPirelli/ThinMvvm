// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Converts command logging parameters to strings reported to the logging mechanism.
    /// </summary>
    public interface ILogValueConverter
    {
        /// <summary>
        /// Converts the specified value to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string representing the value.</returns>
        string Convert( object value );
    }
}