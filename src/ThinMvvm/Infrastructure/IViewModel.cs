using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Base interface for ViewModels.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public interface IViewModel
    {
        /// <summary>
        /// Infrastructure.
        /// Occurs when the ViewModel is navigated to.
        /// </summary>
        event EventHandler<EventArgs> NavigatedTo;

        /// <summary>
        /// Infrastructure.
        /// Occurs when the ViewModel is navigated from.
        /// </summary>
        event EventHandler<EventArgs> NavigatedFrom;

        /// <summary>
        /// Responds to a navigation to the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        Task OnNavigatedToAsync( NavigationKind navigationKind );

        /// <summary>
        /// Responds to a navigation away from the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        Task OnNavigatedFromAsync( NavigationKind navigationKind );

        /// <summary>
        /// Loads the ViewModel's state from the specified store.
        /// </summary>
        /// <param name="store">The store containing values set by this ViewModel last time <see cref="SaveState" /> was called.</param>
        /// <remarks>
        /// This method is called when the ViewModel needs to be inflated from previously stored data,
        /// for instance when navigating back to it if it had been tombstoned by the operating system.
        /// </remarks>
        void LoadState( IKeyValueStore store );

        /// <summary>
        /// Saves the ViewModel's state to the specified store.
        /// </summary>
        /// <param name="store">The store, specific to this ViewModel.</param>
        /// <remarks>
        /// This method is called when the operating system tombstones the ViewModel, 
        /// for instance because it needs to free memory.
        /// </remarks>
        void SaveState( IKeyValueStore store );
    }
}