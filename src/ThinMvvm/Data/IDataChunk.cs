﻿using System;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Infrastructure.
    /// Represents a chunk of data retrieved by a source.
    /// </summary>
    /// <remarks>
    /// Implementations should only use IDataChunk members for equality purposes.
    /// </remarks>
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
        /// Gets the errors that occurred while creating this chunk.
        /// </summary>
        DataErrors Errors { get; }
    }
}