using System;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Contains errors that occur when creating a chunk of data.
    /// </summary>
    public struct DataErrors : IEquatable<DataErrors>
    {
        /// <summary>
        /// Gets the exception thrown when fetching data, if any.
        /// </summary>
        public Exception Fetch { get; }

        /// <summary>
        /// Gets the exception thrown when using a cache, if any.
        /// </summary>
        public Exception Cache { get; }

        /// <summary>
        /// Gets the exception thrown when processing data, if any.
        /// </summary>
        public Exception Process { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataErrors" /> class with the specified errors.
        /// </summary>
        /// <param name="fetch">The fetch exception, if any.</param>
        /// <param name="cache">The cache exception, if any.</param>
        /// <param name="process">The process exception, if any.</param>
        public DataErrors( Exception fetch, Exception cache, Exception process )
        {
            Fetch = fetch;
            Cache = cache;
            Process = process;
        }

        /// <summary>
        /// Indicates whether the two instances of <see cref="DataErrors" /> are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are equal.</returns>
        public static bool operator ==( DataErrors left, DataErrors right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Indicates whether the two instances of <see cref="DataErrors" /> are unequal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are unequal.</returns>
        public static bool operator !=( DataErrors left, DataErrors right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Indicates whether the <see cref="DataErrors" /> is equal to the specified <see cref="DataErrors" />.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals( DataErrors other )
        {
            return Fetch == other.Fetch
                && Cache == other.Cache
                && Process == other.Process;
        }

        /// <summary>
        /// Indicates whether the <see cref="DataErrors" /> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            var other = obj as DataErrors?;
            return other.HasValue && Equals( other.Value );
        }

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns>The object's hash code.</returns>
        public override int GetHashCode()
        {
            var hash = 7;
            hash += 31 * ( Fetch == null ? 0 : Fetch.GetHashCode() );
            hash += 31 * ( Cache == null ? 0 : Cache.GetHashCode() );
            hash += 31 * ( Process == null ? 0 : Process.GetHashCode() );
            return hash;
        }
    }
}