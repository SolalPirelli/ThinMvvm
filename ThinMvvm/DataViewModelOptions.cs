// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThinMvvm
{
    /// <summary>
    /// Options for <see cref="DataViewModel{TParameter}" />.
    /// </summary>
    public static class DataViewModelOptions
    {
        private static readonly HashSet<Type> _networkExceptionTypes = new HashSet<Type>();


        /// <summary>
        /// Adds the specified type as a network exception type.
        /// </summary>
        /// <param name="type">The type, which must inherit from <see cref="Exception" />.</param>
        public static void AddNetworkExceptionType( Type type )
        {
            if ( type == null )
            {
                throw new ArgumentNullException( "type" );
            }
            if ( type != typeof( Exception ) && !type.GetTypeInfo().IsSubclassOf( typeof( Exception ) ) )
            {
                throw new ArgumentException( "The type must inherit from System.Exception." );
            }

            _networkExceptionTypes.Add( type );
        }


        /// <summary>
        /// Clears the network exception types.
        /// </summary>
        [Obsolete( "For unit tests only." )]
        internal static void ClearNetworkExceptionTypes()
        {
            _networkExceptionTypes.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the specified exception is a network exception.
        /// </summary>
        internal static bool IsNetworkException( Exception e )
        {
            return _networkExceptionTypes.Any( ne => e.GetType() == ne || e.GetType().GetTypeInfo().IsSubclassOf( ne ) );
        }
    }
}