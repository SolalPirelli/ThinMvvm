// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Windows Phone extension of <see cref="INavigationService" />.
    /// </summary>
    public interface IWindowsPhoneNavigationService : INavigationService
    {
        /// <summary>
        /// Binds the specified View URI to the specified ViewModel type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <param name="viewUri">The View URI.</param>
        void Bind<TViewModel>( string viewUri );
    }
}