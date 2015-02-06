// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace ThinMvvm
{
    /// <summary>
    /// Base class for observable objects.
    /// </summary>
    [DataContract]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private readonly WeakEvent _propertyChanged;
        // Used to send PropertyChanged messages on the right thread
        private readonly SynchronizationContext _context;


        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObject" /> class.
        /// </summary>
        protected ObservableObject()
        {
            _propertyChanged = new WeakEvent();
            _context = SynchronizationContext.Current;
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged.Add( value ); }
            remove { _propertyChanged.Remove( value ); }
        }

        /// <summary>
        /// Fires a property changed event.
        /// </summary>
        /// <param name="propertyName">Optional. The property's name. If unset, the compiler will fill this parameter in.</param>
        protected void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            if ( _context == null )
            {
                _propertyChanged.Raise( this, new PropertyChangedEventArgs( propertyName ) );
            }
            else
            {
                _context.Post( _ => _propertyChanged.Raise( this, new PropertyChangedEventArgs( propertyName ) ), null );
            }
        }

        /// <summary>
        /// Sets the specified field to the specified value and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Optional. The property's name. If unset, the compiler will fill this parameter in.</param>
        protected void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                OnPropertyChanged( propertyName );
            }
        }
    }
}