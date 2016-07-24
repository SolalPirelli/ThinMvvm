using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ThinMvvm.DependencyInjection.Infrastructure;

namespace ThinMvvm.DependencyInjection
{
    /// <summary>
    /// Contains mappings from service types to implementations.
    /// </summary>
    public sealed class ServiceCollection
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services;


        /// <summary>
        /// Infrastructure.
        /// Initializes a new instance of the <see cref="ServiceCollection" /> class.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public ServiceCollection()
        {
            _services = new Dictionary<Type, ServiceDescriptor>();
        }


        /// <summary>
        /// Adds the specified service instance.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The instance.</param>
        public void AddInstance<TService>( TService instance )
        {
            if( instance == null )
            {
                throw new ArgumentNullException( nameof( instance ) );
            }

            EnsureNotBound( typeof( TService ) );

            _services.Add( typeof( TService ), new ServiceDescriptor( instance ) );
        }


        /// <summary>
        /// Adds the specified service implementation as a singleton.
        /// The implementation will be created the first time the service is needed, and reused afterwards.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        public void AddSingleton<TService, TImplementation>()
            where TImplementation : TService
        {
            EnsureNotBound( typeof( TService ) );
            EnsureNotAbstract( typeof( TImplementation ) );

            _services.Add( typeof( TService ), new ServiceDescriptor( typeof( TImplementation ), true ) );
        }

        /// <summary>
        /// Adds a service as a singleton using the specified factory.
        /// The factory will be used the first time the service is needed, and reused afterwards.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory.</param>
        public void AddSingleton<TService>( Func<TService> factory )
            where TService : class
        {
            if( factory == null )
            {
                throw new ArgumentNullException( nameof( factory ) );
            }

            EnsureNotBound( typeof( TService ) );

            _services.Add( typeof( TService ), new ServiceDescriptor( factory, true ) );
        }

        /// <summary>
        /// Adds the specified service as a singleton.
        /// The service will be created the first time it is needed, and reused afterwards.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        public void AddSingleton<TService>()
        {
            AddSingleton<TService, TService>();
        }


        /// <summary>
        /// Adds the specified service implementation.
        /// The implementation will be created each time the service is needed.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        public void AddTransient<TService, TImplementation>()
            where TImplementation : TService
        {
            EnsureNotBound( typeof( TService ) );
            EnsureNotAbstract( typeof( TImplementation ) );

            _services.Add( typeof( TService ), new ServiceDescriptor( typeof( TImplementation ), false ) );
        }

        /// <summary>
        /// Adds a service using the specified factory.
        /// The factory will be used each time the service is needed.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory.</param>
        public void AddTransient<TService>( Func<TService> factory )
            where TService : class
        {
            if( factory == null )
            {
                throw new ArgumentNullException( nameof( factory ) );
            }

            EnsureNotBound( typeof( TService ) );

            _services.Add( typeof( TService ), new ServiceDescriptor( factory, false ) );
        }


        /// <summary>
        /// Infrastructure.
        /// Builds a <see cref="ObjectCreator" /> using the service implementations defined with this collection.
        /// </summary>
        /// <returns>A <see cref="ObjectCreator" /> using the bound services.</returns>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public ObjectCreator BuildCreator()
        {
            return new ObjectCreator( _services );
        }


        /// <summary>
        /// Ensures that the specified service type is not already bound.
        /// </summary>
        private void EnsureNotBound( Type serviceType )
        {
            if( _services.ContainsKey( serviceType ) )
            {
                throw new InvalidOperationException( $"Service type '{serviceType}' was already bound." );
            }
        }

        /// <summary>
        /// Ensures that the specified implementation type is not abstract.
        /// </summary>
        private void EnsureNotAbstract( Type implementationType )
        {
            if( implementationType.GetTypeInfo().IsAbstract )
            {
                throw new ArgumentException( $"Implementation type {implementationType} is abstract, which is not allowed." );
            }
        }
    }
}