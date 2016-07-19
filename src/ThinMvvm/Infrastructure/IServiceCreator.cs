using System;
using System.ComponentModel;

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Creates objects, recursively creating their dependencies if needed.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public interface IServiceCreator
    {
        /// <summary>
        /// Creates an instance of the specified type, optionally using the specified argument.
        /// </summary>
        /// <param name="type">The object's type.</param>
        /// <param name="arg">The constructor argument, if any.</param>
        /// <returns>An instance of the type.</returns>
        object Create( Type type, object arg );
    }
}