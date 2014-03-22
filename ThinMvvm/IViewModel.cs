// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// ViewModel with a constructor argument (apart from the potential dependencies) and navigation methods.
    /// </summary>
    public interface IViewModel<TArg>
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