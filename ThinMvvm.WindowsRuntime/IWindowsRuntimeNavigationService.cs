// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using Windows.UI.Xaml.Controls;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// Windows Runtime extension of <see cref="INavigationService" />.
    /// </summary>
    public interface IWindowsRuntimeNavigationService : INavigationService
    {
        /// <summary>
        /// Binds the specified ViewModel type to the specified View type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TView">The View type.</typeparam>
        void Bind<TViewModel, TView>()
            where TView : Page;
    }
}