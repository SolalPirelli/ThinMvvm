// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Provides data for the <see cref="CommandBase.Executed" /> event.
    /// </summary>
    internal sealed class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the argument given to the command.
        /// </summary>
        public object Argument { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutedEventArgs" /> class with the specified command argument.
        /// </summary>
        public CommandExecutedEventArgs( object argument )
        {
            Argument = argument;
        }
    }
}