using System;

namespace ThinMvvm
{
    /// <summary>
    /// Provides navigation between ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets a value indicating whether the navigation service can currently navigate back.
        /// </summary>
        bool CanNavigateBack { get; }

        /// <summary>
        /// Occurs after navigating to a ViewModel.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be fired after <see cref="ViewModel{TParameter}.OnNavigatedFromAsync" /> in the source ViewModel,
        /// and before <see cref="ViewModel{TParameter}.OnNavigatedToAsync" /> in the target ViewModel.
        /// </remarks>
        event EventHandler<NavigatedEventArgs> Navigated;

        /// <summary>
        /// Navigates to the specified parameterless ViewModel type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        void NavigateTo<TViewModel>()
            where TViewModel : ViewModel<NoParameter>;

        /// <summary>
        /// Navigates to the specified ViewModel type using the specified argument.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TArg">The argument type.</typeparam>
        /// <param name="arg">The argument.</param>
        void NavigateTo<TViewModel, TArg>( TArg arg )
            where TViewModel : ViewModel<TArg>;

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