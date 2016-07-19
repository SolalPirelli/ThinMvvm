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
        /// Gets a value indicating whether the ViewModel should be kept into navigation history.
        /// </summary>
        bool IsTransient { get; }

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
        /// <remarks>
        /// Navigation service implementations may call this without awaiting its result,
        /// if the platform does not support asynchronous navigation.
        /// </remarks>
        Task OnNavigatedToAsync( NavigationKind navigationKind );

        /// <summary>
        /// Responds to a navigation away from the ViewModel.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation response operation.</returns>
        /// <remarks>
        /// Navigation service implementations may call this without awaiting its result,
        /// if the platform does not support asynchronous navigation.
        /// </remarks>
        Task OnNavigatedFromAsync( NavigationKind navigationKind );

        /// <summary>
        /// Loads the ViewModel's state from the specified store.
        /// </summary>
        /// <param name="store">The store containing values set by this ViewModel last time <see cref="SaveState" /> was called.</param>
        /// <remarks>
        /// This method, if it is called, is guaranteed to be called before <see cref="OnNavigatedToAsync" /> when navigating to the ViewModel.
        /// </remarks>
        void LoadState( IKeyValueStore store );

        /// <summary>
        /// Saves the ViewModel's state to the specified store.
        /// </summary>
        /// <param name="store">The store, specific to this ViewModel.</param>
        /// <remarks>
        /// This method, if it is called, is guaranteed to be called before <see cref="OnNavigatedFromAsync" /> when navigating away from the ViewModel.
        /// </remarks>
        void SaveState( IKeyValueStore store );
    }
}