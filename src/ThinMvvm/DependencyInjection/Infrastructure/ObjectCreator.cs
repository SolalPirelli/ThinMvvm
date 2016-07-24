using System;
using System.Collections.Generic;
using System.Reflection;

namespace ThinMvvm.DependencyInjection.Infrastructure
{
    /// <summary>
    /// Creates objects by injecting known services in their constructors.
    /// </summary>
    public sealed class ObjectCreator
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services;
        private readonly Dictionary<Type, object> _singletons;


        public ObjectCreator( Dictionary<Type, ServiceDescriptor> services )
        {
            _services = new Dictionary<Type, ServiceDescriptor>( services );
            _singletons = new Dictionary<Type, object>();
        }


        /// <summary>
        /// Creates an instance of the specified type, optionally using the specified argument.
        /// </summary>
        public object Create( Type type, object arg )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

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
                    instance = CreateCore( service.ImplementationType, null );
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

            return CreateCore( type, arg );
        }


        /// <summary>
        /// Creates an instance of the specified type, optionally with the specified argument.
        /// Does not look at the services; always creates an instance.
        /// </summary>
        private object CreateCore( Type type, object arg )
        {
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
                if( arg != null && parameters[n].ParameterType == arg.GetType() )
                {
                    arguments[n] = arg;
                    arg = null;
                }
                else
                {
                    arguments[n] = Create( parameters[n].ParameterType, null );
                }
            }

            return constructor.Invoke( arguments );
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