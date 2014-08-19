// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.WindowsPhone.Design
{
    /// <summary>
    /// <see cref="IWindowsPhoneNavigationService" /> implementation for design-time ViewModel construction.
    /// Does nothing.
    /// </summary>
    public sealed class DesignNavigationService : IWindowsPhoneNavigationService
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <typeparam name="TViewModel">Unused.</typeparam>
        /// <param name="viewUri">Unused.</param>
        public void Bind<TViewModel>( string viewUri ) { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <typeparam name="T">Unused.</typeparam>
        public void NavigateTo<T>() where T : ViewModel<NoParameter> { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <typeparam name="TViewModel">Unused.</typeparam>
        /// <typeparam name="TArg">Unused.</typeparam>
        /// <param name="arg">Unused.</param>
        public void NavigateTo<TViewModel, TArg>( TArg arg ) where TViewModel : ViewModel<TArg> { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void NavigateBack() { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void PopBackStack() { }

        /// <summary>
        /// Never occurs.
        /// </summary>
#pragma warning disable 0067 // Unused event
        public event EventHandler<NavigatedEventArgs> Navigated;
#pragma warning restore 0067
    }
}