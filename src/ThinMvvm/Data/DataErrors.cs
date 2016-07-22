using System;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Holds errors that occur when creating a chunk of data.
    /// </summary>
    public struct DataErrors : IEquatable<DataErrors>
    {
        /// <summary>
        /// Gets the exception thrown by a fetching operation, if any.
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


        public static bool operator ==(DataErrors left, DataErrors right)
        {
            return left.Equals( right );
        }

        public static bool operator !=( DataErrors left, DataErrors right )
        {
            return !( left == right );
        }

        public bool Equals( DataErrors other )
        {
            return Fetch == other.Fetch
                && Cache == other.Cache
                && Process == other.Process;
        }

        public override bool Equals( object obj )
        {
            var other = obj as DataErrors?;
            return other.HasValue && Equals( other.Value );
        }

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