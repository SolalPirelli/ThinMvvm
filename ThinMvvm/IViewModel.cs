// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// ViewModel with an optional constructor parameter (apart from the potential dependencies) and navigation methods.
    /// </summary>
    /// <typeparam name="TParameter">The type of the ViewModel's constructor parameter, or <see cref="NoParameter" /> if it does not have one.</typeparam>
    public interface IViewModel<TParameter>
    {
        /// <summary>
        /// Called when the user navigates to the ViewModel.
        /// </summary>
        void OnNavigatedTo();

        /// <summary>
        /// Called when the user navigates from the ViewModel.
        /// </summary>
        void OnNavigatedFrom();
    }
}