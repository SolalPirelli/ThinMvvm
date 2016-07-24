using System;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Chunk of data, which may be cached and have errors.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public sealed class DataChunk<T> : IDataChunk
    {
        /// <summary>
        /// Gets the chunk's value, if any.
        /// 
        /// This property will throw a <see cref="InvalidOperationException" /> if there is no value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the chunk's status.
        /// </summary>
        public DataStatus Status { get; }

        /// <summary>
        /// Gets the errors associated with this chunk, if any.
        /// </summary>
        public DataErrors Errors { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataChunk{T}" /> class with the specified value, status and errors.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="status">The status.</param>
        /// <param name="errors">The errors.</param>
        public DataChunk( T value, DataStatus status, DataErrors errors )
        {
            if( status != DataStatus.Normal && status != DataStatus.Cached && status != DataStatus.Error )
            {
                throw new ArgumentException( "Invalid status.", nameof( status ) );
            }

            Value = value;
            Status = status;
            Errors = errors;
        }

        // Explicitly implemented to provide a typed value instead.
        object IDataChunk.Value => Value;


        /// <summary>
        /// Indicates whether the <see cref="DataChunk{T}" /> is equal to the specified <see cref="IDataChunk" />.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals( IDataChunk other )
        {
            if( other == null )
            {
                return false;
            }

            return Status == other.Status
                && Errors == other.Errors
                && ( Value == null ? other.Value == null : Value.Equals( other.Value ) );
        }

        /// <summary>
        /// Indicates whether the <see cref="DataChunk{T}" /> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return Equals( obj as IDataChunk );
        }

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns>The object's hash code.</returns>
        public override int GetHashCode()
        {
            var hash = 7;

            if( Status != DataStatus.Error )
            {
                hash += 31 * ( Value == null ? 0 : Value.GetHashCode() );
            }

            hash += 31 * (int) Status;
            hash += 31 * Errors.GetHashCode();
            return hash;
        }
    }
}