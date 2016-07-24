using System;
using System.ComponentModel;

namespace ThinMvvm.DependencyInjection.Infrastructure
{
    /// <summary>
    /// Describes a service used in dependency injection.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class ServiceDescriptor
    {
        /// <summary>
        /// Gets the instance, if the service was added as an instance.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets the factory to create an instance, if any.
        /// </summary>
        public Func<object> Factory { get; }

        /// <summary>
        /// Gets the implementation type of the service, if it needs to be created.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets a value indicating whether the service was added as a singleton.
        /// </summary>
        public bool IsSingleton { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor" /> class, 
        /// describing an instance service.
        /// </summary>
        /// <param name="instance">The service instance.</param>
        public ServiceDescriptor( object instance )
        {
            Instance = instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor" /> class, 
        /// describing a scoped or singleton service that must be created from a factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="isSingleton">Whether the service is a singleton.</param>
        public ServiceDescriptor( Func<object> factory, bool isSingleton )
        {
            Factory = factory;
            IsSingleton = isSingleton;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor" /> class, 
        /// describing a scoped or singleton service that must be instantiated.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="isSingleton">Whether the service is a singleton.</param>
        public ServiceDescriptor( Type implementationType, bool isSingleton )
        {
            ImplementationType = implementationType;
            IsSingleton = isSingleton;
        }
    }
}