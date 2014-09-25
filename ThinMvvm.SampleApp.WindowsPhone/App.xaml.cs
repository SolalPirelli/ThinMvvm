// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.Logging;
using ThinMvvm.SampleApp.Services;
using ThinMvvm.SampleApp.ViewModels;
using ThinMvvm.WindowsPhone;

namespace ThinMvvm.SampleApp
{
    public sealed partial class App
    {
        protected override string Language
        {
            get { return "en-US"; }
        }

        protected override string FlowDirection
        {
            // In a real app, this would be obtained from the resources
            get { return "LeftToRight"; }
        }

        protected override void Start( AppArguments arguments )
        {
            // simple app, no arguments necessary

            var navigationService = Container.Bind<IWindowsPhoneNavigationService, WindowsPhoneNavigationService>();
            Container.Bind<ISettingsStorage, WindowsPhoneSettingsStorage>();
            Container.Bind<IDataCache, WindowsPhoneDataCache>();
            Container.Bind<INewsService, GoogleNewsService>();
            Container.Bind<ISettings, Settings>();

            Container.Bind<Logger, DebugLogger>().Start();

            navigationService.Bind<MainViewModel>( "/Views/MainView.xaml" );
            navigationService.Bind<NewsItemViewModel>( "/Views/NewsItemView.xaml" );

            navigationService.NavigateTo<MainViewModel>();
        }
    }
}