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
        /// Initializes a new instance of the <see cref="LogIdAttribute" /> class with the specified ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        public LogIdAttribute( string id )
        {
            Id = id;
        }
    }
}