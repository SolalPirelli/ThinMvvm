﻿// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Marks Commands to translate a parameter value to a logged value for an event.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
    public sealed class LogValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the actual value that should be logged.
        /// </summary>
        public string LoggedValue { get; private set; }


        /// <summary>
        /// Creates a new LogValueConverterAttribute.
        /// </summary>
        public LogValueConverterAttribute( object value, string loggedValue )
        {
            Value = value;
            LoggedValue = loggedValue;
        }
    }
}