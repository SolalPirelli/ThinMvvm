// Copyright (c) Solal Pirelli 2014
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
    /// Helper base class for settings.
    /// </summary>
    /// <typeparam name="TSelf">The type of the derived class implementing this class.</typeparam>
    public abstract class SettingsBase<TSelf> : ObservableObject
        where TSelf : SettingsBase<TSelf>
    {
        private readonly ISettingsStorage _settings;

        private Dictionary<string, Func<object>> _defaultValues;


        /// <summary>
        /// Creates a new instance of the <see cref="SettingsBase{TSelf}" /> class.
        /// </summary>
        /// <param name="settings">The settings storage. Implementors' constructors should also take it as a parameter.</param>
        protected SettingsBase( ISettingsStorage settings )
        {
            _settings = settings;
        }


        /// <summary>
        /// Gets the default values for all settings.
        /// </summary>
        protected abstract SettingsDefaultValues GetDefaultValues();


        /// <summary>
        /// Gets the specified setting's value, as an object of the specified type.
        /// This method is intended to be used from a property's get block.
        /// </summary>
        protected T Get<T>( [CallerMemberName] string propertyName = "" )
        {
            SetIfUndefined( propertyName );
            return _settings.Get<T>( propertyName );
        }

        /// <summary>
        /// Sets the specified setting's value.
        /// This method is intended to be used from a property's set block.
        /// </summary>
        protected void Set( object value, [CallerMemberName] string propertyName = "" )
        {
            _settings.Set( propertyName, value );

            OnPropertyChanged( propertyName );

            var propNotif = value as INotifyPropertyChanged;
            if ( propNotif != null )
            {
                propNotif.PropertyChanged += ( s, _ ) => _settings.Set( propertyName, s );
            }

            var collNotif = value as INotifyCollectionChanged;
            if ( collNotif != null )
            {
                collNotif.CollectionChanged += ( s, _ ) => _settings.Set( propertyName, s );
            }
        }


        /// <summary>
        /// If the specified setting is undefined, set it to its default value.
        /// </summary>
        private void SetIfUndefined( string key )
        {
            if ( _settings.IsDefined( key ) )
            {
                return;
            }

            if ( _defaultValues == null )
            {
                _defaultValues = GetDefaultValues().AsDictionary;
            }

            if ( !_defaultValues.ContainsKey( key ) )
            {
                throw new InvalidOperationException( "Settings: no default value found for key " + key );
            }

            Set( _defaultValues[key](), key );
        }


        /// <summary>
        /// Dictionary containing settings default values.
        /// This class is intended to be used with the collection initializer syntax,
        /// e.g. <code>new SettingsDefaultValues&lt;MySettings&gt; { { x => x.SomeSetting, 42 } }</code>
        /// </summary>
        protected sealed class SettingsDefaultValues : IEnumerable
        {
            internal Dictionary<string, Func<object>> AsDictionary { get; private set; }


            /// <summary>
            /// Creates a new instance of the <see cref="SettingsDefaultValues" /> class.
            /// </summary>
            public SettingsDefaultValues()
            {
                AsDictionary = new Dictionary<string, Func<object>>();
            }


            /// <summary>
            /// Adds the specified key-value mapping.
            /// This function is not intended to be called from your code; use the collection initializer syntax instead.
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
    }
}