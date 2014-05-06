// Copyright (c) Solal Pirelli 2014
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
        /// Gets the corresponding value to log.
        /// </summary>
        public string LoggedValue { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LogValueConverterAttribute" /> class with the specified parameters.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="loggedValue">The corresponding value to log.</param>
        public LogValueConverterAttribute( object value, string loggedValue )
        {
            Value = value;
            LoggedValue = loggedValue;
        }
    }
}