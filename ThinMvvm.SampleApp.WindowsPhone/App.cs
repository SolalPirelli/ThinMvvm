// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.Resources;
using ThinMvvm.SampleApp.Services;
using ThinMvvm.SampleApp.ViewModels;
using ThinMvvm.WindowsPhone;

namespace ThinMvvm.SampleApp.WindowsPhone
{
    public sealed class App : AppBase
    {
        protected override string Language
        {
            get { return AppResources.ResourceLanguage; }
        }

        protected override string FlowDirection
        {
            get { return AppResources.ResourceFlowDirection; }
        }

        protected override void Start( AppArguments arguments )
        {
            // simple app, no arguments necessary

            var navigationService = Container.Bind<IWindowsPhoneNavigationService, WindowsPhoneNavigationService>();
            Container.Bind<ISettingsStorage, WindowsPhoneSettingsStorage>();
            Container.Bind<ISettings, Settings>();

            navigationService.Bind<MainViewModel>( "/Views/MainView.xaml" );
            navigationService.Bind<AboutViewModel>( "/Views/AboutView.xaml" );

            navigationService.NavigateTo<MainViewModel, int>( 42 );
        }
    }
}