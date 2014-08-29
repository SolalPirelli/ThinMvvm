// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.Resources;
using ThinMvvm.SampleApp.WindowsPhone.Services;
using ThinMvvm.SampleApp.WindowsPhone.ViewModels;
using ThinMvvm.WindowsPhone;

namespace ThinMvvm.SampleApp.WindowsPhone
{
    public sealed class App : AppBase
    {
        private readonly IWindowsPhoneNavigationService _navigationService;

        protected override string Language
        {
            get { return AppResources.ResourceLanguage; }
        }

        protected override string FlowDirection
        {
            get { return AppResources.ResourceFlowDirection; }
        }


        public App()
        {
            _navigationService = Container.Bind<IWindowsPhoneNavigationService, WindowsPhoneNavigationService>();
            Container.Bind<ISettingsStorage, WindowsPhoneSettingsStorage>();

            Container.Bind<ISettings, Settings>();
        }


        protected override void Start( AppArguments arguments )
        {
            // simple app, no arguments necessary

            _navigationService.Bind<MainViewModel>( "/Views/MainView.xaml" );
            _navigationService.Bind<AboutViewModel>( "/Views/AboutView.xaml" );

            _navigationService.NavigateTo<MainViewModel, int>( 42 );
        }
    }
}