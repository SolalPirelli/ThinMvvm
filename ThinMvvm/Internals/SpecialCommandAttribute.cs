// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Internal attribute that marks Commands to indicate their special action, if any.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    internal sealed class SpecialCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the special action of the command this attribute is applied to.
        /// </summary>
        public SpecialAction Action { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialCommandAttribute" /> class with the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        public SpecialCommandAttribute( SpecialAction action )
        {
            Action = action;
        }
    }
}