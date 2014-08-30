// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.Services;
using ThinMvvm.SampleApp.ViewModels;
using ThinMvvm.SampleApp.WindowsRuntime.Views;
using ThinMvvm.WindowsRuntime;
using Windows.ApplicationModel.Activation;

namespace ThinMvvm.SampleApp
{
    public sealed class App : AppBase
    {
        protected override void Initialize( LaunchActivatedEventArgs e )
        {
            // simple app, no arguments necessary

            var navigationService = Container.Bind<IWindowsRuntimeNavigationService, WindowsRuntimeNavigationService>();
            Container.Bind<ISettingsStorage, WindowsRuntimeSettingsStorage>();
            Container.Bind<ISettings, Settings>();

            navigationService.Bind<MainViewModel, MainView>();
            navigationService.Bind<AboutViewModel, AboutView>();

            navigationService.NavigateTo<MainViewModel, int>( 42 );
        }
    }
}