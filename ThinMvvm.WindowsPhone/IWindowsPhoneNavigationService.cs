// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Windows Phone specific extension of INavigationService.
    /// </summary>
    public interface IWindowsPhoneNavigationService : INavigationService
    {
        /// <summary>
        /// Binds the specified View URI to the specified ViewModel type.
        /// </summary>
        void Bind<TViewModel>( string viewUri );
    }
}