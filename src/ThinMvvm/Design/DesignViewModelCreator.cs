using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Infrastructure;
using ThinMvvm.Logging;

namespace ThinMvvm.Design
{
    /// <summary>
    /// Base class for design view model collections.
    /// </summary>
    public abstract class DesignViewModelCreator
    {
        private readonly ObjectCreator _creator;


        /// <summary>
        /// Initializes a new instance of the <see cref="DesignViewModelCreator" /> class.
        /// </summary>
        protected DesignViewModelCreator()
        {
            var services = new ServiceCollection();
            ConfigureServices( services );
            _creator = services.BuildCreator();
        }


        /// <summary>
        /// Configures the design-time services.
        /// 
        /// The default implementation registers a design-time dummy for all ThinMvvm services.
        /// </summary>
        /// <param name="services">The services collection.</param>
        protected virtual void ConfigureServices( ServiceCollection services )
        {
            services.AddSingleton<INavigationService, FakeNavigationService>();
            services.AddSingleton<IKeyValueStore, FakeKeyValueStore>();
            services.AddSingleton<IDataStore, FakeDataStore>();
            services.AddSingleton<ILogger, FakeLogger>();
        }

        /// <summary>
        /// Creates a ViewModel of the specified type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <returns>A ViewModel.</returns>
        protected TViewModel Create<TViewModel>()
            where TViewModel : ViewModel<NoParameter>
        {
            return Create<TViewModel>( null );
        }

        /// <summary>
        /// Creates a ViewModel of the specified type using the specified argument.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TArg">The argument type.</typeparam>
        /// <returns>A ViewModel.</returns>
        protected TViewModel Create<TViewModel, TArg>( TArg arg )
            where TViewModel : ViewModel<TArg>
        {
            return Create<TViewModel>( arg );
        }


        /// <summary>
        /// Creates a ViewModel of the specified type using the specified argument.
        /// </summary>
        private TViewModel Create<TViewModel>( object arg )
            where TViewModel : IViewModel
        {
            var value = (TViewModel) _creator.Create( typeof( TViewModel ) );
            value.Initialize( arg );
            value.OnNavigatedToAsync( NavigationKind.Forwards ).Wait();
            return value;
        }


        /// <summary>
        /// Design-time dummy implementation of <see cref="INavigationService" />.
        /// </summary>
        protected sealed class FakeNavigationService : INavigationService
        {
            event EventHandler<NavigatedEventArgs> INavigationService.Navigated
            {
                add
                {
                    // Nothing.
                }
                remove
                {
                    // Nothing.
                }
            }

            bool INavigationService.CanNavigateBack => false;

            void INavigationService.NavigateTo<TViewModel>()
            {
                // Nothing.
            }

            // The .NET Native compiler throws an internal error if this method is implemented explicitly.
            // TODO: Once that bug is fixed, implement this method explicitly.
            /// <summary>
            /// Do not call this method.
            /// </summary>
            /// <typeparam name="TViewModel">Ignored.</typeparam>
            /// <typeparam name="TArg">Ignored.</typeparam>
            /// <param name="arg">Ignored.</param>
            public void NavigateTo<TViewModel, TArg>( TArg arg )
                where TViewModel : ViewModel<TArg>
            {
                // Nothing.
            }

            void INavigationService.NavigateBack()
            {
                // Nothing.
            }

            void INavigationService.Reset()
            {
                // Nothing.
            }

            bool INavigationService.RestorePreviousState()
            {
                return false;
            }
        }

        /// <summary>
        /// Design-time dummy implementation of <see cref="IKeyValueStore" />.
        /// </summary>
        protected sealed class FakeKeyValueStore : IKeyValueStore
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            Optional<T> IKeyValueStore.Get<T>( string key )
            {
                object value;
                if( _values.TryGetValue( key, out value ) )
                {
                    return new Optional<T>( (T) value );
                }

                return default( Optional<T> );
            }

            void IKeyValueStore.Set<T>( string key, T value )
            {
                _values[key] = value;
            }

            void IKeyValueStore.Delete( string key )
            {
                _values.Remove( key );
            }

            void IKeyValueStore.Clear()
            {
                _values.Clear();
            }
        }

        /// <summary>
        /// Design-time dummy implementation of <see cref="IDataStore" />.
        /// </summary>
        protected sealed class FakeDataStore : IDataStore
        {
            private readonly IKeyValueStore _store = new FakeKeyValueStore();


            Task<Optional<T>> IDataStore.LoadAsync<T>( string id )
            {
                return Task.FromResult( _store.Get<T>( id ) );
            }

            Task IDataStore.StoreAsync<T>( string id, T data )
            {
                _store.Set( id, data );
                return Task.FromResult( 0 );
            }

            Task IDataStore.DeleteAsync( string id )
            {
                _store.Delete( id );
                return Task.FromResult( 0 );
            }
        }

        /// <summary>
        /// Design-time dummy implementation of <see cref="ILogger" />.
        /// </summary>
        public sealed class FakeLogger : ILogger
        {
            void ILogger.LogNavigation( string viewModelId, bool isArriving )
            {
                // Nothing.
            }

            void ILogger.LogEvent( string viewModelId, string eventId, string label )
            {
                // Nothing.
            }

            void ILogger.LogError( string name, Exception exception )
            {
                // Nothing.
            }
        }
    }
}