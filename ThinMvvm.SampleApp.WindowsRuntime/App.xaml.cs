// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.Services;
using ThinMvvm.SampleApp.ViewModels;
using ThinMvvm.SampleApp.WindowsRuntime.Views;
using ThinMvvm.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;

namespace ThinMvvm.SampleApp.WindowsRuntime
{
    public sealed partial class App : AppBase
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void Launch( LaunchActivatedEventArgs e )
        {
            // simple app, no arguments necessary

            var navigationService = Container.Bind<IWindowsRuntimeNavigationService, WindowsRuntimeNavigationService>();
            Container.Bind<ISettingsStorage, WindowsRuntimeSettingsStorage>();
            Container.Bind<IDataCache, WindowsRuntimeDataCache>();
            Container.Bind<INewsService, GoogleNewsService>();
            Container.Bind<ISettings, Settings>();

            navigationService.Bind<MainViewModel, MainView>();
            navigationService.Bind<NewsItemViewModel, NewsItemView>();

            // handle the back button since WP doesn't do it for us
            HardwareButtons.BackPressed += ( _, e2 ) =>
            {
                e2.Handled = true;
                navigationService.NavigateBack();
            };

            navigationService.NavigateTo<MainViewModel>();
        }
    }
}