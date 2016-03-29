using System;

namespace ThinMvvm
{
    /// <summary>
    /// Provides navigation between ViewModels.
    /// </summary>
    public interface INavigationService
    {
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
        /// Occurs after navigating to a ViewModel.
        /// </summary>
        event EventHandler<NavigatedEventArgs> Navigated;
    }
}