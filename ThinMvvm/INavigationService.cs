// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm
{
    /// <summary>
    /// Navigation service between ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a <see cref="ViewModel{NoParameter}" /> of the specified type.
        /// </summary>
        /// <typeparam name="T">The ViewModel type.</typeparam>
        void NavigateTo<T>() where T : ViewModel<NoParameter>;

        /// <summary>
        /// Navigates to a <see cref="ViewModel{TArg}"/> of the specified type, with the specified constructor argument.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TArg">The argument type.</typeparam>
        void NavigateTo<TViewModel, TArg>( TArg arg ) where TViewModel : ViewModel<TArg>;

        /// <summary>
        /// Goes back to the previous ViewModel.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Pops the ViewModel back-stack, removing the current one so that going backwards later will not go to it.
        /// </summary>
        void PopBackStack();

        /// <summary>
        /// Occurs when the service navigates to a page, forwards or backwards.
        /// </summary>
        event EventHandler<NavigatedEventArgs> Navigated;
    }
}