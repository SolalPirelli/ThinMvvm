using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// TODO the change tracking part here is not that great;
//      at least need a way to stop tracking old values.
// Also, test coverage is dubious, there was a bug with name/key mixups that didn't get caught.

// TODO having to use as a singleton kinda sucks...

namespace ThinMvvm
{
    /// <summary>
    /// Base class for settings.
    /// 
    /// If an object implementing <see cref="INotifyPropertyChanged" /> or <see cref="INotifyCollectionChanged" /> is stored,
    /// it will be re-written into the persistent store every time it fires a change notification.
    /// 
    /// Classes deriving from this must be injected as singletons (and not re-created each time),
    /// so that changes will be propagated correctly.
    /// </summary>
    public abstract class SettingsBase : INotifyPropertyChanged
    {
        private readonly StoreCache _store;
        private readonly string _keyPrefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBase" /> class, using the specified store.
        /// </summary>
        /// <param name="store">The store.</param>
        protected SettingsBase( IKeyValueStore store )
        {
            if( store == null )
            {
                throw new ArgumentNullException( nameof( store ) );
            }

            _store = new StoreCache( store );
            // TODO Calling a virtual func in the ctor isn't very nice :/
            _keyPrefix = CreateStoragePrefix();
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        // TODO would be nicer to have KVStore be hierarchical...
        // Or maybe have 1 interface for the creator, i.e. CreateKV(key) / CreateData(key) ?
        /// <summary>
        /// Creates the prefix used to store keys.
        /// </summary>
        /// <returns>The prefix.</returns>
        protected virtual string CreateStoragePrefix()
        {
            return GetType().FullName + ".";
        }

        /// <summary>
        /// Gets a property, using the specified default value.
        /// </summary>
        /// <typeparam name="T">The property's type.</typeparam>
        /// <param name="defaultValue">The property's default value.</param>
        /// <param name="name">The property's name.</param>
        /// <returns>The property's value.</returns>
        protected T Get<T>( T defaultValue, [CallerMemberName] string name = "" )
        {
            if( name == null )
            {
                throw new ArgumentNullException( nameof( name ) );
            }

            bool wasInCache;
            var stored = _store.Get<T>( _keyPrefix + name, out wasInCache );
            if( stored.HasValue )
            {
                if( !wasInCache )
                {
                    ListenToChanges( name, stored.Value );
                }

                return stored.Value;
            }

            SetAndNotify( name, defaultValue );
            return defaultValue;
        }

        /// <summary>
        /// Gets a property, using the specified lazy default value.
        /// </summary>
        /// <typeparam name="T">The property's type.</typeparam>
        /// <param name="defaultValueFactory">A factory to create the property's default value.</param>
        /// <param name="name">The property's name.</param>
        /// <returns>The property's value.</returns>
        protected T Get<T>( Func<T> defaultValueFactory, [CallerMemberName] string name = "" )
        {
            if( name == null )
            {
                throw new ArgumentNullException( nameof( name ) );
            }

            bool wasInCache;
            var stored = _store.Get<T>( _keyPrefix + name, out wasInCache );
            if( stored.HasValue )
            {
                if( !wasInCache )
                {
                    ListenToChanges( name, stored.Value );
                }

                return stored.Value;
            }

            var defaultValue = defaultValueFactory();
            SetAndNotify( name, defaultValue );
            return defaultValue;
        }

        /// <summary>
        /// Sets the specified property to the specified value.
        /// </summary>
        /// <typeparam name="T">The property's type.</typeparam>
        /// <param name="value">The property's value.</param>
        /// <param name="name">The property's name.</param>
        protected void Set<T>( T value, [CallerMemberName] string name = "" )
        {
            if( name == null )
            {
                throw new ArgumentNullException( nameof( name ) );
            }

            bool ignored;
            var stored = _store.Get<T>( _keyPrefix + name, out ignored );
            if( !stored.HasValue || !object.Equals( value, stored.Value ) )
            {
                SetAndNotify( name, value );
            }
        }


        /// <summary>
        /// Sets the specified property to the specified value.
        /// </summary>
        private void SetAndNotify<T>( string name, T value )
        {
            var key = _keyPrefix + name;
            _store.Set( key, value );
            OnPropertyChanged( name );
            ListenToChanges( name, value );
        }

        /// <summary>
        /// Triggers the <see cref="PropertyChanged" /> event.
        /// </summary>
        private void OnPropertyChanged( string propertyName )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// Listens to property and collection changes of the specified value,
        /// in order to re-write it to the persistent store when it changes.
        /// </summary>
        /// <remarks>
        /// This method must be generic (and cast its _store.Set argument) for .NET Native
        /// to figure out which serialization metadata to generate automatically.
        /// </remarks>
        private void ListenToChanges<T>( string name, T value )
        {
            var collectionNotifier = value as INotifyCollectionChanged;
            var propertyNotifier = value as INotifyPropertyChanged;

            if( propertyNotifier != null )
            {
                if( collectionNotifier == null )
                {
                    propertyNotifier.PropertyChanged += ( o, _ ) => _store.Set( _keyPrefix + name, (T) o );
                }
                else
                {
                    propertyNotifier.PropertyChanged += ( o, e ) =>
                    {
                        if( e.PropertyName != "Count" && e.PropertyName != "Item[]" )
                        {
                            _store.Set( _keyPrefix + name, (T) o );
                        }
                    };
                }
            }

            if( collectionNotifier != null )
            {
                collectionNotifier.CollectionChanged += ( o, _ ) => _store.Set( _keyPrefix + name, (T) o );
            }
        }


        /// <summary>
        /// Cache for the store, in order to avoid calling <see cref="IKeyValueStore.Get" /> too much.
        /// This is because most stores serialize values before storing them, which incurs a non-trivial cost.
        /// </summary>
        private sealed class StoreCache
        {
            private readonly IKeyValueStore _wrapped;
            private readonly Dictionary<string, object> _cache;


            /// <summary>
            /// Initializes a new instance of the <see cref="IKeyValueStore" /> class, wrapping the specified store.
            /// </summary>
            /// <param name="wrapped">The wrapped store.</param>
            public StoreCache( IKeyValueStore wrapped )
            {
                _wrapped = wrapped;
                _cache = new Dictionary<string, object>();
            }


            /// <summary>
            /// Gets the value corresponding to the specified key, if it exists,
            /// first looking into an in-memory cache.
            /// Also returns whether the value was found in the cache.
            /// </summary>
            public Optional<T> Get<T>( string key, out bool wasInCache )
            {
                object boxedValue;
                wasInCache = _cache.TryGetValue( key, out boxedValue );
                if( wasInCache )
                {
                    return new Optional<T>( (T) boxedValue );
                }

                var stored = _wrapped.Get<T>( key );
                if( stored.HasValue )
                {
                    _cache[key] = stored.Value;
                }

                return stored;
            }

            /// <summary>
            /// Sets the specified value for the specified key, and stores it in an in-memory cache as well.
            /// </summary>
            public void Set<T>( string key, T value )
            {
                _wrapped.Set( key, value );
                _cache[key] = value;
            }
        }
    }
}