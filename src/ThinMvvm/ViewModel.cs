using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    /// <summary>
    /// Base class for ViewModels.
    /// </summary>
    /// <typeparam name="TParameter">Type of the navigation parameter, or <see cref="NoParameter" /> if the ViewModel is parameterless.</typeparam>
    public abstract class ViewModel<TParameter> : IViewModel
    {
        private event EventHandler<EventArgs> _navigatedTo;
        private event EventHandler<EventArgs> _navigatedFrom;


        /// <summary>
        /// Responds to a navigation to the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        protected virtual Task OnNavigatedToAsync( NavigationKind navigationKind )
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Responds to a navigation away from the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        protected virtual Task OnNavigatedFromAsync( NavigationKind navigationKind )
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Loads the ViewModel's state from the specified store.
        /// </summary>
        /// <param name="store">The store containing values set by this ViewModel last time <see cref="SaveState" /> was called.</param>
        /// <remarks>
        /// This method is called when the ViewModel needs to be inflated from previously stored data,
        /// for instance when navigating back to it if it had been tombstoned by the operating system.
        /// </remarks>
        public void LoadState( IKeyValueStore store )
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
        public void SaveState( IKeyValueStore store )
        {
            // Nothing.
        }


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

        Task IViewModel.OnNavigatedToAsync( NavigationKind navigationKind )
        {
            _navigatedTo?.Invoke( this, EventArgs.Empty );
            return OnNavigatedToAsync( navigationKind );
        }

        Task IViewModel.OnNavigatedFromAsync( NavigationKind navigationKind )
        {
            _navigatedFrom?.Invoke( this, EventArgs.Empty );
            return OnNavigatedFromAsync( navigationKind );
        }
    }
}