using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;
using ThinMvvm.Logging;

namespace ThinMvvm.Windows
{
    public abstract class DesignViewModelCreator
    {
        private readonly IServiceCreator _services;


        protected DesignViewModelCreator()
        {
            var binder = new ServiceBinder();
            ConfigureServices( binder );
            _services = binder;
        }


        protected virtual void ConfigureServices( ServiceBinder binder )
        {
            binder.Bind<INavigationService, FakeNavigationService>();
            binder.Bind<IKeyValueStore, FakeKeyValueStore>();
            binder.Bind<IDataStore, FakeDataStore>();
            binder.Bind<ILogger, FakeLogger>();
        }

        protected TViewModel Create<TViewModel>()
            where TViewModel : ViewModel<NoParameter>
        {
            return Create<TViewModel>( null );
        }

        protected TViewModel Create<TViewModel, TParameter>( TParameter arg )
            where TViewModel : ViewModel<TParameter>
        {
            return Create<TViewModel>( arg );
        }


        private TViewModel Create<TViewModel>( object arg )
            where TViewModel : IViewModel
        {
            var value = (TViewModel) _services.Create( typeof( TViewModel ), arg );
            value.OnNavigatedToAsync( NavigationKind.Forwards ).Wait();
            return value;
        }


        public sealed class FakeNavigationService : INavigationService
        {
#pragma warning disable CS0067 // Event is never used
            public event EventHandler<NavigatedEventArgs> Navigated;
#pragma warning restore CS0067

            public bool CanNavigateBack => false;

            public void NavigateTo<TViewModel>()
                where TViewModel : ViewModel<NoParameter>
            {
                // Nothing.
            }

            public void NavigateTo<TViewModel, TParameter>( TParameter arg )
                where TViewModel : ViewModel<TParameter>
            {
                // Nothing.
            }

            public void NavigateBack()
            {
                // Nothing.
            }

            public void Reset()
            {
                // Nothing.
            }
        }

        public sealed class FakeKeyValueStore : IKeyValueStore
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            public Optional<T> Get<T>( string key )
            {
                object value;
                if( _values.TryGetValue( key, out value ) )
                {
                    return new Optional<T>( (T) value );
                }

                return default( Optional<T> );
            }

            public void Set<T>( string key, T value )
            {
                _values[key] = value;
            }

            public void Delete( string key )
            {
                _values.Remove( key );
            }
        }

        public sealed class FakeDataStore : IDataStore
        {
            private readonly FakeKeyValueStore _store = new FakeKeyValueStore();

            public Task<Optional<T>> LoadAsync<T>( string id )
            {
                return Task.FromResult( _store.Get<T>( id ) );
            }

            public Task StoreAsync<T>( string id, T data )
            {
                _store.Set( id, data );
                return Task.CompletedTask;
            }

            public Task DeleteAsync( string id )
            {
                _store.Delete( id );
                return Task.CompletedTask;
            }
        }

        public sealed class FakeLogger : ILogger
        {
            public void LogEvent( string viewModelId, string eventId, string label )
            {
                // Nothing.
            }

            public void LogNavigation( string viewModelId, bool isArriving )
            {
                // Nothing.
            }
        }
    }
}