// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Marks Commands and ViewModels to indicate their log ID.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Class )]
    public sealed class LogIdAttribute : Attribute
    {
        /// <summary>
        /// Gets the ID of the object this attribute is applied to.
        /// </summary>
        public string Id { get; private set; }


        /// <summary>
        /// Creates a new LogIdAttribute.
        /// </summary>
        public LogIdAttribute( string id )
        {
            Id = id;
        }
    }
}