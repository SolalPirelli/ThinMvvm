// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.SampleApp.Resources;
using ThinMvvm.WindowsPhone.SampleApp.ViewModels;

namespace ThinMvvm.WindowsPhone.SampleApp
{
    public partial class App : AppBase
    {
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
            Container.Bind<IWindowsPhoneNavigationService, WindowsPhoneNavigationService>();
            Container.Bind<ISettingsStorage, WindowsPhoneSettingsStorage>();

            Container.Bind<ISettings, Settings>();
        }


        protected override void Start( AppDependencies dependencies, AppArguments arguments )
        {
            // simple app, no additional dependencies or arguments

            dependencies.NavigationService.Bind<MainViewModel>( "/Views/MainView.xaml" );
            dependencies.NavigationService.Bind<AboutViewModel>( "/Views/AboutView.xaml" );

            dependencies.NavigationService.NavigateTo<MainViewModel, int>( 42 );
        }
    }
}