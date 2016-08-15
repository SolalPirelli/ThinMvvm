using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    /// <summary>
    /// Base class for ViewModels.
    /// </summary>
    /// <typeparam name="TParameter">The navigation parameter type, or <see cref="NoParameter" /> if the ViewModel is parameterless.</typeparam>
    public abstract class ViewModel<TParameter> : ObservableObject, IViewModel
    {
        // COMPAT: Task.CompletedTask does not exist in Profile111
        private static readonly Task CompletedTask = Task.FromResult( 0 );

        private event EventHandler<EventArgs> _navigatedTo;
        private event EventHandler<EventArgs> _navigatedFrom;

        /// <summary>
        /// Gets a value indicating whether the ViewModel should be kept into navigation history.
        /// </summary>
        /// <remarks>
        /// Most ViewModels should not be transient, as users expect normal navigation behavior.
        ///
        /// An example of a transient ViewModels is authentication to access a protected resource;
        /// if a guest user is on the main app page and opens a page that requires authentication,
        /// a login page should be shown. However, when the user navigates back from the protected
        /// page, they should be back in the main page, not in the login page.
        /// </remarks>
        protected virtual bool IsTransient
        {
            get { return false; }
        }

        /// <summary>
        /// Responds to a navigation to the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        protected virtual Task OnNavigatedToAsync( NavigationKind navigationKind )
        {
            return CompletedTask;
        }

        /// <summary>
        /// Responds to a navigation away from the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        protected virtual Task OnNavigatedFromAsync( NavigationKind navigationKind )
        {
            return CompletedTask;
        }

        /// <summary>
        /// Loads the ViewModel's state from the specified store.
        /// </summary>
        /// <param name="store">The store containing values set by this ViewModel last time <see cref="SaveState" /> was called.</param>
        /// <remarks>
        /// This method is called when the ViewModel needs to be inflated from previously stored data,
        /// for instance when navigating back to it if it had been tombstoned by the operating system.
        /// </remarks>
        public virtual void LoadState( IKeyValueStore store )
        {
            // Nothing.
        }

        /// <summary>
        /// Saves the ViewModel's state to the specified store.
        /// </summary>
        /// <param name="store">The store, specific to this ViewModel.</param>
        /// <remarks>
        /// This method is called when the operating system tombstones the ViewModel, 
        /// for instance because it needs to free memory.
        /// </remarks>
        public virtual void SaveState( IKeyValueStore store )
        {
            // Nothing.
        }


        // This is both a protected virtual property and explicitly implemented,
        // so that only subclasses can see it normally, e.g. not a designer when suggesting properties to bind.
        bool IViewModel.IsTransient => IsTransient;

        // The events and associated machinery are explicitly implemented,
        // both to guarantee that they will always be fired properly,
        // and to hide them from users since they're infrastructure details.

        event EventHandler<EventArgs> IViewModel.NavigatedTo
        {
            add { _navigatedTo += value; }
            remove { _navigatedTo -= value; }
        }

        event EventHandler<EventArgs> IViewModel.NavigatedFrom
        {
            add { _navigatedFrom += value; }
            remove { _navigatedFrom -= value; }
        }

        async Task IViewModel.OnNavigatedToAsync( NavigationKind navigationKind )
        {
            await OnNavigatedToAsync( navigationKind );
            _navigatedTo?.Invoke( this, EventArgs.Empty );
        }

        async Task IViewModel.OnNavigatedFromAsync( NavigationKind navigationKind )
        {
            await OnNavigatedFromAsync( navigationKind );
            _navigatedFrom?.Invoke( this, EventArgs.Empty );
        }
    }
}