// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    /// <summary>
    /// Base class for settings.
    /// </summary>
    /// <typeparam name="TSelf">The type of the derived class implementing this class.</typeparam>
    public abstract class SettingsBase<TSelf> : ObservableObject
        where TSelf : SettingsBase<TSelf>
    {
        private const string KeyPrefixSeparator = ".";

        private readonly ISettingsStorage _storage;
        private readonly ISettingsStorage _live;
        private readonly string _keyPrefix;

        private Dictionary<string, Func<object>> _defaultValues;


        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBase{TSelf}" /> class.
        /// </summary>
        /// <param name="storage">The settings storage. Implementors' constructors should also take it as a parameter.</param>
        protected SettingsBase( ISettingsStorage storage )
        {
            if ( storage == null )
            {
                throw new ArgumentNullException( "storage" );
            }

            _storage = storage;
            _live = new InMemorySettingsStorage();
            _keyPrefix = GetType().FullName + KeyPrefixSeparator;
        }


        /// <summary>
        /// Gets the default values for all settings.
        /// </summary>
        protected abstract SettingsDefaultValues GetDefaultValues();


        /// <summary>
        /// Gets the specified setting's value, as an object of the specified type.
        /// This method is intended to be used from a property's get block.
        /// </summary>
        /// <param name="propertyName">The property name. This should not be specified; it will be filled in by the compiler.</param>
        protected T Get<T>( [CallerMemberName] string propertyName = "" )
        {
            string key = GetKey( propertyName );

            if ( _live.IsDefined( key ) )
            {
                return _live.Get<T>( key );
            }

            if ( _storage.IsDefined( key ) )
            {
                var value = _storage.Get<T>( key );
                RegisterToChanges( key, value );
                _live.Set( key, value );
                return value;
            }

            var defaultValue = GetDefaultValue<T>( propertyName );
            RegisterToChanges( key, defaultValue );
            _storage.Set( key, defaultValue );
            _live.Set( key, defaultValue );
            return defaultValue;
        }

        /// <summary>
        /// Sets the specified setting's value.
        /// This method is intended to be used from a property's set block.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">The property name. This should not be specified; it will be filled in by the compiler.</param>
        protected void Set<T>( T value, [CallerMemberName] string propertyName = "" )
        {
            string key = GetKey( propertyName );

            if ( !_live.IsDefined( key ) || !object.Equals( _live.Get<T>( key ), value ) )
            {
                RegisterToChanges( key, value );
            }

            _live.Set( key, value );
            _storage.Set( key, value );

            OnPropertyChanged( propertyName );
        }


        /// <summary>
        /// Gets the default value for the specified property.
        /// </summary>
        private T GetDefaultValue<T>( string propertyName )
        {
            if ( _defaultValues == null )
            {
                _defaultValues = GetDefaultValues().AsDictionary;
            }

            if ( !_defaultValues.ContainsKey( propertyName ) )
            {
                throw new InvalidOperationException( "Settings: no default value found for property " + propertyName );
            }

            return (T) _defaultValues[propertyName]();
        }


        /// <summary>
        /// Gets the settings key for the specified property.
        /// </summary>
        /// <remarks>
        /// This ensures that different settings types don't collide
        /// e.g. if SettingsA and SettingsB both inherit from SettingsBase and have a property named X,
        /// setting A.X shouldn't affect B.X.
        /// </remarks>
        private string GetKey( string propertyName )
        {
            return _keyPrefix + propertyName;
        }

        /// <summary>
        /// Register to change notifications for the specified object, with the specified settings key.
        /// </summary>
        private void RegisterToChanges( string key, object value )
        {
            var propNotif = value as INotifyPropertyChanged;
            if ( propNotif != null )
            {
                propNotif.PropertyChanged += ( s, _ ) => _storage.Set( key, s );
            }

            var collNotif = value as INotifyCollectionChanged;
            if ( collNotif != null )
            {
                collNotif.CollectionChanged += ( s, _ ) => _storage.Set( key, s );
            }
        }


        /// <summary>
        /// Dictionary containing settings default values.
        /// This class is intended to be used with the collection initializer syntax,
        /// e.g. <code>new SettingsDefaultValues&lt;MySettings&gt; { { x => x.SomeSetting, () => 42 } }</code>
        /// </summary>
        protected sealed class SettingsDefaultValues : IEnumerable
        {
            internal readonly Dictionary<string, Func<object>> AsDictionary = new Dictionary<string, Func<object>>();


            /// <summary>
            /// Adds the specified key-value mapping.
            /// This function is not intended to be called explicitly; use the collection initializer syntax instead.
            /// </summary>
            /// <typeparam name="TProp">The type of the property.</typeparam>
            /// <param name="expr">The property expression.</param>
            /// <param name="value">The value.</param>
            public void Add<TProp>( Expression<Func<TSelf, TProp>> expr, Func<TProp> value )
            {
                AsDictionary.Add( ExpressionHelper.GetPropertyName( expr ), () => (object) value() );
            }

            /// <summary>
            /// Not supported.
            /// Do not call this method, or try to iterate over an instance of <see cref="SettingsDefaultValues" />.
            /// </summary>
            /// <returns>This method always throws a <see cref="NotSupportedException" />.</returns>
            /// <remarks>
            /// This method's existence is required for collection initializers; see §7.6.10.3 of the C# spec.
            /// </remarks>
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// In-memory settings storage, to avoid having to (de)serialize objects on each get/set call.
        /// </summary>
        private sealed class InMemorySettingsStorage : ISettingsStorage
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();


            public bool IsDefined( string key )
            {
                return _values.ContainsKey( key );
            }

            public T Get<T>( string key )
            {
                return (T) _values[key];
            }

            public void Set( string key, object value )
            {
                _values[key] = value;
            }
        }
    }
}