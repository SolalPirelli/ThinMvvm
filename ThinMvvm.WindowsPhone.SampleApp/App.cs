// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.SampleApp.Resources;
using ThinMvvm.WindowsPhone.SampleApp.ViewModels;

namespace ThinMvvm.WindowsPhone.SampleApp
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
            // simple app, no additional dependencies or arguments

            _navigationService.Bind<MainViewModel>( "/Views/MainView.xaml" );
            _navigationService.Bind<AboutViewModel>( "/Views/AboutView.xaml" );

            _navigationService.NavigateTo<MainViewModel, int>( 42 );
        }
    }
}