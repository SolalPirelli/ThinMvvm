using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ThinMvvm.DependencyInjection.Infrastructure
{
    /// <summary>
    /// Creates objects by injecting known services in their constructors.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class ObjectCreator
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services;
        private readonly Dictionary<Type, object> _singletons;


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectCreator" /> class with the specified services.
        /// </summary>
        /// <param name="services">The services to use when resolving constructor parameters.</param>
        public ObjectCreator( Dictionary<Type, ServiceDescriptor> services )
        {
            _services = new Dictionary<Type, ServiceDescriptor>( services );
            _singletons = new Dictionary<Type, object>();
        }


        /// <summary>
        /// Creates an instance of the specified type, optionally with the specified argument.
        /// </summary>
        public object Create( Type type )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            var typeInfo = type.GetTypeInfo();
            if( typeInfo.IsAbstract )
            {
                throw new ArgumentException( $"Cannot instantiate unknown abstract type '{type.FullName}'." );
            }

            var constructor = GetSinglePublicConstructor( typeInfo );
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            for( int n = 0; n < parameters.Length; n++ )
            {
                arguments[n] = GetService( parameters[n].ParameterType );
            }

            return constructor.Invoke( arguments );
        }


        /// <summary>
        /// Gets an instance of the specified service type, creating it if needed.
        /// </summary>
        private object GetService( Type type )
        {
            ServiceDescriptor service;
            if( _services.TryGetValue( type, out service ) )
            {
                if( service.Instance != null )
                {
                    return service.Instance;
                }

                object instance;
                if( service.IsSingleton && _singletons.TryGetValue( type, out instance ) )
                {
                    return instance;
                }

                if( service.Factory == null )
                {
                    instance = Create( service.ImplementationType );
                }
                else
                {
                    instance = service.Factory();

                    if( instance == null )
                    {
                        throw new InvalidOperationException( $"Factory for service {type} returned null." );
                    }
                }

                if( service.IsSingleton )
                {
                    _singletons.Add( type, instance );
                }

                return instance;
            }

            return Create( type );
        }


        /// <summary>
        /// Gets the single public constructor of the specified type.
        /// Throws if there are zero or more than one public constructors.
        /// </summary>
        private static ConstructorInfo GetSinglePublicConstructor( TypeInfo typeInfo )
        {
            ConstructorInfo constructor = null;
            foreach( var candidate in typeInfo.DeclaredConstructors )
            {
                if( candidate.IsPublic )
                {
                    if( constructor != null )
                    {
                        throw new ArgumentException( $"Cannot instantiate type '{typeInfo.FullName}' because it has multiple public constructors." );
                    }

                    constructor = candidate;
                }
            }

            if( constructor == null )
            {
                throw new ArgumentException( $"Cannot instantiate type '{typeInfo.FullName}' because it has no public constructor." );
            }

            return constructor;
        }
    }
}