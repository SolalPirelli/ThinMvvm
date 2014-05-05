// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThinMvvm
{
    /// <summary>
    /// Options for DataViewModel.
    /// </summary>
    public static class DataViewModelOptions
    {
        private static readonly HashSet<TypeInfo> _networkExceptionTypes;


        /// <summary>
        /// Initializes static members of the DataViewModelOptions class.
        /// By default, network exception types are restricted to System.Net.WebException.
        /// </summary>
        static DataViewModelOptions()
        {
            _networkExceptionTypes = new HashSet<TypeInfo>();
        }


        /// <summary>
        /// Adds the specified type as a network exception type.
        /// </summary>
        /// <param name="type">The type, which must inherit from System.Exception.</param>
        public static void AddNetworkExceptionType( Type type )
        {
            var typeInfo = type.GetTypeInfo();

            if ( !typeof( Exception ).GetTypeInfo().IsAssignableFrom( typeInfo ) )
            {
                throw new ArgumentException( "NetworkExceptionTypes must inherit from System.Exception." );
            }

            _networkExceptionTypes.Add( typeInfo );
        }


        /// <summary>
        /// Clears the network exception types.
        /// </summary>
        /// <remarks>
        /// For use in unit tests.
        /// </remarks>
        internal static void ClearNetworkExceptionTypes()
        {
            _networkExceptionTypes.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the specified exception is a network exception.
        /// </summary>
        internal static bool IsNetworkException( Exception e )
        {
            return _networkExceptionTypes.Any( ne => ne.IsAssignableFrom( e.GetType().GetTypeInfo() ) );
        }
    }
}