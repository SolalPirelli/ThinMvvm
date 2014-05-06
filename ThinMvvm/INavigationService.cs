// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Navigation service between ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// </summary>
        /// <typeparam name="T">The ViewModel type.</typeparam>
        void NavigateTo<T>() where T : IViewModel<NoParameter>;

        /// <summary>
        /// Navigates to a ViewModel of the specified type, with the specified constructor argument.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TArg">The argument type.</typeparam>
        void NavigateTo<TViewModel, TArg>( TArg arg ) where TViewModel : IViewModel<TArg>;

        /// <summary>
        /// Goes back to the previous ViewModel.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Pops the ViewModel back-stack, removing the current one so that going backwards will not go to it.
        /// </summary>
        void PopBackStack();
    }
}