using System;
using System.ComponentModel;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Represents a chunk of data retrieved by a source.
    /// </summary>
    /// <remarks>
    /// Implementations should only use IDataChunk members for equality purposes.
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public interface IDataChunk : IEquatable<IDataChunk>
    {
        /// <summary>
        /// Gets the chunk's value, if any.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the chunk's status.
        /// </summary>
        DataStatus Status { get; }

        /// <summary>
        /// Gets the errors associated with this chunk, if any.
        /// </summary>
        DataErrors Errors { get; }
    }
}