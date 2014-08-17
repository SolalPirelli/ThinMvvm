// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.Navigated" /> event.
    /// </summary>
    public sealed class NavigatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ViewModel that was navigated to.
        /// </summary>
        public object ViewModel { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the navigation went forwards.
        /// </summary>
        public bool IsForward { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatedEventArgs" /> class with the specified parameters.
        /// </summary>
        /// <param name="viewModel">The ViewModel that was navigated to.</param>
        /// <param name="isForward">A value indicating whether the navigation went forwards.</param>
        public NavigatedEventArgs( object viewModel, bool isForward )
        {
            if ( viewModel == null )
            {
                throw new ArgumentNullException( "viewModel" );
            }

            ViewModel = viewModel;
            IsForward = isForward;
        }
    }
}