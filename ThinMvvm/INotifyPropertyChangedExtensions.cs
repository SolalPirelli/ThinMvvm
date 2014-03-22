// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    /// <summary>
    /// Extensions for INotifyPropertyChanged.
    /// </summary>
    public static class INotifyPropertyChangedExtensions
    {
        /// <summary>
        /// Adds a listener to changes on the specified property of the item.
        /// </summary>
        /// <typeparam name="TNotifier">The type of the item.</typeparam>
        /// <typeparam name="TProp">The type of the property.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="propertyExpr">An expression that returns the property to listen to.</param>
        /// <param name="listener">The listener.</param>
        public static void ListenToProperty<TNotifier, TProp>( this TNotifier item, Expression<Func<TNotifier, TProp>> propertyExpr, Action listener )
            where TNotifier : INotifyPropertyChanged
        {
            ListenToProperty( item, ExpressionHelper.GetPropertyName( propertyExpr ), listener );
        }

        /// <summary>
        /// Adds a listener to changes on the property with the specified name of the item.
        /// </summary>
        internal static void ListenToProperty<TNotifier>( this TNotifier item, string propertyName, Action listener )
            where TNotifier : INotifyPropertyChanged
        {
            item.PropertyChanged += ( _, e ) =>
            {
                if ( e.PropertyName == propertyName )
                {
                    listener();
                }
            };
        }
    }
}