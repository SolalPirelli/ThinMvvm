// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThinMvvm
{
    /// <summary>
    /// Simple container for dependency injection.
    /// </summary>
    public static class Container
    {
        private static readonly Dictionary<Type, object> _impls = new Dictionary<Type, object>();


        /// <summary>
        /// Binds an abstract type to a concrete type, and returns an instance of the concrete type.
        /// </summary>
        /// <returns>The instance of the implementation.</returns>
        /// <typeparam name="TAbstract">The abstract type (or interface).</typeparam>
        /// <typeparam name="TImpl">The concrete type.</typeparam>
        /// <returns>An instance of the concrete type.</returns>
        public static TImpl Bind<TAbstract, TImpl>()
            where TImpl : TAbstract
        {
            CheckBindArguments<TAbstract, TImpl>();

            var implementation = Get( typeof( TImpl ), null );
            _impls.Add( typeof( TAbstract ), implementation );
            return (TImpl) implementation;
        }


        /// <summary>
        /// Gets a concrete instance of the specified type, resolving constructor parameters as needed,
        /// with an optional additional constructor parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameter">The parameter, or null if there is none.</param>
        /// <returns>A concrete instance of the specified type.</returns>
        internal static object Get( Type type, object parameter )
        {
            var typeInfo = type.GetTypeInfo();

            var existingImpl = GetImplementation( typeInfo );
            if ( existingImpl != null )
            {
                return existingImpl;
            }

            if ( typeInfo.IsInterface || typeInfo.IsAbstract )
            {
                throw new ArgumentException( string.Format( "Missing implementation: {0}.", typeInfo.Name ) );
            }

            var ctors = typeInfo.DeclaredConstructors.Where( ci => !ci.IsStatic ).ToArray();
            if ( ctors.Length > 1 )
            {
                throw new ArgumentException( string.Format( "Could not find an unique constructor for type {0}.", typeInfo.Name ) );
            }

            var ctor = ctors[0];
            var argTypeInfo = parameter == null ? null : parameter.GetType().GetTypeInfo();
            bool argUsed = false;

            var ctorParams = ctor.GetParameters();
            var ctorArgs = new object[ctorParams.Length];

            for ( int n = 0; n < ctorArgs.Length; n++ )
            {
                var paramTypeInfo = ctorParams[n].ParameterType.GetTypeInfo();
                if ( parameter != null && paramTypeInfo.IsAssignableFrom( argTypeInfo ) )
                {
                    if ( GetImplementation( paramTypeInfo ) != null )
                    {
                        throw new ArgumentException(
                            string.Format( "Ambiguous match for constructor parameter of type {0} between a dependency and the additional parameter.",
                                           ctorParams[n].ParameterType.FullName ) );
                    }

                    if ( argUsed )
                    {
                        throw new InvalidOperationException( "Cannot use the argument twice in a constructor." );
                    }

                    argUsed = true;
                    ctorArgs[n] = parameter;
                }
                else
                {
                    ctorArgs[n] = Get( ctorParams[n].ParameterType, null );
                }
            }

            return ctor.Invoke( ctorArgs );
        }

        /// <summary>
        /// Clears the container.
        /// </summary>
        [Obsolete( "For use in unit tests only." )]
        internal static void Clear()
        {
            _impls.Clear();
        }


        /// <summary>
        /// Checks the arguments provided to the <see cref="Bind{TAbstract, TImpl}" /> method, ensuring they are valid.
        /// </summary>
        private static void CheckBindArguments<TAbstract, TImpl>()
        {
            if ( typeof( TAbstract ) == typeof( TImpl ) )
            {
                throw new ArgumentException(
                    string.Format( "Cannot bind the type {0} to itself.",
                                    typeof( TAbstract ).FullName ) );
            }

            var implInfo = typeof( TImpl ).GetTypeInfo();
            if ( implInfo.IsInterface )
            {
                throw new ArgumentException(
                    string.Format( "Implementation types must be concrete; the given implementation for {0}, {1}, is an interface.",
                                    typeof( TAbstract ).FullName, typeof( TImpl ).FullName ) );
            }
            if ( implInfo.IsAbstract )
            {
                throw new ArgumentException(
                    string.Format( "Implementation types must be concrete; the given implementation for {0}, {1}, is abstract.",
                                    typeof( TAbstract ).FullName, typeof( TImpl ).FullName ) );
            }

            if ( _impls.ContainsKey( typeof( TAbstract ) ) )
            {
                throw new InvalidOperationException(
                    string.Format( "Cannot override an implementation. Type name: {0}.",
                                   typeof( TAbstract ).FullName ) );
            }
        }

        /// <summary>
        /// Gets the implementation for the specified type, or null if none was registered. 
        /// </summary>
        private static object GetImplementation( TypeInfo typeInfo )
        {
            return _impls.FirstOrDefault( pair => typeInfo.IsAssignableFrom( pair.Key.GetTypeInfo() ) ).Value;
        }
    }
}