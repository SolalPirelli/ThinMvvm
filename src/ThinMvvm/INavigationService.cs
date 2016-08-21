using System;

namespace ThinMvvm
{
    /// <summary>
    /// Provides navigation between ViewModels.
    /// </summary>
    /// <remarks>
    /// When navigating to a ViewModel, the following operations must be performed in order:
    /// 1. Create the ViewModel, if required
    /// 2. Initialize the ViewModel, if step 1 was performed
    /// 3. Restore the ViewModel's state, if navigating backwards
    /// 4. Use the ViewModel in the application, such as by assigning it as a data context
    /// 5. Trigger the Navigated event on the navigation service
    /// 6. Call OnNavigatedTo on the ViewModel
    /// 
    /// Step 4 of the above process is important since UI frameworks which support binding usually
    /// require properties to trigger events when they changed; however, many ViewModels have
    /// properties that are set once during initialization. 
    /// It would be bad UX to force users to implement change notifications on such properties.
    /// 
    /// 
    /// When navigating away from a ViewModel, the following operations must be performed in order:
    /// 1. Save the ViewModel's state, if navigating forwards
    /// 2. Call OnNavigatedFrom on the ViewModel
    /// 3. Perform navigation to the target ViewModel
    /// </remarks>
    public interface INavigationService
    {
        /// <summary>
        /// Gets a value indicating whether the navigation service can currently navigate back.
        /// </summary>
        bool CanNavigateBack { get; }

        /// <summary>
        /// Occurs after navigating to a ViewModel.
        /// </summary>
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

        /// <summary>
        /// Restores navigation state from a previous execution.
        /// </summary>
        /// <returns>True if state had to be restored and the restore was successful; false otherwise.</returns>
        bool RestorePreviousState();
    }
}