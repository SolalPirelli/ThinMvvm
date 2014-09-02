// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Design
{
    /// <summary>
    /// <see cref="INavigationService" /> implementation for design-time ViewModel construction.
    /// Does nothing.
    /// </summary>
    public sealed class DesignNavigationService : INavigationService
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <typeparam name="T">Unused.</typeparam>
        public void NavigateTo<T>() where T : ViewModel<NoParameter> { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <typeparam name="TViewModel">Unused.</typeparam>
        /// <typeparam name="TParameter">Unused.</typeparam>
        /// <param name="arg">Unused.</param>
        public void NavigateTo<TViewModel, TParameter>( TParameter arg ) where TViewModel : ViewModel<TParameter> { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void NavigateBack() { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void RemoveCurrentFromBackStack() { }

        /// <summary>
        /// Never occurs.
        /// </summary>
#pragma warning disable 0067 // Unused event
        public event EventHandler<NavigatedEventArgs> Navigated;
#pragma warning restore 0067
    }
}