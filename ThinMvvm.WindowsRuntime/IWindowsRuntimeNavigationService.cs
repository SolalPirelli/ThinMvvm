// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using Windows.UI.Xaml.Controls;

namespace ThinMvvm.WindowsRuntime
{
    public interface IWindowsRuntimeNavigationService : INavigationService
    {
        void Bind<TViewModel, TView>()
            where TView : Page;
    }
}