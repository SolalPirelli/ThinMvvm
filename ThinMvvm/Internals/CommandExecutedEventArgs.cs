// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Provides data for the CommandBase.Executed event.
    /// </summary>
    internal sealed class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the parameter with which the command was executed.
        /// </summary>
        public object Parameter { get; private set; }


        /// <summary>
        /// Creates a new CommandExecutedEventArgs.
        /// </summary>
        public CommandExecutedEventArgs( object parameter )
        {
            Parameter = parameter;
        }
    }
}