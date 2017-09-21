using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ThinMvvm
{
    /// <summary>
    /// Base class for objects whose property changes can be observed.
    /// </summary>
    [DataContract]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Sets the specified field to the specified value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        protected void Set<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            if( propertyName == null )
            {
                throw new ArgumentNullException( nameof( propertyName ) );
            }

            if( !EqualityComparer<T>.Default.Equals( field, value ) )
            {
                field = value;
                OnPropertyChanged( propertyName );
            }
        }

        /// <summary>
        /// Triggers the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">The property name, or null if the entire object state changed.</param>
        protected void OnPropertyChanged( string propertyName )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}