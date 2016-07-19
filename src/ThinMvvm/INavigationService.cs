using System;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    /// <summary>
    /// Provides navigation between ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Occurs after navigating to a ViewModel.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be fired after <see cref="IViewModel.OnNavigatedFromAsync" /> in the source ViewModel,
        /// and before <see cref="IViewModel.OnNavigatedToAsync" /> in the target ViewModel.
        /// </remarks>
        event EventHandler<NavigatedEventArgs> Navigated;

        /// <summary>
        /// Gets a value indicating whether the navigation service can navigate back.
        /// </summary>
        bool CanNavigateBack { get; }

        /// <summary>
        /// Navigates to the specified parameterless ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        void NavigateTo<TViewModel>()
            where TViewModel : ViewModel<NoParameter>;

        /// <summary>
        /// Navigates to the specified parameterized ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TParameter">The type of the ViewModel's parameter.</typeparam>
        /// <param name="arg">The argument.</param>
        void NavigateTo<TViewModel, TParameter>( TParameter arg )
            where TViewModel : ViewModel<TParameter>;

        /// <summary>
        /// Navigates back to the previous ViewModel.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Resets the service, as if no navigation had occurred.
        /// </summary>
        void Reset();
    }
}