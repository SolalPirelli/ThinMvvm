using System;
using System.ComponentModel;

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Provides data for the <see cref="CommandBase.Executed" /> event.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// The argument passed to the <see cref="CommandBase.Execute" /> method, if any.
        /// </summary>
        public object Argument { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutedEventArgs" /> class with the specified argument.
        /// </summary>
        /// <param name="argument">The command argument.</param>
        public CommandExecutedEventArgs( object argument )
        {
            Argument = argument;
        }
    }
}