using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ThinMvvm
{
    /// <summary>
    /// Binds services to concrete implementations.
    /// All implementations are instanciated only once.
    /// </summary>
    public sealed class ServiceBinder
    {
        private readonly Dictionary<Type, object> _instances;


        /// <summary>
        /// Infrastructure.
        /// Initializes a new instance of the <see cref="ServiceBinder" /> class.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public ServiceBinder()
        {
            _instances = new Dictionary<Type, object>();
        }


        /// <summary>
        /// Binds the specified interface type to the specified implementation type.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The implementation instance.</returns>
        public TImplementation Bind<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            var implementationTypeInfo = typeof( TImplementation ).GetTypeInfo();
            if( implementationTypeInfo.IsAbstract )
            {
                throw new ArgumentException( "The implementation type cannot be abstract." );
            }

            var instance = (TImplementation) Get( typeof( TImplementation ), null );
            Bind<TInterface>( instance );
            return instance;
        }

        /// <summary>
        /// Binds the specified interface type to the specified instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <param name="instance">The instance.</param>
        public void Bind<TInterface>( TInterface instance )
        {
            if( instance == null )
            {
                throw new ArgumentNullException( nameof( instance ) );
            }

            if( _instances.ContainsKey( typeof( TInterface ) ) )
            {
                throw new InvalidOperationException( "Cannot bind the same interface type twice." );
            }

            _instances.Add( typeof( TInterface ), instance );
        }

        /// <summary>
        /// Infrastructure.
        /// Gets an instance of the specified type, optionally using the specified argument.
        /// Do not call this method to retrieve an object's own dependencies; use constructor injection instead.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arg">The argument, if needed.</param>
        /// <returns>An instance of the type.</returns>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public object Get( Type type, object arg )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            object instance;
            if( _instances.TryGetValue( type, out instance ) )
            {
                return instance;
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
                if( arg != null && parameters[n].ParameterType == arg.GetType() )
                {
                    arguments[n] = arg;
                    arg = null;
                }
                else
                {
                    arguments[n] = Get( parameters[n].ParameterType, null );
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
                        throw new ArgumentException( $"Cannot instantiate type '{typeInfo.FullName }' because it has multiple public constructors." );
                    }

                    constructor = candidate;
                }
            }

            if( constructor == null )
            {
                throw new ArgumentException( $"Cannot instantiate type '{ typeInfo.FullName}' because it has no public constructor." );
            }

            return constructor;
        }
    }
}