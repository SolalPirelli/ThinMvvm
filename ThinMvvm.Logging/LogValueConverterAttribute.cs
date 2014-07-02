// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Reflection;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Marks Commands with a converter that will translate a parameter value to a logged value for an event.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
    public sealed class LogValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets the converter type, which must implement <see cref="ILogValueConverter" />.
        /// </summary>
        public Type ConverterType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LogValueConverterAttribute" /> class with the specified converter type.
        /// </summary>
        /// <param name="converterType">The converter type.</param>
        public LogValueConverterAttribute( Type converterType )
        {
            var converterTypeInfo = converterType.GetTypeInfo();
            if ( converterTypeInfo.IsAbstract || converterTypeInfo.IsInterface )
            {
                throw new ArgumentException( "converterType must be concrete." );
            }
            if ( !typeof( ILogValueConverter ).GetTypeInfo().IsAssignableFrom( converterTypeInfo ) )
            {
                throw new ArgumentException( "converterType must implement ILogValueConverter." );
            }

            ConverterType = converterType;
        }
    }
}