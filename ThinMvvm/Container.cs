// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThinMvvm
{
    /// <summary>
    /// Simple container for dependency injection (DI).
    /// </summary>
    public static class Container
    {
        private static readonly Dictionary<Type, object> _impls = new Dictionary<Type, object>();

        /// <summary>
        /// Binds an abstract type to a concrete type.
        /// </summary>
        /// <returns>The instance of the implementation.</returns>
        /// <typeparam name="TAbstract">The abstract type (or interface).</typeparam>
        /// <typeparam name="TImpl">The concrete type.</typeparam>
        public static TImpl Bind<TAbstract, TImpl>()
            where TImpl : TAbstract
        {
            var key = typeof( TAbstract );
            var implInfo = typeof( TImpl ).GetTypeInfo();

            if ( typeof( TAbstract ) == typeof( TImpl ) )
            {
                throw new ArgumentException( "Cannot bind a type to itself." );
            }

            if ( implInfo.IsInterface || implInfo.IsAbstract )
            {
                throw new ArgumentException( "The implementation type must be concrete." );
            }

            if ( _impls.ContainsKey( key ) )
            {
                throw new InvalidOperationException( "Cannot override an implementation." );
            }

            var implementation = Get( typeof( TImpl ), null );
            _impls.Add( key, implementation );
            return (TImpl) implementation;
        }

        /// <summary>
        /// Gets a concrete instance of the specified type, resolving constructor parameters as needed,
        /// with an optional additional constructor parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameter">Optional. The additional parameter, if any.</param>
        /// <returns>A concrete instance of the specified type.</returns>
        public static object Get( Type type, object parameter = null )
        {
            var typeInfo = type.GetTypeInfo();

            var existingImpl = _impls.FirstOrDefault( pair => typeInfo.IsAssignableFrom( pair.Key.GetTypeInfo() ) ).Value;
            if ( existingImpl != null )
            {
                return existingImpl;
            }

            if ( typeInfo.IsInterface || typeInfo.IsAbstract )
            {
                throw new ArgumentException( "Missing implementation: " + typeInfo.Name );
            }

            var ctor = typeInfo.DeclaredConstructors.SingleOrDefault( ci => !ci.IsStatic );
            if ( ctor == null )
            {
                throw new ArgumentException( "Could not find an unique constructor for type {0}", typeInfo.Name );
            }

            var argTypeInfo = parameter == null ? null : parameter.GetType().GetTypeInfo();
            bool argUsed = false;

            var ctorArgs =
                ctor.GetParameters()
                    .Select( param =>
                    {
                        if ( parameter != null && param.ParameterType.GetTypeInfo().IsAssignableFrom( argTypeInfo ) )
                        {
                            if ( _impls.ContainsKey( param.ParameterType ) )
                            {
                                throw new ArgumentException( "Ambiguous match for constructor parameter of type {0} between a dependency and the additional parameter.",
                                                             param.ParameterType.FullName );
                            }

                            if ( argUsed )
                            {
                                throw new InvalidOperationException( "Cannot use the argument twice in a constructor." );
                            }

                            argUsed = true;
                            return parameter;
                        }
                        return Get( param.ParameterType, null );
                    } )
                    .ToArray();
            return ctor.Invoke( ctorArgs );
        }

        /// <summary>
        /// Clears the container.
        /// </summary>
        /// <remarks>
        /// For use in unit tests.
        /// </remarks>
        internal static void Clear()
        {
            _impls.Clear();
        }
    }
}